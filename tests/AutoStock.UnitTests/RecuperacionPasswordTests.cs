using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Application.Services;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using Moq;

namespace AutoStock.UnitTests;

public class RecuperacionPasswordTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<ISesionRepository> _sesionRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITokenRecuperacionRepository> _tokenRepositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly AuthService _authService;

    public RecuperacionPasswordTests()
    {
        _authService = new AuthService(
            _usuarioRepositoryMock.Object,
            _sesionRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _unitOfWorkMock.Object,
            _tokenRepositoryMock.Object,
            _emailServiceMock.Object);
    }

    private static Usuario CrearUsuarioPrueba(bool activo = true)
    {
        return new Usuario
        {
            Id = 10,
            EmpleadoId = "EMP-00010",
            NombreCompleto = "José de Jesús López",
            Correo = "chucho@autostock.mx",
            PasswordHash = "hash-antiguo",
            Activo = activo,
            IntentosFallidos = 5,
            BloqueadoHasta = DateTime.UtcNow.AddMinutes(15),
            RequiereCambioPassword = true,
            RolId = 4,
            Rol = new Rol { Id = 4, Nombre = "Asesor de Ventas" },
        };
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("correo-sin-arroba")]
    [InlineData("correo@dominio")]
    public async Task SolicitarRecuperacion_CorreoInvalido_LanzaValidationAppException(string correoInvalido)
    {
        var dto = new SolicitarRecuperacionDto { Correo = correoInvalido };

        await Assert.ThrowsAsync<ValidationAppException>(
            () => _authService.SolicitarRecuperacionPasswordAsync(dto));

        _emailServiceMock.Verify(
            e => e.EnviarEmailRecuperacionPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SolicitarRecuperacion_ExcedeTresPorHora_LanzaValidationAppException()
    {
        var dto = new SolicitarRecuperacionDto { Correo = "chucho@autostock.mx" };

        _tokenRepositoryMock
            .Setup(r => r.ContarSolicitudesRecientesPorCorreoAsync("chucho@autostock.mx", It.IsAny<DateTime>()))
            .ReturnsAsync(3);

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _authService.SolicitarRecuperacionPasswordAsync(dto));

        Assert.Contains("límite de 3 solicitudes por hora", ex.Message);
        _emailServiceMock.Verify(
            e => e.EnviarEmailRecuperacionPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SolicitarRecuperacion_UsuarioInexistente_RetornaMensajeEstandarSinEnviarCorreo()
    {
        var dto = new SolicitarRecuperacionDto { Correo = "noexiste@autostock.mx" };

        _tokenRepositoryMock
            .Setup(r => r.ContarSolicitudesRecientesPorCorreoAsync("noexiste@autostock.mx", It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        _usuarioRepositoryMock
            .Setup(u => u.GetByIdentifierAsync("noexiste@autostock.mx"))
            .ReturnsAsync((Usuario?)null);

        var respuesta = await _authService.SolicitarRecuperacionPasswordAsync(dto);

        Assert.Contains("Si el correo está registrado", respuesta.Mensaje);
        _emailServiceMock.Verify(
            e => e.EnviarEmailRecuperacionPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SolicitarRecuperacion_UsuarioInactivo_RetornaMensajeEstandarSinEnviarCorreo()
    {
        var usuarioInactivo = CrearUsuarioPrueba(activo: false);
        var dto = new SolicitarRecuperacionDto { Correo = "chucho@autostock.mx" };

        _tokenRepositoryMock
            .Setup(r => r.ContarSolicitudesRecientesPorCorreoAsync("chucho@autostock.mx", It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        _usuarioRepositoryMock
            .Setup(u => u.GetByIdentifierAsync("chucho@autostock.mx"))
            .ReturnsAsync(usuarioInactivo);

        var respuesta = await _authService.SolicitarRecuperacionPasswordAsync(dto);

        Assert.Contains("Si el correo está registrado", respuesta.Mensaje);
        _emailServiceMock.Verify(
            e => e.EnviarEmailRecuperacionPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SolicitarRecuperacion_UsuarioActivo_InvalidaPreviosCreaTokenYEnviaEmail()
    {
        var usuarioActivo = CrearUsuarioPrueba(activo: true);
        var dto = new SolicitarRecuperacionDto { Correo = "chucho@autostock.mx" };

        var tokenPrevio = new TokenRecuperacionPassword
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioActivo.Id,
            Token = "token-anterior-activo",
            FechaExpiracion = DateTime.UtcNow.AddMinutes(20),
            Usado = false,
        };

        _tokenRepositoryMock
            .Setup(r => r.ContarSolicitudesRecientesPorCorreoAsync("chucho@autostock.mx", It.IsAny<DateTime>()))
            .ReturnsAsync(1);

        _usuarioRepositoryMock
            .Setup(u => u.GetByIdentifierAsync("chucho@autostock.mx"))
            .ReturnsAsync(usuarioActivo);

        _tokenRepositoryMock
            .Setup(r => r.ObtenerActivosPorUsuarioIdAsync(usuarioActivo.Id))
            .ReturnsAsync(new List<TokenRecuperacionPassword> { tokenPrevio });

        var respuesta = await _authService.SolicitarRecuperacionPasswordAsync(dto, "https://autostock.mx");

        Assert.Contains("Si el correo está registrado", respuesta.Mensaje);
        Assert.NotNull(respuesta.TokenDev);

        Assert.True(tokenPrevio.Usado);
        Assert.NotNull(tokenPrevio.FechaUso);
        _tokenRepositoryMock.Verify(r => r.Actualizar(tokenPrevio), Times.Once);

        _tokenRepositoryMock.Verify(
            r => r.AgregarAsync(It.Is<TokenRecuperacionPassword>(
                t => t.UsuarioId == usuarioActivo.Id &&
                     t.Correo == "chucho@autostock.mx" &&
                     !t.Usado &&
                     t.FechaExpiracion > DateTime.UtcNow)),
            Times.Once);

        _emailServiceMock.Verify(
            e => e.EnviarEmailRecuperacionPasswordAsync(
                "chucho@autostock.mx",
                "José de Jesús López",
                It.Is<string>(link => link.Contains("https://autostock.mx/restablecer-password?token="))),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidarToken_TokenVacio_RetornaInvalido(string token)
    {
        var resultado = await _authService.ValidarTokenRecuperacionAsync(token);

        Assert.False(resultado.EsValido);
        Assert.Equal("El enlace ya no es válido", resultado.Mensaje);
    }

    [Fact]
    public async Task ValidarToken_TokenVencidoOUsado_RetornaInvalido()
    {
        var tokenVencido = new TokenRecuperacionPassword
        {
            Id = Guid.NewGuid(),
            Token = "token-vencido",
            Correo = "test@autostock.mx",
            FechaExpiracion = DateTime.UtcNow.AddMinutes(-5),
            Usado = false,
        };

        _tokenRepositoryMock
            .Setup(r => r.ObtenerPorTokenAsync("token-vencido"))
            .ReturnsAsync(tokenVencido);

        var resultado = await _authService.ValidarTokenRecuperacionAsync("token-vencido");

        Assert.False(resultado.EsValido);
        Assert.Equal("El enlace ya no es válido", resultado.Mensaje);
    }

    [Fact]
    public async Task ValidarToken_TokenValido_RetornaValidoConCorreoEnmascarado()
    {
        var usuario = CrearUsuarioPrueba(activo: true);
        var tokenValido = new TokenRecuperacionPassword
        {
            Id = Guid.NewGuid(),
            Token = "token-valido-123",
            Correo = "jose.lopez@autostock.com",
            FechaExpiracion = DateTime.UtcNow.AddMinutes(25),
            Usado = false,
            Usuario = usuario,
        };

        _tokenRepositoryMock
            .Setup(r => r.ObtenerPorTokenAsync("token-valido-123"))
            .ReturnsAsync(tokenValido);

        var resultado = await _authService.ValidarTokenRecuperacionAsync("token-valido-123");

        Assert.True(resultado.EsValido);
        Assert.Equal("Enlace válido", resultado.Mensaje);
        Assert.NotNull(resultado.CorreoEnmascarado);
        Assert.Contains("***", resultado.CorreoEnmascarado);
        Assert.EndsWith("@autostock.com", resultado.CorreoEnmascarado);
    }

    [Fact]
    public async Task RestablecerPassword_PasswordsNoCoinciden_LanzaValidationAppException()
    {
        var dto = new RestablecerPasswordDto
        {
            Token = "token-123",
            PasswordNueva = "Password123!",
            ConfirmarPassword = "PasswordDiferente456!",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _authService.RestablecerPasswordAsync(dto));

        Assert.Contains("Las contraseñas no coinciden", ex.Message);
    }

    [Theory]
    [InlineData("corta1")]
    [InlineData("sololetras")]
    [InlineData("12345678")]
    public async Task RestablecerPassword_PasswordInvalida_LanzaValidationAppException(string claveDebil)
    {
        var dto = new RestablecerPasswordDto
        {
            Token = "token-123",
            PasswordNueva = claveDebil,
            ConfirmarPassword = claveDebil,
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _authService.RestablecerPasswordAsync(dto));

        Assert.Contains("La nueva contraseña debe tener al menos 8 caracteres", ex.Message);
    }

    [Fact]
    public async Task RestablecerPassword_TokenInexistenteOVencido_LanzaValidationAppException()
    {
        var dto = new RestablecerPasswordDto
        {
            Token = "token-inexistente",
            PasswordNueva = "AutoStock2026!",
            ConfirmarPassword = "AutoStock2026!",
        };

        _tokenRepositoryMock
            .Setup(r => r.ObtenerPorTokenAsync("token-inexistente"))
            .ReturnsAsync((TokenRecuperacionPassword?)null);

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _authService.RestablecerPasswordAsync(dto));

        Assert.Contains("El enlace ya no es válido", ex.Message);
    }

    [Fact]
    public async Task RestablecerPassword_Exitoso_ActualizaPasswordDesbloqueaCuentaEMarcaTokenUsado()
    {
        var usuario = CrearUsuarioPrueba(activo: true);
        usuario.IntentosFallidos = 5;
        usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15);
        usuario.RequiereCambioPassword = true;

        var tokenValido = new TokenRecuperacionPassword
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            Token = "token-valido-123",
            Correo = "chucho@autostock.mx",
            FechaExpiracion = DateTime.UtcNow.AddMinutes(20),
            Usado = false,
            Usuario = usuario,
        };

        _tokenRepositoryMock
            .Setup(r => r.ObtenerPorTokenAsync("token-valido-123"))
            .ReturnsAsync(tokenValido);

        _tokenRepositoryMock
            .Setup(r => r.ObtenerActivosPorUsuarioIdAsync(usuario.Id))
            .ReturnsAsync(new List<TokenRecuperacionPassword> { tokenValido });

        _passwordHasherMock
            .Setup(h => h.Hash("NuevaClave2026!"))
            .Returns("hash-nueva-clave");

        var dto = new RestablecerPasswordDto
        {
            Token = "token-valido-123",
            PasswordNueva = "NuevaClave2026!",
            ConfirmarPassword = "NuevaClave2026!",
        };

        await _authService.RestablecerPasswordAsync(dto);

        Assert.Equal("hash-nueva-clave", usuario.PasswordHash);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        Assert.False(usuario.RequiereCambioPassword);

        Assert.True(tokenValido.Usado);
        Assert.NotNull(tokenValido.FechaUso);

        _usuarioRepositoryMock.Verify(u => u.Update(usuario), Times.Once);
        _tokenRepositoryMock.Verify(t => t.Actualizar(tokenValido), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
