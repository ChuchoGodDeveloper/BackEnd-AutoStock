using System.Security.Cryptography;
using System.Text;
using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
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
        if (string.IsNullOrWhiteSpace(dto.EmpleadoId) ||
            string.IsNullOrWhiteSpace(dto.Correo) ||
            string.IsNullOrWhiteSpace(dto.NombreCompleto))
        {
            throw new ValidationAppException("El ID de empleado, nombre completo y correo son obligatorios");
        }

        var empleadoId = dto.EmpleadoId.Trim();
        var correo = dto.Correo.Trim();

        var yaExiste = await _usuarioRepository.ExisteEmpleadoIdOCorreoAsync(empleadoId, correo);

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
            EmpleadoId = empleadoId,
            Correo = correo,
            NombreCompleto = dto.NombreCompleto.Trim(),
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
                Rol = u.Rol.Nombre,
                Activo = u.Activo,
            })
            .ToList();
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
