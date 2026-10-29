using AutoStock.Domain.Constants;
using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Application.Services;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using Moq;

namespace AutoStock.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<ISesionRepository> _sesionRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(
            _usuarioRepositoryMock.Object,
            _sesionRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Rol RolAdministrador() => new()
    {
        Id = 1,
        Nombre = "Administrador",
        Descripcion = "Acceso total al sistema",
    };

    private static Usuario UsuarioActivoAdministrador() => new()
    {
        Id = 1,
        EmpleadoId = "EMP-00001",
        Correo = "admin@autostock.com",
        NombreCompleto = "Carlos Ramírez",
        PasswordHash = "hash-valido",
        Activo = true,
        IntentosFallidos = 0,
        BloqueadoHasta = null,
        RolId = 1,
        Rol = RolAdministrador(),
        FechaCreacion = DateTime.UtcNow,
    };

    private void ConfigurarJwt(Usuario usuario)
    {
        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<Usuario>()))
            .Returns(("token-jwt-falso", "jti-123", DateTime.UtcNow.AddHours(8)));
    }

    [Fact]
    public async Task Login_ConEmpleadoIdYPasswordCorrectos_RetornaTokenRolYNombre()
    {
        var usuario = UsuarioActivoAdministrador();
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-00001"))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify("AdminAutoStock2026!", "hash-valido"))
            .Returns(true);
        ConfigurarJwt(usuario);

        var resultado = await _authService.LoginAsync(new LoginRequestDto
        {
            Identificador = "EMP-00001",
            Password = "AdminAutoStock2026!",
        });

        Assert.Equal("token-jwt-falso", resultado.Token);
        Assert.Equal("Administrador", resultado.Usuario.Rol);
        Assert.Equal("Carlos Ramírez", resultado.Usuario.NombreCompleto);
        Assert.Equal("EMP-00001", resultado.Usuario.EmpleadoId);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        _sesionRepositoryMock.Verify(
            s => s.AddAsync(It.Is<Sesion>(x => x.Activa && x.TokenJti == "jti-123")),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ConCorreoYPasswordCorrectos_RetornaToken()
    {
        var usuario = UsuarioActivoAdministrador();
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("admin@autostock.com"))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify("AdminAutoStock2026!", "hash-valido"))
            .Returns(true);
        ConfigurarJwt(usuario);

        var resultado = await _authService.LoginAsync(new LoginRequestDto
        {
            Identificador = "admin@autostock.com",
            Password = "AdminAutoStock2026!",
        });

        Assert.Equal("token-jwt-falso", resultado.Token);
        Assert.Equal("Administrador", resultado.Usuario.Rol);
        Assert.Equal("admin@autostock.com", resultado.Usuario.Correo);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_RetornaMensajeExactoEIncrementaIntentos()
    {
        var usuario = UsuarioActivoAdministrador();
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-00001"))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify("PasswordIncorrecta!", "hash-valido"))
            .Returns(false);

        var excepcion = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => _authService.LoginAsync(new LoginRequestDto
            {
                Identificador = "EMP-00001",
                Password = "PasswordIncorrecta!",
            }));

        Assert.Equal("ID de empleado o contraseña incorrectos", excepcion.Message);
        Assert.Equal(1, usuario.IntentosFallidos);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ConUsuarioInexistente_RetornaMensajeExacto()
    {
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-99999"))
            .ReturnsAsync((Usuario?)null);

        var excepcion = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => _authService.LoginAsync(new LoginRequestDto
            {
                Identificador = "EMP-99999",
                Password = "CualquierPassword!",
            }));

        Assert.Equal("ID de empleado o contraseña incorrectos", excepcion.Message);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_CuatroIntentosFallidosMasUno_BloqueaCuentaPor15Minutos()
    {
        var usuario = UsuarioActivoAdministrador();
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-00001"))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), "hash-valido"))
            .Returns(false);

        for (var intento = 1; intento <= 4; intento++)
        {
            await Assert.ThrowsAsync<AuthenticationFailedException>(
                () => _authService.LoginAsync(new LoginRequestDto
                {
                    Identificador = "EMP-00001",
                    Password = "PasswordIncorrecta!",
                }));

            Assert.Null(usuario.BloqueadoHasta);
        }

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => _authService.LoginAsync(new LoginRequestDto
            {
                Identificador = "EMP-00001",
                Password = "PasswordIncorrecta!",
            }));

        Assert.Equal(5, usuario.IntentosFallidos);
        Assert.NotNull(usuario.BloqueadoHasta);
        Assert.True(usuario.BloqueadoHasta > DateTime.UtcNow);
        Assert.True(usuario.BloqueadoHasta <= DateTime.UtcNow.AddMinutes(15));
    }

    [Fact]
    public async Task Login_CuentaBloqueadaVigente_RechazaAccesoConPasswordCorrecta()
    {
        var usuario = UsuarioActivoAdministrador();
        usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(10);
        usuario.IntentosFallidos = 5;

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-00001"))
            .ReturnsAsync(usuario);

        var excepcion = await Assert.ThrowsAsync<AccountLockedException>(
            () => _authService.LoginAsync(new LoginRequestDto
            {
                Identificador = "EMP-00001",
                Password = "AdminAutoStock2026!",
            }));

        Assert.Contains("La cuenta está bloqueada temporalmente", excepcion.Message);
        _passwordHasherMock.Verify(
            h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_UsuarioDesactivadoConPasswordCorrecta_RetornaMensajeExacto()
    {
        var usuario = UsuarioActivoAdministrador();
        usuario.Activo = false;

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-00999"))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify("Inactivo2026!", "hash-valido"))
            .Returns(true);

        var excepcion = await Assert.ThrowsAsync<AccountDisabledException>(
            () => _authService.LoginAsync(new LoginRequestDto
            {
                Identificador = "EMP-00999",
                Password = "Inactivo2026!",
            }));

        Assert.Equal("Tu cuenta está desactivada. Contacta al administrador", excepcion.Message);
        _sesionRepositoryMock.Verify(
            s => s.AddAsync(It.IsAny<Sesion>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", "AdminAutoStock2026!")]
    [InlineData("   ", "AdminAutoStock2026!")]
    [InlineData("EMP-00001", "")]
    [InlineData("EMP-00001", "   ")]
    public async Task Login_CamposVacios_LanzaErrorDeValidacionSinConsultarSesiones(
        string identificador,
        string password)
    {
        var excepcion = await Assert.ThrowsAsync<ValidationAppException>(
            () => _authService.LoginAsync(new LoginRequestDto
            {
                Identificador = identificador,
                Password = password,
            }));

        Assert.Equal("El identificador y la contraseña son obligatorios", excepcion.Message);
        _usuarioRepositoryMock.Verify(
            r => r.GetByIdentifierAsync(It.IsAny<string>()),
            Times.Never);
        _sesionRepositoryMock.Verify(
            s => s.AddAsync(It.IsAny<Sesion>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Logout_MarcaSesionInactivaYAsignaFechaCierre()
    {
        var sesion = new Sesion
        {
            Id = Guid.NewGuid(),
            UsuarioId = 1,
            TokenJti = "jti-123",
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaExpiracion = DateTime.UtcNow.AddHours(7),
            Activa = true,
            FechaCierre = null,
        };

        _sesionRepositoryMock
            .Setup(r => r.GetByJtiAsync("jti-123"))
            .ReturnsAsync(sesion);

        await _authService.LogoutAsync("jti-123");

        Assert.False(sesion.Activa);
        Assert.NotNull(sesion.FechaCierre);
        Assert.True(sesion.FechaCierre <= DateTime.UtcNow);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_UsuarioNuevo_DebeCambiarPassword_DevuelveRequiereCambioPasswordTrue()
    {
        var usuario = UsuarioActivoAdministrador();
        usuario.RequiereCambioPassword = true;

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdentifierAsync("emp-00001"))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify("TempPassword2026", "hash-valido"))
            .Returns(true);
        ConfigurarJwt(usuario);

        var resultado = await _authService.LoginAsync(new LoginRequestDto
        {
            Identificador = "EMP-00001",
            Password = "TempPassword2026",
        });

        Assert.True(resultado.RequiereCambioPassword);
        Assert.Equal("token-jwt-falso", resultado.Token);
    }

    [Fact]
    public async Task CambiarPasswordPrimerAcceso_ConPasswordActualValida_ActualizaHashYQuitaRequiereCambio()
    {
        var usuario = UsuarioActivoAdministrador();
        usuario.RequiereCambioPassword = true;
        usuario.IntentosFallidos = 3;

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(usuario);
        _passwordHasherMock
            .Setup(h => h.Verify("Temporal2026", "hash-valido"))
            .Returns(true);
        _passwordHasherMock
            .Setup(h => h.Hash("NuevaClave2026"))
            .Returns("hash-nuevo");

        await _authService.CambiarPasswordPrimerAccesoAsync(new CambiarPasswordDto
        {
            PasswordActual = "Temporal2026",
            PasswordNueva = "NuevaClave2026",
        }, usuarioId: 1);

        Assert.Equal("hash-nuevo", usuario.PasswordHash);
        Assert.False(usuario.RequiereCambioPassword);
        Assert.Equal(0, usuario.IntentosFallidos);
        _usuarioRepositoryMock.Verify(r => r.Update(usuario), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPerfilActualAsync_ConUsuarioExistente_RetornaPerfilConRolYDatosCompletos()
    {
        var usuario = UsuarioActivoAdministrador();
        usuario.FechaNacimiento = new DateOnly(1995, 5, 20);

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(usuario);

        var perfil = await _authService.GetPerfilActualAsync(1);

        Assert.NotNull(perfil);
        Assert.Equal("Carlos Ramírez", perfil.NombreCompleto);
        Assert.Equal("Administrador", perfil.Rol);
        Assert.Equal(new DateOnly(1995, 5, 20), perfil.FechaNacimiento);
        Assert.Equal("admin@autostock.com", perfil.Correo);
        Assert.NotEmpty(perfil.Permisos);
        Assert.Contains(CodigosPermiso.ConsultarCatalogo, perfil.Permisos);
    }
}
