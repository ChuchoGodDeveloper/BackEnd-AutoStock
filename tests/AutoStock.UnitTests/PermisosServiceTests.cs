using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Services;
using AutoStock.Domain.Constants;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using Moq;

namespace AutoStock.UnitTests;

public class PermisosServiceTests
{
    private readonly Mock<IPermisoRepository> _permisoRepositoryMock = new();
    private readonly Mock<IRolRepository> _rolRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly PermisosService _permisosService;

    public PermisosServiceTests()
    {
        _permisosService = new PermisosService(
            _permisoRepositoryMock.Object,
            _rolRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static List<Permiso> CrearPermisosPrueba() =>
    [
        new() { Id = 1, Codigo = CodigosPermiso.ConsultarCatalogo, Nombre = "Consultar catálogo", Modulo = "Catálogo", EsFijo = true },
        new() { Id = 2, Codigo = CodigosPermiso.CambiarEstado, Nombre = "Cambiar estado operativo", Modulo = "Inventario", EsFijo = false },
        new() { Id = 3, Codigo = CodigosPermiso.RegistrarVehiculos, Nombre = "Registrar vehículos", Modulo = "Inventario", EsFijo = false },
        new() { Id = 4, Codigo = CodigosPermiso.CapturarFotografias, Nombre = "Capturar fotografías", Modulo = "Inspección", EsFijo = false },
        new() { Id = 5, Codigo = CodigosPermiso.AsignarCajon, Nombre = "Asignar o reubicar cajón", Modulo = "Lote", EsFijo = false },
        new() { Id = 6, Codigo = CodigosPermiso.EditarKilometrajeNotas, Nombre = "Editar kilometraje y notas", Modulo = "Inspección", EsFijo = false },
        new() { Id = 7, Codigo = CodigosPermiso.GestionarOrdenesServicio, Nombre = "Gestionar órdenes de servicio", Modulo = "Taller", EsFijo = false },
        new() { Id = 8, Codigo = CodigosPermiso.MarcarVendido, Nombre = "Marcar unidades como vendidas", Modulo = "Ventas", EsFijo = false },
        new() { Id = 9, Codigo = CodigosPermiso.VerReporteVentas, Nombre = "Ver el reporte de ventas", Modulo = "Ventas", EsFijo = false },
        new() { Id = 10, Codigo = CodigosPermiso.VerMetricasLote, Nombre = "Ver las métricas del lote", Modulo = "Dashboard", EsFijo = false },
    ];

    private static List<Rol> CrearRolesPrueba() =>
    [
        new() { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total" },
        new() { Id = 2, Nombre = "Gerente de Ventas", Descripcion = "Supervisión de ventas" },
        new() { Id = 3, Nombre = "Encargado de Lote y Recepción", Descripcion = "Administración de lotes" },
        new() { Id = 4, Nombre = "Asesor de Ventas", Descripcion = "Atención a clientes" },
    ];

    [Fact]
    public async Task GetMatrizPermisosAsync_RetornaMatrizCompletaConRolesYAcciones()
    {
        var permisos = CrearPermisosPrueba();
        var roles = CrearRolesPrueba();

        _permisoRepositoryMock.Setup(r => r.GetAllPermisosAsync()).ReturnsAsync(permisos);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);
        _permisoRepositoryMock.Setup(r => r.GetAllRolPermisosAsync()).ReturnsAsync([]);

        var matriz = await _permisosService.GetMatrizPermisosAsync();

        Assert.NotNull(matriz);
        Assert.Equal(10, matriz.Acciones.Count);
        Assert.Equal(4, matriz.Roles.Count);

        var adminRol = matriz.Roles.First(r => r.NombreRol == "Administrador");
        Assert.All(adminRol.Permisos.Values, activo => Assert.True(activo));

        var asesorRol = matriz.Roles.First(r => r.NombreRol == "Asesor de Ventas");
        Assert.True(asesorRol.Permisos[CodigosPermiso.ConsultarCatalogo]);
        Assert.True(asesorRol.Permisos[CodigosPermiso.CambiarEstado]);
        Assert.False(asesorRol.Permisos[CodigosPermiso.RegistrarVehiculos]);
    }

    [Fact]
    public async Task ActualizarMatrizPermisosAsync_ConDatosValidos_ActualizaYRetornaMensajeExitoso()
    {
        var permisos = CrearPermisosPrueba();
        var roles = CrearRolesPrueba();

        _permisoRepositoryMock.Setup(r => r.GetAllPermisosAsync()).ReturnsAsync(permisos);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);
        _permisoRepositoryMock.Setup(r => r.GetAllRolPermisosAsync()).ReturnsAsync([]);

        var dto = new ActualizarMatrizPermisosRequestDto
        {
            Roles =
            [
                new()
                {
                    RolId = 4, // Asesor
                    Permisos = new Dictionary<string, bool>
                    {
                        { CodigosPermiso.ConsultarCatalogo, true },
                        { CodigosPermiso.CambiarEstado, true },
                        { CodigosPermiso.MarcarVendido, true },
                    },
                },
            ],
        };

        var resultado = await _permisosService.ActualizarMatrizPermisosAsync(dto, usuarioId: 1, nombreUsuario: "Carlos Ramírez");

        Assert.Equal("Permisos actualizados", resultado.Mensaje);
        _permisoRepositoryMock.Verify(r => r.AddRolPermisosAsync(It.IsAny<IEnumerable<RolPermiso>>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarMatrizPermisosAsync_IntentoDesactivarPermisoAdmin_LanzaValidationAppException()
    {
        var permisos = CrearPermisosPrueba();
        var roles = CrearRolesPrueba();

        _permisoRepositoryMock.Setup(r => r.GetAllPermisosAsync()).ReturnsAsync(permisos);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);
        _permisoRepositoryMock.Setup(r => r.GetAllRolPermisosAsync()).ReturnsAsync([]);

        var dto = new ActualizarMatrizPermisosRequestDto
        {
            Roles =
            [
                new()
                {
                    RolId = 1, // Administrador
                    Permisos = new Dictionary<string, bool>
                    {
                        { CodigosPermiso.RegistrarVehiculos, false },
                    },
                },
            ],
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() =>
            _permisosService.ActualizarMatrizPermisosAsync(dto, usuarioId: 1, nombreUsuario: "Carlos Ramírez"));

        Assert.Contains("inmutables", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActualizarMatrizPermisosAsync_IntentoQuitarConsultarCatalogo_LanzaValidationAppException()
    {
        var permisos = CrearPermisosPrueba();
        var roles = CrearRolesPrueba();

        _permisoRepositoryMock.Setup(r => r.GetAllPermisosAsync()).ReturnsAsync(permisos);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);
        _permisoRepositoryMock.Setup(r => r.GetAllRolPermisosAsync()).ReturnsAsync([]);

        var dto = new ActualizarMatrizPermisosRequestDto
        {
            Roles =
            [
                new()
                {
                    RolId = 4, // Asesor
                    Permisos = new Dictionary<string, bool>
                    {
                        { CodigosPermiso.ConsultarCatalogo, false },
                    },
                },
            ],
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() =>
            _permisosService.ActualizarMatrizPermisosAsync(dto, usuarioId: 1, nombreUsuario: "Carlos Ramírez"));

        Assert.Contains("obligatorio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RestablecerMatrizBaseAsync_RestauraPermisosPorDefectoYGuardaAuditoria()
    {
        var permisos = CrearPermisosPrueba();
        var roles = CrearRolesPrueba();

        _permisoRepositoryMock.Setup(r => r.GetAllPermisosAsync()).ReturnsAsync(permisos);
        _rolRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);
        _permisoRepositoryMock.Setup(r => r.GetAllRolPermisosAsync()).ReturnsAsync([]);

        var resultado = await _permisosService.RestablecerMatrizBaseAsync(usuarioId: 1, nombreUsuario: "Carlos Ramírez");

        Assert.Equal("Permisos restablecidos a los valores iniciales", resultado.Mensaje);
        _permisoRepositoryMock.Verify(r => r.AddRolPermisosAsync(It.Is<IEnumerable<RolPermiso>>(lista =>
            lista.Count() == 40)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
