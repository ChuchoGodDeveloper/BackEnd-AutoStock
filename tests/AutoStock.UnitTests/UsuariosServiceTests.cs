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
        FechaNacimiento = new DateOnly(1995, 4, 20),
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
        Assert.Equal(new DateOnly(1995, 4, 20), resultado.FechaNacimiento);
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
    public async Task CrearUsuario_SinEmpleadoId_GeneraIdentificadorAutomaticamente()
    {
        var dto = DtoValido();
        dto.EmpleadoId = null;

        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdAsync("ML-950420-01"))
            .ReturnsAsync(false);
        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdOCorreoAsync("ML-950420-01", "maria.lopez@autostock.com"))
            .ReturnsAsync(false);
        _rolRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(RolVendedor());
        _passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hashed-pass");
        _usuarioRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => u.Id = 15)
            .Returns(Task.CompletedTask);

        var resultado = await _usuariosService.CrearUsuarioAsync(dto);

        Assert.Equal("ML-950420-01", resultado.EmpleadoId);
        _usuarioRepositoryMock.Verify(r => r.AddAsync(It.Is<Usuario>(u => u.EmpleadoId == "ML-950420-01")), Times.Once);
    }

    [Fact]
    public async Task CrearUsuario_ConColisionEnConsecutivo01_IncrementaAConsecutivo02()
    {
        var dto = DtoValido();
        dto.EmpleadoId = null;

        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdAsync("ML-950420-01"))
            .ReturnsAsync(true);
        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdAsync("ML-950420-02"))
            .ReturnsAsync(false);
        _usuarioRepositoryMock
            .Setup(r => r.ExisteEmpleadoIdOCorreoAsync("ML-950420-02", "maria.lopez@autostock.com"))
            .ReturnsAsync(false);
        _rolRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(RolVendedor());
        _passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hashed-pass");
        _usuarioRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => u.Id = 16)
            .Returns(Task.CompletedTask);

        var resultado = await _usuariosService.CrearUsuarioAsync(dto);

        Assert.Equal("ML-950420-02", resultado.EmpleadoId);
    }

    [Fact]
    public async Task CrearUsuario_MenorDeEdad_LanzaValidationAppException()
    {
        var dto = DtoValido();
        dto.FechaNacimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17));

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _usuariosService.CrearUsuarioAsync(dto));

        Assert.Equal("El usuario debe tener al menos 18 años de edad", ex.Message);
        _usuarioRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Never);
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

    private static Rol RolAdministrador() => new()
    {
        Id = 1,
        Nombre = "Administrador",
        Descripcion = "Acceso total al sistema",
    };

    [Fact]
    public async Task ObtenerUsuarioPorId_UsuarioExiste_RetornaUsuarioDetalleAdminDto()
    {
        var usuario = new Usuario
        {
            Id = 10,
            EmpleadoId = "EMP-00010",
            NombreCompleto = "Carlos Mendoza",
            Correo = "carlos@autostock.com",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            RolId = 2,
            Rol = RolVendedor(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            RequiereCambioPassword = false,
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(usuario);

        var resultado = await _usuariosService.ObtenerUsuarioPorIdAsync(10);

        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.Id);
        Assert.Equal("EMP-00010", resultado.EmpleadoId);
        Assert.Equal("Carlos Mendoza", resultado.NombreCompleto);
        Assert.Equal("carlos@autostock.com", resultado.Correo);
        Assert.Equal("Vendedor", resultado.Rol);
        Assert.Equal(2, resultado.RolId);
        Assert.True(resultado.Activo);
    }

    [Fact]
    public async Task ObtenerUsuarioPorId_UsuarioInexistente_LanzaNotFoundException()
    {
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _usuariosService.ObtenerUsuarioPorIdAsync(999));
    }

    [Fact]
    public async Task EditarUsuario_DatosValidos_ActualizaCorrectamente()
    {
        var rolAdmin = RolAdministrador();
        var rolVend = RolVendedor();
        var usuario = new Usuario
        {
            Id = 5,
            EmpleadoId = "EMP-00005",
            NombreCompleto = "Juan Pérez Original",
            Correo = "juan.original@autostock.com",
            FechaNacimiento = new DateOnly(1990, 5, 10),
            RolId = 2,
            Rol = rolVend,
            Activo = true,
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(usuario);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rol> { rolAdmin, rolVend });
        _usuarioRepositoryMock.Setup(r => r.ExisteCorreoEnOtroUsuarioAsync(5, "juan.nuevo@autostock.com")).ReturnsAsync(false);

        var dto = new EditarUsuarioDto
        {
            NombreCompleto = "Juan Pérez Modificado",
            Correo = "juan.nuevo@autostock.com",
            Rol = "Vendedor",
            FechaNacimiento = new DateOnly(1990, 5, 15),
        };

        var resultado = await _usuariosService.EditarUsuarioAsync(5, dto, administradorAutenticadoId: 1);

        Assert.Equal("Juan Pérez Modificado", resultado.NombreCompleto);
        Assert.Equal("juan.nuevo@autostock.com", resultado.Correo);
        Assert.Equal("Vendedor", resultado.Rol);
        _usuarioRepositoryMock.Verify(r => r.Update(It.Is<Usuario>(u => u.Id == 5 && u.NombreCompleto == "Juan Pérez Modificado")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditarUsuario_CorreoDuplicadoEnOtroUsuario_LanzaValidationAppException()
    {
        var rolAdmin = RolAdministrador();
        var rolVend = RolVendedor();
        var usuario = new Usuario
        {
            Id = 5,
            EmpleadoId = "EMP-00005",
            NombreCompleto = "Juan Pérez",
            Correo = "duplicado@autostock.com",
            RolId = 2,
            Rol = rolVend,
            Activo = true,
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(usuario);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rol> { rolAdmin, rolVend });
        _usuarioRepositoryMock.Setup(r => r.ExisteCorreoEnOtroUsuarioAsync(5, "duplicado@autostock.com")).ReturnsAsync(true);

        var dto = new EditarUsuarioDto
        {
            NombreCompleto = "Juan Pérez",
            Correo = "duplicado@autostock.com",
            Rol = "Vendedor",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _usuariosService.EditarUsuarioAsync(5, dto, administradorAutenticadoId: 1));

        Assert.Equal("Ya existe un usuario con ese correo", ex.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditarUsuario_AdminIntentaCambiarSuPropioRol_LanzaValidationAppException()
    {
        var rolAdmin = RolAdministrador();
        var rolVend = RolVendedor();
        var admin = new Usuario
        {
            Id = 1,
            EmpleadoId = "EMP-ADMIN",
            NombreCompleto = "Super Admin",
            Correo = "admin@autostock.com",
            RolId = 1,
            Rol = rolAdmin,
            Activo = true,
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rol> { rolAdmin, rolVend });

        var dto = new EditarUsuarioDto
        {
            NombreCompleto = "Super Admin",
            Correo = "admin@autostock.com",
            Rol = "Vendedor",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _usuariosService.EditarUsuarioAsync(1, dto, administradorAutenticadoId: 1));

        Assert.Equal("No puedes desactivar tu cuenta ni cambiar tu propio rol", ex.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstatus_AdminIntentaDesactivarseASiMismo_LanzaValidationAppException()
    {
        var admin = new Usuario
        {
            Id = 1,
            EmpleadoId = "EMP-ADMIN",
            NombreCompleto = "Super Admin",
            Correo = "admin@autostock.com",
            RolId = 1,
            Rol = RolAdministrador(),
            Activo = true,
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);

        var dto = new CambiarEstatusUsuarioDto { Activo = false };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _usuariosService.CambiarEstatusUsuarioAsync(1, dto, administradorAutenticadoId: 1));

        Assert.Equal("No puedes desactivar tu cuenta ni cambiar tu propio rol", ex.Message);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstatus_UsuarioNormal_DesactivaOReactivaCorrectamente()
    {
        var usuario = new Usuario
        {
            Id = 8,
            EmpleadoId = "EMP-00008",
            NombreCompleto = "Laura Gomez",
            Correo = "laura@autostock.com",
            RolId = 2,
            Rol = RolVendedor(),
            Activo = true,
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(usuario);

        var dto = new CambiarEstatusUsuarioDto { Activo = false };

        var resultado = await _usuariosService.CambiarEstatusUsuarioAsync(8, dto, administradorAutenticadoId: 1);

        Assert.False(resultado.Activo);
        Assert.False(usuario.Activo);
        _usuarioRepositoryMock.Verify(r => r.Update(It.Is<Usuario>(u => u.Id == 8 && !u.Activo)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestablecerPasswordPorAdmin_GeneraPasswordTemporalYMarcaRequiereCambio()
    {
        var usuario = new Usuario
        {
            Id = 12,
            EmpleadoId = "EMP-00012",
            NombreCompleto = "Pedro Pascal",
            Correo = "pedro@autostock.com",
            RolId = 2,
            Rol = RolVendedor(),
            Activo = true,
            RequiereCambioPassword = false,
            IntentosFallidos = 3,
            BloqueadoHasta = DateTime.UtcNow.AddMinutes(15),
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(12)).ReturnsAsync(usuario);
        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => $"hash-{p}");

        var resultado = await _usuariosService.RestablecerPasswordPorAdminAsync(12);

        Assert.Equal(12, resultado.Id);
        Assert.Equal("EMP-00012", resultado.EmpleadoId);
        Assert.Equal("Pedro Pascal", resultado.NombreCompleto);
        Assert.False(string.IsNullOrWhiteSpace(resultado.PasswordTemporal));
        Assert.True(resultado.PasswordTemporal.Length >= 8);
        Assert.True(usuario.RequiereCambioPassword);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        _usuarioRepositoryMock.Verify(r => r.Update(It.Is<Usuario>(u => u.Id == 12 && u.RequiereCambioPassword)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
