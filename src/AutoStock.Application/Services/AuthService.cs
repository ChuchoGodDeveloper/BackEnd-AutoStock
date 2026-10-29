using System.Security.Cryptography;
using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Constants;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;

namespace AutoStock.Application.Services;

public class AuthService : IAuthService
{
    private const int MaximoIntentosFallidos = 5;
    private const int MinutosBloqueo = 15;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISesionRepository _sesionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenRecuperacionRepository? _tokenRecuperacionRepository;
    private readonly IEmailService? _emailService;
    private readonly IPermisosService? _permisosService;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ISesionRepository sesionRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        ITokenRecuperacionRepository? tokenRecuperacionRepository,
        IEmailService? emailService,
        IPermisosService? permisosService = null)
    {
        _usuarioRepository = usuarioRepository;
        _sesionRepository = sesionRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _tokenRecuperacionRepository = tokenRecuperacionRepository;
        _emailService = emailService;
        _permisosService = permisosService;
    }

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ISesionRepository sesionRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IPermisosService? permisosService = null)
        : this(
            usuarioRepository,
            sesionRepository,
            passwordHasher,
            jwtTokenGenerator,
            unitOfWork,
            null,
            null,
            permisosService)
    {
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Identificador) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationAppException("El identificador y la contraseña son obligatorios");
        }

        var identificador = request.Identificador.Trim().ToLowerInvariant();
        var usuario = await _usuarioRepository.GetByIdentifierAsync(identificador);

        if (usuario is null)
        {
            throw new AuthenticationFailedException("ID de empleado o contraseña incorrectos");
        }

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.UtcNow)
        {
            throw new AccountLockedException(
                "La cuenta está bloqueada temporalmente por múltiples intentos fallidos. Intenta de nuevo en 15 minutos.");
        }

        if (usuario.BloqueadoHasta.HasValue)
        {
            usuario.BloqueadoHasta = null;
            usuario.IntentosFallidos = 0;
            _usuarioRepository.Update(usuario);
        }

        if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= MaximoIntentosFallidos)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
            }

            _usuarioRepository.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            throw new AuthenticationFailedException("ID de empleado o contraseña incorrectos");
        }

        if (!usuario.Activo)
        {
            throw new AccountDisabledException("Tu cuenta está desactivada. Contacta al administrador");
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        _usuarioRepository.Update(usuario);

        var (token, jti, expiresAt) = _jwtTokenGenerator.GenerateToken(usuario);

        var sesion = new Sesion
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            TokenJti = jti,
            FechaInicio = DateTime.UtcNow,
            FechaExpiracion = expiresAt,
            Activa = true,
        };

        await _sesionRepository.AddAsync(sesion);
        await _unitOfWork.SaveChangesAsync();

        var permisos = await ObtenerPermisosAsync(usuario.Rol.Id, usuario.Rol.Nombre);

        return new LoginResponseDto
        {
            Token = token,
            ExpiraEn = expiresAt,
            RequiereCambioPassword = usuario.RequiereCambioPassword,
            Usuario = new UsuarioLoginDto
            {
                Id = usuario.Id,
                EmpleadoId = usuario.EmpleadoId,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                FechaNacimiento = usuario.FechaNacimiento,
                Rol = usuario.Rol.Nombre,
                Permisos = permisos,
            },
        };
    }

    public async Task LogoutAsync(string jti)
    {
        var sesion = await _sesionRepository.GetByJtiAsync(jti);
        if (sesion is not null)
        {
            sesion.Activa = false;
            sesion.FechaCierre = DateTime.UtcNow;
            _sesionRepository.Update(sesion);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<PerfilResponseDto> GetPerfilActualAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario is null)
        {
            throw new AuthenticationFailedException("Usuario no encontrado");
        }

        var permisos = await ObtenerPermisosAsync(usuario.Rol.Id, usuario.Rol.Nombre);

        return new PerfilResponseDto
        {
            Id = usuario.Id,
            EmpleadoId = usuario.EmpleadoId,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            Rol = usuario.Rol.Nombre,
            Permisos = permisos,
            FechaNacimiento = usuario.FechaNacimiento,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
        };
    }

    public async Task CambiarPasswordPrimerAccesoAsync(CambiarPasswordDto dto, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(dto.PasswordActual) || string.IsNullOrWhiteSpace(dto.PasswordNueva))
        {
            throw new ValidationAppException("La contraseña actual y la nueva son obligatorias");
        }

        if (dto.PasswordNueva.Length < 8 ||
            !dto.PasswordNueva.Any(char.IsLetter) ||
            !dto.PasswordNueva.Any(char.IsDigit))
        {
            throw new ValidationAppException("La nueva contraseña debe tener al menos 8 caracteres, con letras y números");
        }

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario is null)
        {
            throw new AuthenticationFailedException("Usuario no encontrado");
        }

        if (!_passwordHasher.Verify(dto.PasswordActual, usuario.PasswordHash))
        {
            throw new ValidationAppException("La contraseña actual no es correcta");
        }

        usuario.PasswordHash = _passwordHasher.Hash(dto.PasswordNueva);
        usuario.RequiereCambioPassword = false;
        usuario.IntentosFallidos = 0;
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<SolicitarRecuperacionResponseDto> SolicitarRecuperacionPasswordAsync(
        SolicitarRecuperacionDto dto,
        string? baseUrl = null)
    {
        const string mensajeEstandar = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña";

        if (string.IsNullOrWhiteSpace(dto.Correo))
        {
            throw new ValidationAppException("El correo electrónico es obligatorio");
        }

        var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();

        if (!EsFormatoCorreoValido(correoNormalizado))
        {
            throw new ValidationAppException("Ingresa un correo electrónico válido");
        }

        if (_tokenRecuperacionRepository is not null)
        {
            var solicitudesRecientes = await _tokenRecuperacionRepository
                .ContarSolicitudesRecientesPorCorreoAsync(correoNormalizado, DateTime.UtcNow.AddHours(-1));

            if (solicitudesRecientes >= 3)
            {
                throw new ValidationAppException(
                    "Has alcanzado el límite de 3 solicitudes por hora para este correo. Intenta más tarde.");
            }
        }

        var usuario = await _usuarioRepository.GetByIdentifierAsync(correoNormalizado);

        // CA 3: Si no existe o no está activo, retornamos mensaje estándar sin enviar correo (anti-enumeración)
        if (usuario is null || !usuario.Activo)
        {
            return new SolicitarRecuperacionResponseDto
            {
                Mensaje = mensajeEstandar,
            };
        }

        if (_tokenRecuperacionRepository is not null)
        {
            // Regla de negocio: Invalida enlaces previos al pedir uno nuevo
            var tokensActivos = await _tokenRecuperacionRepository.ObtenerActivosPorUsuarioIdAsync(usuario.Id);
            foreach (var tokenActivo in tokensActivos)
            {
                tokenActivo.Usado = true;
                tokenActivo.FechaUso = DateTime.UtcNow;
                _tokenRecuperacionRepository.Actualizar(tokenActivo);
            }

            var tokenSeguro = Guid.NewGuid().ToString("N") +
                Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            var ahora = DateTime.UtcNow;

            var nuevoToken = new TokenRecuperacionPassword
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                Token = tokenSeguro,
                Correo = correoNormalizado,
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddMinutes(30), // Vigencia de 30 minutos
                Usado = false,
            };

            await _tokenRecuperacionRepository.AgregarAsync(nuevoToken);
            await _unitOfWork.SaveChangesAsync();

            var baseFinal = string.IsNullOrWhiteSpace(baseUrl)
                ? "http://localhost:5173"
                : baseUrl.TrimEnd('/');
            var enlace = $"{baseFinal}/restablecer-password?token={tokenSeguro}";

            if (_emailService is not null)
            {
                await _emailService.EnviarEmailRecuperacionPasswordAsync(
                    usuario.Correo,
                    usuario.NombreCompleto,
                    enlace);
            }

            return new SolicitarRecuperacionResponseDto
            {
                Mensaje = mensajeEstandar,
                TokenDev = tokenSeguro,
            };
        }

        return new SolicitarRecuperacionResponseDto
        {
            Mensaje = mensajeEstandar,
        };
    }

    public async Task<ValidarTokenRecuperacionResponseDto> ValidarTokenRecuperacionAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || _tokenRecuperacionRepository is null)
        {
            return new ValidarTokenRecuperacionResponseDto
            {
                EsValido = false,
                Mensaje = "El enlace ya no es válido",
            };
        }

        var tokenDb = await _tokenRecuperacionRepository.ObtenerPorTokenAsync(token.Trim());
        if (tokenDb is null || tokenDb.Usado || tokenDb.FechaExpiracion < DateTime.UtcNow)
        {
            return new ValidarTokenRecuperacionResponseDto
            {
                EsValido = false,
                Mensaje = "El enlace ya no es válido",
            };
        }

        if (tokenDb.Usuario is not null && !tokenDb.Usuario.Activo)
        {
            return new ValidarTokenRecuperacionResponseDto
            {
                EsValido = false,
                Mensaje = "El enlace ya no es válido",
            };
        }

        return new ValidarTokenRecuperacionResponseDto
        {
            EsValido = true,
            Mensaje = "Enlace válido",
            CorreoEnmascarado = EnmascararCorreo(tokenDb.Correo),
        };
    }

    public async Task RestablecerPasswordAsync(RestablecerPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || _tokenRecuperacionRepository is null)
        {
            throw new ValidationAppException("El enlace ya no es válido");
        }

        if (string.IsNullOrWhiteSpace(dto.PasswordNueva) || string.IsNullOrWhiteSpace(dto.ConfirmarPassword))
        {
            throw new ValidationAppException("La nueva contraseña y su confirmación son obligatorias");
        }

        if (dto.PasswordNueva != dto.ConfirmarPassword)
        {
            throw new ValidationAppException("Las contraseñas no coinciden");
        }

        if (dto.PasswordNueva.Length < 8 ||
            !dto.PasswordNueva.Any(char.IsLetter) ||
            !dto.PasswordNueva.Any(char.IsDigit))
        {
            throw new ValidationAppException("La nueva contraseña debe tener al menos 8 caracteres, con letras y números");
        }

        var tokenDb = await _tokenRecuperacionRepository.ObtenerPorTokenAsync(dto.Token.Trim());
        if (tokenDb is null || tokenDb.Usado || tokenDb.FechaExpiracion < DateTime.UtcNow)
        {
            throw new ValidationAppException("El enlace ya no es válido");
        }

        var usuario = tokenDb.Usuario ?? await _usuarioRepository.GetByIdAsync(tokenDb.UsuarioId);
        if (usuario is null || !usuario.Activo)
        {
            throw new ValidationAppException("El enlace ya no es válido");
        }

        usuario.PasswordHash = _passwordHasher.Hash(dto.PasswordNueva);

        // Regla de negocio: Al restablecer la contraseña se levanta el bloqueo temporal
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.RequiereCambioPassword = false;

        _usuarioRepository.Update(usuario);

        tokenDb.Usado = true;
        tokenDb.FechaUso = DateTime.UtcNow;
        _tokenRecuperacionRepository.Actualizar(tokenDb);

        var otrosTokens = await _tokenRecuperacionRepository.ObtenerActivosPorUsuarioIdAsync(usuario.Id);
        foreach (var otro in otrosTokens)
        {
            if (otro.Id != tokenDb.Id)
            {
                otro.Usado = true;
                otro.FechaUso = DateTime.UtcNow;
                _tokenRecuperacionRepository.Actualizar(otro);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static string EnmascararCorreo(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
        {
            return correo;
        }

        var partes = correo.Split('@');
        var nombre = partes[0];
        var dominio = partes[1];

        if (nombre.Length <= 2)
        {
            return $"{nombre[0]}***@{dominio}";
        }

        return $"{nombre[0]}***{nombre[^1]}@{dominio}";
    }

    private static bool EsFormatoCorreoValido(string correo)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(correo);
            return addr.Address == correo && correo.Contains('.') && !correo.EndsWith('.');
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<string>> ObtenerPermisosAsync(int rolId, string nombreRol)
    {
        if (_permisosService is not null)
        {
            return await _permisosService.ObtenerCodigosPermisosActivosPorRolAsync(rolId, nombreRol);
        }

        return CodigosPermiso.TodosLosCodigos
            .Where(c => MatrizPermisosBase.EsPermisoActivoBase(nombreRol, c))
            .ToList();
    }
}
