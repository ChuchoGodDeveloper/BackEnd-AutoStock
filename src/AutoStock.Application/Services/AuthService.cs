using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;

namespace AutoStock.Application.Services;

public class AuthService : IAuthService
{
    private const int MaximoIntentosFallidos = 5;
    private const int MinutosBloqueo = 15;
    private const int HorasVigenciaSesion = 8;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISesionRepository _sesionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ISesionRepository sesionRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _sesionRepository = sesionRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Identificador) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationAppException("El identificador y la contraseña son obligatorios");
        }

        var identificador = request.Identificador.Trim().ToLower();
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
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = token,
            ExpiraEn = expiresAt,
            Usuario = new UsuarioLoginDto
            {
                Id = usuario.Id,
                EmpleadoId = usuario.EmpleadoId,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = usuario.Rol.Nombre,
            },
        };
    }

    public async Task LogoutAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw new ValidationAppException("El identificador de sesión es obligatorio");
        }

        var sesion = await _sesionRepository.GetByJtiAsync(jti);

        if (sesion is not null && sesion.Activa)
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

        return new PerfilResponseDto
        {
            Id = usuario.Id,
            EmpleadoId = usuario.EmpleadoId,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            Rol = usuario.Rol.Nombre,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
        };
    }
}
