using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Application.Services;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using Moq;

namespace AutoStock.UnitTests;

public class UsuariosServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IRolRepository> _rolRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UsuariosService _usuariosService;

    public UsuariosServiceTests()
    {
        _usuariosService = new UsuariosService(
            _usuarioRepositoryMock.Object,
            _rolRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Rol RolVendedor() => new()
    {
        Id = 2,
        Nombre = "Vendedor",
        Descripcion = "Consulta y registro de unidades",
    };

    private static CrearUsuarioDto DtoValido() => new()
    {
        EmpleadoId = "EMP-00042",
        NombreCompleto = "María López",
        Correo = "maria.lopez@autostock.com",
        RolId = 2,
    };

    [Fact]
    public async Task CrearUsuario_ConDatosValidos_RetornaPasswordTemporalYRequiereCambioPassword()
    {
        string? passwordHashGuardado = null;

        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdOCorreoAsync("EMP-00042", "maria.lopez@autostock.com"))
            .ReturnsAsync(false);
        _rolRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(RolVendedor());
        _passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns<string>((password) =>
            {
                passwordHashGuardado = password;
                return $"hash-{password}";
            });
        _usuarioRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(usuario => usuario.Id = 7)
            .Returns(Task.CompletedTask);

        var resultado = await _usuariosService.CrearUsuarioAsync(DtoValido());

        Assert.Equal(7, resultado.Id);
        Assert.Equal("EMP-00042", resultado.EmpleadoId);
        Assert.Equal("maria.lopez@autostock.com", resultado.Correo);
        Assert.Equal("Vendedor", resultado.Rol);
        Assert.True(resultado.Activo);
        Assert.True(resultado.RequiereCambioPassword);
        Assert.False(string.IsNullOrWhiteSpace(resultado.PasswordTemporal));
        Assert.True(resultado.PasswordTemporal.Length >= 8);
        Assert.Contains(resultado.PasswordTemporal, char.IsLetter);
        Assert.Contains(resultado.PasswordTemporal, char.IsDigit);
        Assert.Equal(resultado.PasswordTemporal, passwordHashGuardado);
        Assert.NotNull(passwordHashGuardado);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearUsuario_ConEmpleadoIdOCorreoDuplicado_LanzaErrorConMensajeExacto()
    {
        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdOCorreoAsync("EMP-00042", "maria.lopez@autostock.com"))
            .ReturnsAsync(true);

        var excepcion = await Assert.ThrowsAsync<ValidationAppException>(
            () => _usuariosService.CrearUsuarioAsync(DtoValido()));

        Assert.Equal("Ya existe un usuario con ese ID de empleado o correo", excepcion.Message);
        _usuarioRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
