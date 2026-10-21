using AutoStock.Application.Exceptions;
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

    private static Vehiculo CrearVehiculoConFicha(Guid id, int cantidadImagenes)
    {
        var vehiculo = new Vehiculo
        {
            Id = id,
            Marca = "Toyota",
            Modelo = "Corolla",
            Anio = 2024,
            Precio = 389900m,
            Kilometraje = 18500,
            Placa = "ABC-1234",
            ZonaLote = "Z1",
            CajonLote = "A01",
            FotoPrincipalUrl = "https://cdn.autostock.mx/vehiculos/toyota-corolla-2024.jpg",
            Estado = EstadoOperativo.Disponible,
            FechaRegistro = new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc),
            VIN = "3AUTST24000000001",
            NumeroStock = "STK-0001",
            Motor = "2.0L 4 cilindros",
            Potencia = "150 HP",
            Transmision = "Automática",
            ColorExterior = "Blanco",
            ColorInterior = "Negro",
            Equipamiento = "Aire acondicionado, cámara de reversa",
            NotasInspeccion = "Sin daños estructurales",
        };

        for (var indice = 0; indice < cantidadImagenes; indice++)
        {
            vehiculo.Imagenes.Add(new ImagenVehiculo
            {
                Id = Guid.Parse($"00000000-0000-0000-0002-{(indice + 1).ToString("D12")}"),
                VehiculoId = id,
                Url = $"https://cdn.autostock.mx/vehiculos/toyota-corolla-2024-v{indice + 1}.jpg",
                EsPrincipal = indice == 0,
                Orden = indice,
            });
        }

        return vehiculo;
    }

    [Fact]
    public async Task GetVehiculoByIdAsync_IdExistente_DevuelveDtoConTodasSusImagenes()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var vehiculo = CrearVehiculoConFicha(id, cantidadImagenes: 4);

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(id))
            .ReturnsAsync(vehiculo);

        var resultado = await _vehiculosService.GetVehiculoByIdAsync(id);

        Assert.Equal(id, resultado.Id);
        Assert.Equal("Toyota", resultado.Marca);
        Assert.Equal("Corolla", resultado.Modelo);
        Assert.Equal("3AUTST24000000001", resultado.VIN);
        Assert.Equal("STK-0001", resultado.NumeroStock);
        Assert.Equal("2.0L 4 cilindros", resultado.Motor);
        Assert.Equal("150 HP", resultado.Potencia);
        Assert.Equal("Automática", resultado.Transmision);
        Assert.Equal("Blanco", resultado.ColorExterior);
        Assert.Equal("Negro", resultado.ColorInterior);
        Assert.Equal("Aire acondicionado, cámara de reversa", resultado.Equipamiento);
        Assert.Equal("Sin daños estructurales", resultado.NotasInspeccion);
        Assert.Equal(4, resultado.Imagenes.Count);
        Assert.True(resultado.Imagenes[0].EsPrincipal);
        Assert.All(resultado.Imagenes, imagen => Assert.StartsWith("https://", imagen.Url));
    }

    [Fact]
    public async Task GetVehiculoByIdAsync_IdInexistente_LanzaNotFoundException()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000999");

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(id))
            .ReturnsAsync((Vehiculo?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _vehiculosService.GetVehiculoByIdAsync(id));
    }
}
