using AutoStock.Application.Services;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
using AutoStock.Domain.Interfaces;
using Moq;

namespace AutoStock.UnitTests;

public class VehiculosServiceTests
{
    private readonly Mock<IVehiculoRepository> _vehiculoRepositoryMock = new();
    private readonly VehiculosService _vehiculosService;

    public VehiculosServiceTests()
    {
        _vehiculosService = new VehiculosService(_vehiculoRepositoryMock.Object);
    }

    private static Vehiculo CrearVehiculo(int indice, EstadoOperativo estado, DateTime fechaRegistro)
    {
        return new Vehiculo
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{(indice + 1).ToString("D12")}"),
            Marca = $"Marca{indice}",
            Modelo = $"Modelo{indice}",
            Anio = 2024,
            Precio = 300000m + indice,
            Kilometraje = 10000 + indice,
            Placa = $"ABC-{indice}",
            ZonaLote = "Z1",
            CajonLote = "A01",
            FotoPrincipalUrl = $"https://cdn.autostock.mx/vehiculos/marca{indice}.jpg",
            Estado = estado,
            FechaRegistro = fechaRegistro,
        };
    }

    private static List<Vehiculo> CatalogoDePrueba()
    {
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var vehiculos = new List<Vehiculo>();

        for (var indice = 0; indice < 25; indice++)
        {
            vehiculos.Add(CrearVehiculo(indice, EstadoOperativo.Disponible, fechaBase.AddMinutes(indice)));
        }

        vehiculos.Add(CrearVehiculo(500, EstadoOperativo.Vendido, fechaBase.AddDays(1)));
        vehiculos.Add(CrearVehiculo(501, EstadoOperativo.Vendido, fechaBase.AddMinutes(-50)));

        return vehiculos;
    }

    [Fact]
    public async Task GetCatalogoAsync_NoDevuelveVehiculosVendidos()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(CatalogoDePrueba());

        var resultado = await _vehiculosService.GetCatalogoAsync();

        Assert.Equal(25, resultado.TotalCount);
        Assert.DoesNotContain(resultado.Items, vehiculo => vehiculo.Estado == nameof(EstadoOperativo.Vendido));
        Assert.All(resultado.Items, vehiculo => Assert.NotEqual(nameof(EstadoOperativo.Vendido), vehiculo.Estado));
    }

    [Fact]
    public async Task GetCatalogoAsync_OrdenaPorFechaRegistroDescendente()
    {
        var vehiculos = CatalogoDePrueba();
        var fechasEsperadas = vehiculos
            .Where(v => v.Estado != EstadoOperativo.Vendido)
            .Select(v => v.FechaRegistro)
            .OrderByDescending(fecha => fecha)
            .ToList();

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(1, 25);

        var fechasObtenidas = resultado.Items.Select(v => v.FechaRegistro).ToList();

        Assert.Equal(fechasEsperadas, fechasObtenidas);
    }

    [Fact]
    public async Task GetCatalogoAsync_Pagina1Devuelve20YPagina2Devuelve5()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(CatalogoDePrueba());

        var pagina1 = await _vehiculosService.GetCatalogoAsync(1, 20);
        var pagina2 = await _vehiculosService.GetCatalogoAsync(2, 20);

        Assert.Equal(20, pagina1.Items.Count);
        Assert.Equal(1, pagina1.Page);
        Assert.Equal(5, pagina2.Items.Count);
        Assert.Equal(2, pagina2.Page);
        Assert.Equal(20, pagina1.PageSize);
        Assert.Equal(20, pagina2.PageSize);
    }

    [Fact]
    public async Task GetCatalogoAsync_TotalCount_EsElTotalDeNoVendidosAlPaginar()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(CatalogoDePrueba());

        var pagina1 = await _vehiculosService.GetCatalogoAsync(1, 20);
        var pagina2 = await _vehiculosService.GetCatalogoAsync(2, 20);

        Assert.Equal(25, pagina1.TotalCount);
        Assert.Equal(25, pagina2.TotalCount);
    }

    [Fact]
    public async Task GetCatalogoAsync_SinVehiculos_RetornaListaVacia()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(new List<Vehiculo>());

        var resultado = await _vehiculosService.GetCatalogoAsync();

        Assert.Empty(resultado.Items);
        Assert.Equal(0, resultado.TotalCount);
        Assert.Equal(1, resultado.Page);
        Assert.Equal(20, resultado.PageSize);
    }

    [Fact]
    public async Task GetCatalogoAsync_ParametrosInvalidosUsaValoresPorDefecto()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(CatalogoDePrueba());

        var resultado = await _vehiculosService.GetCatalogoAsync(0, -5);

        Assert.Equal(1, resultado.Page);
        Assert.Equal(20, resultado.PageSize);
        Assert.Equal(20, resultado.Items.Count);
        Assert.Equal(25, resultado.TotalCount);
    }
}
