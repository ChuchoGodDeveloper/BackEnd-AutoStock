using System.Security.Cryptography;
using System.Text;
using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Helpers;
using AutoStock.Domain.Interfaces;

namespace AutoStock.Application.Services;

public class UsuariosService : IUsuariosService
{
    private const int LongitudPasswordTemporal = 12;
    private const string LetrasPassword = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz";
    private const string NumerosPassword = "23456789";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public UsuariosService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioCreadoDto> CrearUsuarioAsync(CrearUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Correo) ||
            string.IsNullOrWhiteSpace(dto.NombreCompleto))
        {
            throw new ValidationAppException("El ID de empleado, nombre completo y correo son obligatorios");
        }

        var nombreCompleto = dto.NombreCompleto.Trim();
        var correo = dto.Correo.Trim();

        if (nombreCompleto.Length < 3 || nombreCompleto.Length > 150)
        {
            throw new ValidationAppException("El nombre completo debe tener entre 3 y 150 caracteres");
        }

        if (correo.Length > 100)
        {
            throw new ValidationAppException("El correo electrónico no puede exceder 100 caracteres");
        }

        if (dto.FechaNacimiento == default)
        {
            throw new ValidationAppException("La fecha de nacimiento es obligatoria");
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var edad = hoy.Year - dto.FechaNacimiento.Year;
        if (dto.FechaNacimiento > hoy.AddYears(-edad))
        {
            edad--;
        }

        if (edad < 18)
        {
            throw new ValidationAppException("El usuario debe tener al menos 18 años de edad");
        }

        if (edad > 99 || dto.FechaNacimiento.Year < 1920)
        {
            throw new ValidationAppException("La fecha de nacimiento no es válida");
        }

        string empleadoIdFinal;

        if (!string.IsNullOrWhiteSpace(dto.EmpleadoId))
        {
            var empleadoIdManual = dto.EmpleadoId.Trim();
            if (empleadoIdManual.Length > 20)
            {
                throw new ValidationAppException("El ID de empleado no puede exceder 20 caracteres");
            }

            empleadoIdFinal = empleadoIdManual;
        }
        else
        {
            var baseId = GeneradorIdentificadorUsuario.GenerarBase(nombreCompleto, dto.FechaNacimiento);
            var consecutivo = 1;
            var candidato = GeneradorIdentificadorUsuario.GenerarConConsecutivo(baseId, consecutivo);

            while (await _usuarioRepository.ExisteEmpleadoIdAsync(candidato) && consecutivo < 99)
            {
                consecutivo++;
                candidato = GeneradorIdentificadorUsuario.GenerarConConsecutivo(baseId, consecutivo);
            }

            empleadoIdFinal = candidato;
        }

        var yaExiste = await _usuarioRepository.ExisteEmpleadoIdOCorreoAsync(empleadoIdFinal, correo);

        if (yaExiste)
        {
            throw new ValidationAppException("Ya existe un usuario con ese ID de empleado o correo");
        }

        var rol = await _rolRepository.GetByIdAsync(dto.RolId);

        if (rol is null)
        {
            throw new ValidationAppException("El rol indicado no existe");
        }

        var passwordTemporal = GenerarPasswordTemporal();

        var usuario = new Usuario
        {
            EmpleadoId = empleadoIdFinal,
            Correo = correo,
            NombreCompleto = nombreCompleto,
            FechaNacimiento = dto.FechaNacimiento,
            PasswordHash = _passwordHasher.Hash(passwordTemporal),
            Activo = true,
            RequiereCambioPassword = true,
            IntentosFallidos = 0,
            BloqueadoHasta = null,
            RolId = rol.Id,
            Rol = rol,
            FechaCreacion = DateTime.UtcNow,
        };

        await _usuarioRepository.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return new UsuarioCreadoDto
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            EmpleadoId = usuario.EmpleadoId,
            Correo = usuario.Correo,
            FechaNacimiento = usuario.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            Activo = usuario.Activo,
            RequiereCambioPassword = usuario.RequiereCambioPassword,
            PasswordTemporal = passwordTemporal,
        };
    }

    public async Task<IReadOnlyList<UsuarioListaDto>> ObtenerUsuariosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();

        return usuarios
            .Select(u => new UsuarioListaDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                EmpleadoId = u.EmpleadoId,
                Correo = u.Correo,
                FechaNacimiento = u.FechaNacimiento,
                Rol = u.Rol.Nombre,
                Activo = u.Activo,
            })
            .ToList();
    }

    public async Task<UsuarioDetalleAdminDto> ObtenerUsuarioPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new NotFoundException("Usuario no encontrado");
        }

        return new UsuarioDetalleAdminDto
        {
            Id = usuario.Id,
            EmpleadoId = usuario.EmpleadoId,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            FechaNacimiento = usuario.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            RolId = usuario.RolId,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            RequiereCambioPassword = usuario.RequiereCambioPassword,
        };
    }

    public async Task<UsuarioListaDto> EditarUsuarioAsync(int id, EditarUsuarioDto dto, int administradorAutenticadoId)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto) || string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrWhiteSpace(dto.Rol))
        {
            throw new ValidationAppException("El nombre completo, correo y rol son obligatorios");
        }

        var nombreCompleto = dto.NombreCompleto.Trim();
        var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();

        if (nombreCompleto.Length < 3 || nombreCompleto.Length > 150)
        {
            throw new ValidationAppException("El nombre completo debe tener entre 3 y 150 caracteres");
        }

        if (correoNormalizado.Length > 100)
        {
            throw new ValidationAppException("El correo electrónico no puede exceder 100 caracteres");
        }

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new NotFoundException("Usuario no encontrado");
        }

        var rolLimpio = dto.Rol.Trim();
        var roles = await _rolRepository.GetAllAsync();
        var rol = roles.FirstOrDefault(r =>
            string.Equals(r.Nombre, rolLimpio, StringComparison.OrdinalIgnoreCase) ||
            (int.TryParse(rolLimpio, out var parsedRolId) && r.Id == parsedRolId));

        if (rol is null)
        {
            throw new ValidationAppException("El rol especificado no es válido");
        }

        // CA 7: El propio Administrador no puede cambiar su rol ni desactivarse
        if (id == administradorAutenticadoId)
        {
            var esAdminActual = string.Equals(usuario.Rol.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase);
            var seraAdminNuevo = string.Equals(rol.Nombre, "Administrador", StringComparison.OrdinalIgnoreCase);

            if (esAdminActual && !seraAdminNuevo)
            {
                throw new ValidationAppException("No puedes desactivar tu cuenta ni cambiar tu propio rol");
            }
        }

        // CA 3: Si el correo ya está registrado en otro usuario
        var correoEnUso = await _usuarioRepository.ExisteCorreoEnOtroUsuarioAsync(id, correoNormalizado);
        if (correoEnUso)
        {
            throw new ValidationAppException("Ya existe un usuario con ese correo");
        }

        usuario.NombreCompleto = nombreCompleto;
        usuario.Correo = correoNormalizado;
        usuario.RolId = rol.Id;
        usuario.Rol = rol;

        if (dto.FechaNacimiento.HasValue && dto.FechaNacimiento.Value != default)
        {
            usuario.FechaNacimiento = dto.FechaNacimiento.Value;
        }

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync();

        return new UsuarioListaDto
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            EmpleadoId = usuario.EmpleadoId,
            Correo = usuario.Correo,
            FechaNacimiento = usuario.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            Activo = usuario.Activo,
        };
    }

    public async Task<UsuarioListaDto> CambiarEstatusUsuarioAsync(int id, CambiarEstatusUsuarioDto dto, int administradorAutenticadoId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new NotFoundException("Usuario no encontrado");
        }

        // CA 7: No puedes desactivar tu propia cuenta
        if (id == administradorAutenticadoId && !dto.Activo)
        {
            throw new ValidationAppException("No puedes desactivar tu cuenta ni cambiar tu propio rol");
        }

        usuario.Activo = dto.Activo;
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync();

        return new UsuarioListaDto
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            EmpleadoId = usuario.EmpleadoId,
            Correo = usuario.Correo,
            FechaNacimiento = usuario.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            Activo = usuario.Activo,
        };
    }

    public async Task<PasswordTemporalRestablecidaDto> RestablecerPasswordPorAdminAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new NotFoundException("Usuario no encontrado");
        }

        var passwordTemporal = GenerarPasswordTemporal();

        usuario.PasswordHash = _passwordHasher.Hash(passwordTemporal);
        usuario.RequiereCambioPassword = true;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;

        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync();

        return new PasswordTemporalRestablecidaDto
        {
            Id = usuario.Id,
            EmpleadoId = usuario.EmpleadoId,
            NombreCompleto = usuario.NombreCompleto,
            PasswordTemporal = passwordTemporal,
        };
    }

    private static string GenerarPasswordTemporal()
    {
        var caracteres = LetrasPassword + NumerosPassword;
        var password = new StringBuilder(LongitudPasswordTemporal);

        for (var indice = 0; indice < LongitudPasswordTemporal; indice++)
        {
            password.Append(caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)]);
        }

        password[0] = LetrasPassword[RandomNumberGenerator.GetInt32(LetrasPassword.Length)];
        password[1] = NumerosPassword[RandomNumberGenerator.GetInt32(NumerosPassword.Length)];

        return password.ToString();
    }
}
