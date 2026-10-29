using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Constants;
using AutoStock.Application.Services;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
using AutoStock.Domain.Interfaces;
using Moq;

namespace AutoStock.UnitTests;

public class VehiculosServiceTests
{
    private readonly Mock<IVehiculoRepository> _vehiculoRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPermisosService> _permisosServiceMock = new();
    private readonly VehiculosService _vehiculosService;

    public VehiculosServiceTests()
    {
        _vehiculosService = new VehiculosService(
            _vehiculoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _permisosServiceMock.Object);
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

    private static Vehiculo CrearVehiculoParaBusqueda(
        int indice,
        string marca,
        string modelo,
        string? placa,
        string vin,
        EstadoOperativo estado = EstadoOperativo.Disponible,
        decimal precio = 300000m,
        TipoVehiculo tipo = TipoVehiculo.Seminuevo)
    {
        return new Vehiculo
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{(indice + 1).ToString("D12")}"),
            Marca = marca,
            Modelo = modelo,
            Anio = 2024,
            Precio = precio,
            Kilometraje = 10000,
            Placa = placa,
            ZonaLote = "Z1",
            CajonLote = "A01",
            FotoPrincipalUrl = null,
            Estado = estado,
            Tipo = tipo,
            FechaRegistro = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc).AddMinutes(indice),
            VIN = vin,
            NumeroStock = $"STK-{(indice + 1):D4}",
        };
    }

    [Fact]
    public async Task GetCatalogoAsync_BusquedaPorVinParcial_DevuelveCoincidencia()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST24000000001"),
            CrearVehiculoParaBusqueda(1, "Mazda", "CX-5", "ABC-1001", "3AUTST25000000002"),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(busqueda: "000001");

        Assert.Single(resultado.Items);
        Assert.Equal(vehiculos[0].Id, resultado.Items[0].Id);
        Assert.Equal(1, resultado.TotalCount);
    }

    [Fact]
    public async Task GetCatalogoAsync_BusquedaPorPlacaEnMinusculasSinGuiones_DevuelveCoincidencia()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Nissan", "Versa", "PZG-482-B", "3AUTST24000000010"),
            CrearVehiculoParaBusqueda(1, "Kia", "Rio", "ABC-1001", "3AUTST24000000011"),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(busqueda: "pzg482b");

        Assert.Single(resultado.Items);
        Assert.Equal("PZG-482-B", resultado.Items[0].Placa);
        Assert.Equal(1, resultado.TotalCount);
    }

    [Fact]
    public async Task GetCatalogoAsync_BusquedaPorMarcaOModelo_DevuelveCoincidencias()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST24000000020"),
            CrearVehiculoParaBusqueda(1, "Mazda", "CX-5", "ABC-1001", "3AUTST24000000021"),
            CrearVehiculoParaBusqueda(2, "Honda", "CR-V", "ABC-1002", "3AUTST24000000022"),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var porMarca = await _vehiculosService.GetCatalogoAsync(busqueda: "toyota");
        var porModelo = await _vehiculosService.GetCatalogoAsync(busqueda: "cx");

        Assert.Single(porMarca.Items);
        Assert.Equal("Toyota", porMarca.Items[0].Marca);
        Assert.Single(porModelo.Items);
        Assert.Equal("CX-5", porModelo.Items[0].Modelo);
    }

    [Fact]
    public async Task GetCatalogoAsync_BusquedaSinResultados_DevuelveListaVaciaYTotalCero()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST24000000030"),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(busqueda: "zzz-no-existe");

        Assert.Empty(resultado.Items);
        Assert.Equal(0, resultado.TotalCount);
    }

    [Fact]
    public async Task GetCatalogoAsync_BusquedaSeCombinaConPaginacionYExcluyeVendidos()
    {
        var fechaBase = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var vehiculos = new List<Vehiculo>();

        for (var indice = 0; indice < 5; indice++)
        {
            vehiculos.Add(new Vehiculo
            {
                Id = Guid.Parse($"00000000-0000-0000-0000-{(indice + 1).ToString("D12")}"),
                Marca = "Toyota",
                Modelo = "Corolla",
                Anio = 2024,
                Precio = 300000m,
                Kilometraje = 10000,
                Placa = $"ABC-{1000 + indice}",
                ZonaLote = "Z1",
                CajonLote = "A01",
                FotoPrincipalUrl = null,
                Estado = EstadoOperativo.Disponible,
                FechaRegistro = fechaBase.AddMinutes(indice),
                VIN = $"3AUTST240000000{40 + indice}",
                NumeroStock = $"STK-{(indice + 1):D4}",
            });
        }

        vehiculos.Add(CrearVehiculoParaBusqueda(10, "Toyota", "Corolla", "VEN-0001", "3AUTST24000000050", EstadoOperativo.Vendido));
        vehiculos.Add(CrearVehiculoParaBusqueda(11, "Mazda", "CX-5", "ABC-1999", "3AUTST24000000051"));

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var pagina1 = await _vehiculosService.GetCatalogoAsync(1, 2, "corolla");
        var pagina3 = await _vehiculosService.GetCatalogoAsync(3, 2, "corolla");

        Assert.Equal(5, pagina1.TotalCount);
        Assert.Equal(2, pagina1.Items.Count);
        Assert.DoesNotContain(pagina1.Items, vehiculo => vehiculo.Estado == nameof(EstadoOperativo.Vendido));
        Assert.DoesNotContain(pagina1.Items, vehiculo => vehiculo.Placa == "VEN-0001");
        Assert.Single(pagina3.Items);
        Assert.All(pagina1.Items, vehiculo => Assert.Equal("Toyota", vehiculo.Marca));
    }

    [Fact]
    public async Task GetCatalogoAsync_FiltradoPorEstadosCombinados_DevuelveUnionDeEstados()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Kia", "Sportage", "ABC-1000", "3AUTST24000000100", EstadoOperativo.EnTaller),
            CrearVehiculoParaBusqueda(1, "Renault", "Kwid", "ABC-1001", "3AUTST24000000101", EstadoOperativo.EnLavado),
            CrearVehiculoParaBusqueda(2, "Toyota", "Corolla", "ABC-1002", "3AUTST24000000102", EstadoOperativo.Disponible),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(
            estados: [EstadoOperativo.EnTaller, EstadoOperativo.EnLavado]);

        Assert.Equal(2, resultado.TotalCount);
        Assert.All(resultado.Items, vehiculo =>
            Assert.Contains(vehiculo.Estado, new[] { nameof(EstadoOperativo.EnTaller), nameof(EstadoOperativo.EnLavado) }));
    }

    [Fact]
    public async Task GetCatalogoAsync_FiltradoPorVendido_MuestraLosVendidos()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Kia", "Rio", "VEN-1000", "3AUTST24000000110", EstadoOperativo.Vendido),
            CrearVehiculoParaBusqueda(1, "Ford", "Explorer", "VEN-1001", "3AUTST24000000111", EstadoOperativo.Vendido),
            CrearVehiculoParaBusqueda(2, "Toyota", "Corolla", "ABC-1002", "3AUTST24000000112", EstadoOperativo.Disponible),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(
            estados: [EstadoOperativo.Vendido]);

        Assert.Equal(2, resultado.TotalCount);
        Assert.All(resultado.Items, vehiculo => Assert.Equal(nameof(EstadoOperativo.Vendido), vehiculo.Estado));
    }

    [Fact]
    public async Task GetCatalogoAsync_FiltradoPorPrecioRango_EsInclusivoEnLosExtremos()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Nissan", "Versa", "ABC-1000", "3AUTST24000000120", precio: 200000m),
            CrearVehiculoParaBusqueda(1, "Mazda", "CX-5", "ABC-1001", "3AUTST24000000121", precio: 300000m),
            CrearVehiculoParaBusqueda(2, "Honda", "CR-V", "ABC-1002", "3AUTST24000000122", precio: 400000m),
            CrearVehiculoParaBusqueda(3, "Toyota", "RAV4", "ABC-1003", "3AUTST24000000123", precio: 500000m),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(precioMin: 200000m, precioMax: 400000m);

        Assert.Equal(3, resultado.TotalCount);
        Assert.Contains(resultado.Items, vehiculo => vehiculo.Precio == 200000m);
        Assert.Contains(resultado.Items, vehiculo => vehiculo.Precio == 300000m);
        Assert.Contains(resultado.Items, vehiculo => vehiculo.Precio == 400000m);
        Assert.DoesNotContain(resultado.Items, vehiculo => vehiculo.Precio == 500000m);
    }

    [Fact]
    public async Task GetCatalogoAsync_CombinacionEstadosTipoYPrecio_AplicaTodosLosFiltros()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Kia", "Sportage", "ABC-1000", "3AUTST24000000130", EstadoOperativo.EnTaller, 450000m, TipoVehiculo.Seminuevo),
            CrearVehiculoParaBusqueda(1, "Kia", "Sonet", "ABC-1001", "3AUTST24000000131", EstadoOperativo.EnTaller, 450000m, TipoVehiculo.Nuevo),
            CrearVehiculoParaBusqueda(2, "Renault", "Kwid", "ABC-1002", "3AUTST24000000132", EstadoOperativo.EnLavado, 450000m, TipoVehiculo.Seminuevo),
            CrearVehiculoParaBusqueda(3, "Hyundai", "Tucson", "ABC-1003", "3AUTST24000000133", EstadoOperativo.EnTaller, 600000m, TipoVehiculo.Seminuevo),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetCatalogoAsync(
            estados: [EstadoOperativo.EnTaller, EstadoOperativo.EnLavado],
            tipo: TipoVehiculo.Seminuevo,
            precioMin: 400000m,
            precioMax: 500000m);

        Assert.Equal(2, resultado.TotalCount);
        Assert.All(resultado.Items, vehiculo =>
        {
            Assert.Contains(vehiculo.Estado, new[] { nameof(EstadoOperativo.EnTaller), nameof(EstadoOperativo.EnLavado) });
            Assert.Equal(nameof(TipoVehiculo.Seminuevo), vehiculo.Tipo);
            Assert.InRange(vehiculo.Precio, 400000m, 500000m);
        });
    }

    [Fact]
    public async Task GetConteosRapidosAsync_ExcluyeVendidosYDevuelveConteosCorrectos()
    {
        var vehiculos = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST24000000140", EstadoOperativo.Disponible, 300000m, TipoVehiculo.Nuevo),
            CrearVehiculoParaBusqueda(1, "Mazda", "CX-5", "ABC-1001", "3AUTST24000000141", EstadoOperativo.Disponible, 400000m, TipoVehiculo.Seminuevo),
            CrearVehiculoParaBusqueda(2, "Kia", "Sportage", "ABC-1002", "3AUTST24000000142", EstadoOperativo.EnTaller, 500000m, TipoVehiculo.Seminuevo),
            CrearVehiculoParaBusqueda(3, "Ford", "Maverick", "ABC-1003", "3AUTST24000000143", EstadoOperativo.Reservado, 520000m, TipoVehiculo.Nuevo),
            CrearVehiculoParaBusqueda(4, "Kia", "Rio", "VEN-1004", "3AUTST24000000144", EstadoOperativo.Vendido, 250000m, TipoVehiculo.Seminuevo),
            CrearVehiculoParaBusqueda(5, "Ford", "Explorer", "VEN-1005", "3AUTST24000000145", EstadoOperativo.Vendido, 700000m, TipoVehiculo.Nuevo),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(vehiculos);

        var resultado = await _vehiculosService.GetConteosRapidosAsync();

        Assert.Equal(4, resultado.Todos);
        Assert.Equal(2, resultado.Disponibles);
        Assert.Equal(2, resultado.Seminuevos);
        Assert.Equal(2, resultado.Nuevos);
    }

    private static RegistrarVehiculoDto CrearRegistroValido(
        TipoVehiculo tipo = TipoVehiculo.Seminuevo)
    {
        return new RegistrarVehiculoDto
        {
            Tipo = tipo,
            VIN = "3AUTST25000000999",
            Marca = "Toyota",
            Modelo = "Camry",
            Anio = 2025,
            Version = "XLE",
            Placa = tipo == TipoVehiculo.Seminuevo ? "PZG-999-A" : null,
            Kilometraje = tipo == TipoVehiculo.Seminuevo ? 14500 : 800,
            Transmision = "Automática",
            Motor = "2.5L 4 cilindros",
            Potencia = "203 HP",
            ColorExterior = "Blanco",
            ColorInterior = "Negro",
            Equipamiento = "Pantalla táctil, cámara 360",
            NotasInspeccion = "Unidad en excelente estado",
            Precio = 529900m,
            Estado = EstadoOperativo.Disponible,
            ZonaLote = "Z2",
            CajonLote = "B04",
        };
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_SeminuevoValido_GeneraStockConsecutivoYGuarda()
    {
        var existentes = new List<Vehiculo>
        {
            CrearVehiculoParaBusqueda(24, "Toyota", "Yaris", "ABC-1024", "3AUTST23000000025"),
        };

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(existentes);

        var dto = CrearRegistroValido(TipoVehiculo.Seminuevo);

        var resultado = await _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie");

        Assert.Equal("STK-0026", resultado.NumeroStock);
        Assert.Equal("3AUTST25000000999", resultado.VIN);
        Assert.Equal("PZG-999-A", resultado.Placa);
        Assert.Equal(14500, resultado.Kilometraje);
        Assert.Equal("Delia Leslie", resultado.RegistradoPor);
        Assert.Equal("XLE", resultado.Version);
        _vehiculoRepositoryMock.Verify(repo => repo.AgregarAsync(It.IsAny<Vehiculo>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_NuevoDePlanta_FijaKilometrajeEnCeroYSinPlaca()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(new List<Vehiculo>());

        var dto = CrearRegistroValido(TipoVehiculo.Nuevo);
        dto.Kilometraje = 2500;
        dto.Placa = "NO-DEBE-GUARDARSE";

        var resultado = await _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie");

        Assert.Equal(0, resultado.Kilometraje);
        Assert.Null(resultado.Placa);
        Assert.Equal(nameof(TipoVehiculo.Nuevo), resultado.Tipo);
        Assert.Equal("STK-0001", resultado.NumeroStock);
    }

    [Theory]
    [InlineData("CORT0123")]
    [InlineData("3AUTST25I00000999")]
    [InlineData("3AUTST25O00000999")]
    [InlineData("3AUTST25Q00000999")]
    public async Task RegistrarVehiculoAsync_VinInvalido_LanzaValidationAppException(string vinInvalido)
    {
        var dto = CrearRegistroValido();
        dto.VIN = vinInvalido;

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));

        Assert.Contains("17 caracteres", ex.Message);
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_VinDuplicado_LanzaValidationConIdDeUnidadExistente()
    {
        var existente = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000999");

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(new List<Vehiculo> { existente });
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorVinAsync("3AUTST25000000999"))
            .ReturnsAsync(existente);

        var dto = CrearRegistroValido();

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));

        Assert.Equal("Ya existe una unidad con ese VIN", ex.Message);
        Assert.Equal(existente.Id, ex.VehiculoExistenteId);
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_EstadoInicialVendido_LanzaValidationAppException()
    {
        var dto = CrearRegistroValido();
        dto.Estado = EstadoOperativo.Vendido;

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));

        Assert.Contains("Vendido", ex.Message);
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_MarcaYModeloIncoherentes_LanzaValidationAppException()
    {
        var dto = CrearRegistroValido();
        dto.Marca = "Honda";
        dto.Modelo = "Corolla";

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));

        Assert.Equal("El modelo no corresponde a la marca seleccionada", ex.Message);
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_CamposExcedenLongitudMaxima_LanzaValidationAppException()
    {
        var dto = CrearRegistroValido();
        dto.Marca = new string('A', 51);

        var exMarca = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));
        Assert.Equal("La marca no puede exceder 50 caracteres", exMarca.Message);

        dto = CrearRegistroValido();
        dto.ZonaLote = new string('Z', 21);
        var exZona = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));
        Assert.Equal("La zona del lote no puede exceder 20 caracteres", exZona.Message);

        dto = CrearRegistroValido();
        dto.Precio = 100000000m;
        var exPrecio = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));
        Assert.Equal("El precio no puede exceder 99,999,999.99", exPrecio.Message);

        dto = CrearRegistroValido();
        dto.Kilometraje = 1000001;
        var exKm = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));
        Assert.Equal("El kilometraje no puede exceder 1,000,000 km", exKm.Message);
    }

    [Fact]
    public async Task GuardarFotografiaAsync_TomaFrontal_EstableceFotoPrincipalYAvance25PorCiento()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000801");
        vehiculo.FotoPrincipalUrl = null;
        vehiculo.Imagenes = [];

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new GuardarFotografiaDto
        {
            TipoToma = "Frontal",
            ImagenBase64 = "data:image/jpeg;base64,FOTO_FRONTAL",
        };

        var resultado = await _vehiculosService.GuardarFotografiaAsync(
            vehiculo.Id,
            dto,
            "José de Jesús López Islas");

        Assert.Equal("data:image/jpeg;base64,FOTO_FRONTAL", resultado.FotoPrincipalUrl);
        Assert.Equal(1, resultado.TomasObligatoriasCompletadas);
        Assert.Equal(25, resultado.PorcentajeEvidencia);
        Assert.False(resultado.InspeccionCompleta);
        var imagen = Assert.Single(resultado.Imagenes);
        Assert.True(imagen.EsPrincipal);
        Assert.Equal("Frontal", imagen.TipoToma);
        Assert.Equal("José de Jesús López Islas", imagen.CapturadaPor);
    }

    [Fact]
    public async Task GuardarFotografiaAsync_CuatroTomasObligatorias_MarcaInspeccionCompletaAl100PorCiento()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000802");
        vehiculo.Imagenes =
        [
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Frontal", Url = "data:image/jpeg;base64,1", EsPrincipal = true, Orden = 0 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "LateralDerecha", Url = "data:image/jpeg;base64,2", Orden = 1 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Trasera", Url = "data:image/jpeg;base64,3", Orden = 2 },
        ];

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new GuardarFotografiaDto
        {
            TipoToma = "TableroOdometro",
            ImagenBase64 = "data:image/jpeg;base64,4",
        };

        var resultado = await _vehiculosService.GuardarFotografiaAsync(
            vehiculo.Id,
            dto,
            "José de Jesús López Islas");

        Assert.Equal(4, resultado.TomasObligatoriasCompletadas);
        Assert.Equal(100, resultado.PorcentajeEvidencia);
        Assert.True(resultado.InspeccionCompleta);
        Assert.Equal(4, resultado.Imagenes.Count);
    }

    [Fact]
    public async Task GuardarFotografiaAsync_SeptimaFotografia_LanzaValidationConMensajeMaximo6()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000803");
        vehiculo.Imagenes =
        [
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Frontal", Url = "1", EsPrincipal = true, Orden = 0 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "LateralDerecha", Url = "2", Orden = 1 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Trasera", Url = "3", Orden = 2 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "TableroOdometro", Url = "4", Orden = 3 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Adicional1", Url = "5", Orden = 4 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Adicional2", Url = "6", Orden = 5 },
        ];

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new GuardarFotografiaDto
        {
            TipoToma = "Adicional",
            ImagenBase64 = "data:image/jpeg;base64,7",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.GuardarFotografiaAsync(vehiculo.Id, dto, "José de Jesús"));

        Assert.Equal("Máximo 6 fotografías por unidad", ex.Message);
    }

    [Fact]
    public async Task GuardarFotografiaAsync_ReemplazarTomaExistenteConSeisFotos_PermiteReemplazarSinExcederLimite()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000804");
        vehiculo.Imagenes =
        [
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Frontal", Url = "1", EsPrincipal = true, Orden = 0 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "LateralDerecha", Url = "2", Orden = 1 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Trasera", Url = "3", Orden = 2 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "TableroOdometro", Url = "4", Orden = 3 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Adicional1", Url = "5", Orden = 4 },
            new ImagenVehiculo { Id = Guid.NewGuid(), VehiculoId = vehiculo.Id, TipoToma = "Adicional2", Url = "6", Orden = 5 },
        ];

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new GuardarFotografiaDto
        {
            TipoToma = "Frontal",
            ImagenBase64 = "data:image/jpeg;base64,NUEVA_FRONTAL",
        };

        var resultado = await _vehiculosService.GuardarFotografiaAsync(
            vehiculo.Id,
            dto,
            "José de Jesús");

        Assert.Equal(6, resultado.Imagenes.Count);
        Assert.Equal("data:image/jpeg;base64,NUEVA_FRONTAL", resultado.FotoPrincipalUrl);
    }

    [Fact]
    public async Task EliminarFotografiaAsync_EliminaTomaFrontal_LimpiaFotoPrincipalYRegresaAPendiente()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000805");
        var imagenFrontalId = Guid.NewGuid();
        vehiculo.FotoPrincipalUrl = "data:image/jpeg;base64,FRONTAL";
        vehiculo.Imagenes =
        [
            new ImagenVehiculo
            {
                Id = imagenFrontalId,
                VehiculoId = vehiculo.Id,
                TipoToma = "Frontal",
                Url = "data:image/jpeg;base64,FRONTAL",
                EsPrincipal = true,
                Orden = 0,
            },
        ];

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var resultado = await _vehiculosService.EliminarFotografiaAsync(vehiculo.Id, imagenFrontalId);

        Assert.Null(resultado.FotoPrincipalUrl);
        Assert.Empty(resultado.Imagenes);
        Assert.Equal(0, resultado.TomasObligatoriasCompletadas);
        Assert.Equal(0, resultado.PorcentajeEvidencia);
        Assert.False(resultado.InspeccionCompleta);
    }

    [Fact]
    public async Task EliminarFotografiaAsync_ImagenNoExiste_LanzaNotFoundException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-1000", "3AUTST25000000806");
        vehiculo.Imagenes = [];

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _vehiculosService.EliminarFotografiaAsync(vehiculo.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_CambioATestDrive_RegistraHistorialYResponsableTestDrive()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000901");
        vehiculo.Estado = EstadoOperativo.Disponible;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.TestDrive,
        };

        var resultado = await _vehiculosService.CambiarEstadoOperativoAsync(
            vehiculo.Id,
            dto,
            usuarioId: 2,
            nombreUsuario: "Marcos Hernández",
            rolUsuario: "Asesor de Ventas");

        Assert.Equal(nameof(EstadoOperativo.TestDrive), resultado.Estado);
        Assert.Equal("Marcos Hernández", resultado.ResponsableTestDrive);
        Assert.Equal("Marcos Hernández", resultado.EstadoActualizadoPor);
        Assert.NotNull(resultado.FechaCambioEstado);
        var historial = Assert.Single(resultado.HistorialEstados);
        Assert.Equal(nameof(EstadoOperativo.Disponible), historial.EstadoAnterior);
        Assert.Equal(nameof(EstadoOperativo.TestDrive), historial.EstadoNuevo);
        Assert.Equal("Marcos Hernández", historial.CambiadoPor);
        _vehiculoRepositoryMock.Verify(
            repo => repo.RegistrarCambioEstadoAsync(vehiculo, It.IsAny<HistorialEstadoVehiculo>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_CambioAReservado_RegistraUsuarioQueReservo()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000902");
        vehiculo.Estado = EstadoOperativo.Disponible;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.Reservado,
        };

        var resultado = await _vehiculosService.CambiarEstadoOperativoAsync(
            vehiculo.Id,
            dto,
            usuarioId: 4,
            nombreUsuario: "Marcos Hernández",
            rolUsuario: "Asesor de Ventas");

        Assert.Equal(nameof(EstadoOperativo.Reservado), resultado.Estado);
        Assert.Equal(4, resultado.ReservadoPorUsuarioId);
        Assert.Equal("Marcos Hernández", resultado.ReservadoPorNombre);
        Assert.Null(resultado.ResponsableTestDrive);
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_MismoEstadoActual_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000903");
        vehiculo.Estado = EstadoOperativo.EnTaller;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.EnTaller,
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.CambiarEstadoOperativoAsync(
                vehiculo.Id,
                dto,
                2,
                "Delia Leslie",
                "Encargado de Lote y Recepción"));

        Assert.Contains("distinto al actual", ex.Message);
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_IntentarMarcarComoVendido_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000904");
        vehiculo.Estado = EstadoOperativo.Disponible;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.Vendido,
        };

        await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.CambiarEstadoOperativoAsync(
                vehiculo.Id,
                dto,
                1,
                "Carlos Ramírez",
                "Administrador"));
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_VehiculoYaVendido_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000905");
        vehiculo.Estado = EstadoOperativo.Vendido;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.Disponible,
        };

        await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.CambiarEstadoOperativoAsync(
                vehiculo.Id,
                dto,
                1,
                "Carlos Ramírez",
                "Administrador"));
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_ReservadoPorOtroUsuarioSiendoAsesor_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000906");
        vehiculo.Estado = EstadoOperativo.Reservado;
        vehiculo.ReservadoPorUsuarioId = 10;
        vehiculo.ReservadoPorNombre = "Ana Gómez";

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.Disponible,
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.CambiarEstadoOperativoAsync(
                vehiculo.Id,
                dto,
                usuarioId: 2,
                nombreUsuario: "Marcos Hernández",
                rolUsuario: "Asesor de Ventas"));

        Assert.Contains("Ana Gómez", ex.Message);
    }

    [Fact]
    public async Task CambiarEstadoOperativoAsync_ReservadoPorOtroUsuarioSiendoGerenteOAdmin_PermiteLiberarUnidad()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Mazda", "CX-5", "ABC-1000", "3AUTST25000000907");
        vehiculo.Estado = EstadoOperativo.Reservado;
        vehiculo.ReservadoPorUsuarioId = 10;
        vehiculo.ReservadoPorNombre = "Ana Gómez";

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new CambiarEstadoOperativoDto
        {
            NuevoEstado = EstadoOperativo.Disponible,
        };

        var resultado = await _vehiculosService.CambiarEstadoOperativoAsync(
            vehiculo.Id,
            dto,
            usuarioId: 1,
            nombreUsuario: "Carlos Ramírez",
            rolUsuario: "Gerente de Ventas");

        Assert.Equal(nameof(EstadoOperativo.Disponible), resultado.Estado);
        Assert.Null(resultado.ReservadoPorUsuarioId);
        Assert.Null(resultado.ReservadoPorNombre);
    }

    [Fact]
    public async Task ObtenerZonasLoteAsync_RetornaZonasCorrectas()
    {
        var zonas = await _vehiculosService.ObtenerZonasLoteAsync();

        Assert.NotNull(zonas);
        Assert.Contains("Zona A", zonas);
        Assert.Contains("Zona B", zonas);
        Assert.Contains("Zona C", zonas);
        Assert.Contains("Taller", zonas);
        Assert.Contains("Lavado", zonas);
    }

    [Fact]
    public async Task ValidarCajonDisponibleAsync_CajonOcupado_RetornaOcupadoYDatosVehiculo()
    {
        var ocupante = CrearVehiculoParaBusqueda(0, "Honda", "Civic", "XYZ-789", "3AUTST25000000123");
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerOcupanteCajonAsync("Zona A", "14", null))
            .ReturnsAsync(ocupante);

        var resultado = await _vehiculosService.ValidarCajonDisponibleAsync("Zona A", "14");

        Assert.True(resultado.Ocupado);
        Assert.Equal("Honda", resultado.Marca);
        Assert.Equal("Civic", resultado.Modelo);
        Assert.NotNull(resultado.Mensaje);
        Assert.Contains("XYZ-789", resultado.Mensaje);
    }

    [Fact]
    public async Task ValidarCajonDisponibleAsync_CajonLibre_RetornaNoOcupado()
    {
        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerOcupanteCajonAsync("Zona B", "01", null))
            .ReturnsAsync((Vehiculo?)null);

        var resultado = await _vehiculosService.ValidarCajonDisponibleAsync("Zona B", "01");

        Assert.False(resultado.Ocupado);
        Assert.Null(resultado.Marca);
    }

    [Fact]
    public async Task ReubicarVehiculoAsync_SinPermiso_LanzaForbiddenAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        _permisosServiceMock
            .Setup(s => s.ValidarPermisoAsync(2, CodigosPermiso.AsignarCajon))
            .ThrowsAsync(new ForbiddenAppException("No tienes permiso para realizar esta acción"));

        var dto = new ReubicarVehiculoDto
        {
            Zona = "Zona B",
            Cajon = "12",
        };

        var ex = await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _vehiculosService.ReubicarVehiculoAsync(vehiculo.Id, dto, usuarioId: 2, nombreUsuario: "Asesor"));

        Assert.Equal("No tienes permiso para realizar esta acción", ex.Message);
    }

    [Fact]
    public async Task ReubicarVehiculoAsync_MismaUbicacion_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.ZonaLote = "Zona A";
        vehiculo.CajonLote = "05";

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ReubicarVehiculoDto
        {
            Zona = "Zona A",
            Cajon = "05",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ReubicarVehiculoAsync(vehiculo.Id, dto, usuarioId: 1, nombreUsuario: "Encargado"));

        Assert.Contains("ya se encuentra en la zona y cajón", ex.Message);
    }

    [Fact]
    public async Task ReubicarVehiculoAsync_CajonOcupado_LanzaValidationAppExceptionConDatosVehiculo()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.ZonaLote = "Zona A";
        vehiculo.CajonLote = "05";

        var ocupante = CrearVehiculoParaBusqueda(1, "Nissan", "Versa", "PQR-456", "3AUTST25000000789");

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerOcupanteCajonAsync("Zona B", "10", vehiculo.Id))
            .ReturnsAsync(ocupante);

        var dto = new ReubicarVehiculoDto
        {
            Zona = "Zona B",
            Cajon = "10",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ReubicarVehiculoAsync(vehiculo.Id, dto, usuarioId: 1, nombreUsuario: "Encargado"));

        Assert.Contains("El cajón ya está ocupado", ex.Message);
        Assert.Contains("Nissan", ex.Message);
        Assert.Contains("Versa", ex.Message);
        Assert.Contains("PQR-456", ex.Message);
    }

    [Fact]
    public async Task ReubicarVehiculoAsync_Exito_ActualizaUbicacionYRegistraHistorial()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.ZonaLote = "Zona A";
        vehiculo.CajonLote = "05";

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerOcupanteCajonAsync("Zona C", "22", vehiculo.Id))
            .ReturnsAsync((Vehiculo?)null);

        var dto = new ReubicarVehiculoDto
        {
            Zona = "Zona C",
            Cajon = "22",
        };

        var resultado = await _vehiculosService.ReubicarVehiculoAsync(
            vehiculo.Id,
            dto,
            usuarioId: 1,
            nombreUsuario: "Juan Pérez");

        Assert.Equal("Zona C", resultado.ZonaLote);
        Assert.Equal("22", resultado.CajonLote);
        Assert.Equal("Juan Pérez", resultado.ReubicadoPor);
        Assert.NotNull(resultado.FechaReubicacion);

        _vehiculoRepositoryMock.Verify(
            repo => repo.RegistrarReubicacionAsync(
                vehiculo,
                It.Is<HistorialUbicacionVehiculo>(
                    h => h.ZonaAnterior == "Zona A" &&
                         h.CajonAnterior == "05" &&
                         h.ZonaNueva == "Zona C" &&
                         h.CajonNuevo == "22" &&
                         h.ReubicadoPor == "Juan Pérez")),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegistrarVehiculoAsync_CajonOcupado_LanzaValidationAppExceptionConDatosVehiculo()
    {
        var dto = CrearRegistroValido();
        dto.ZonaLote = "Zona A";
        dto.CajonLote = "15";

        var ocupante = CrearVehiculoParaBusqueda(1, "Mazda", "3", "MZD-999", "3AUTST25000000888");

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerTodosAsync())
            .ReturnsAsync(new List<Vehiculo>());

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerOcupanteCajonAsync("Zona A", "15", null))
            .ReturnsAsync(ocupante);

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.RegistrarVehiculoAsync(dto, "Delia Leslie"));

        Assert.Contains("El cajón ya está ocupado", ex.Message);
        Assert.Contains("Mazda", ex.Message);
        Assert.Contains("3", ex.Message);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_KmMayorOIgual_ActualizaYRegistraHistorial()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.Kilometraje = 25000;
        vehiculo.NotasInspeccion = "Nota anterior";

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 26500,
            NotasInspeccion = "Inspección de rutina completada sin novedades.",
        };

        var resultado = await _vehiculosService.ActualizarKilometrajeYNotasAsync(
            vehiculo.Id,
            dto,
            usuarioId: 10,
            nombreUsuario: "Delia Leslie",
            rolUsuario: "Encargado de Inventario");

        Assert.Equal(26500, resultado.Kilometraje);
        Assert.Equal("Inspección de rutina completada sin novedades.", resultado.NotasInspeccion);
        Assert.Equal("Delia Leslie", resultado.KilometrajeActualizadoPor);
        Assert.NotNull(resultado.FechaActualizacionKilometraje);
        Assert.Single(resultado.HistorialKilometrajes);
        Assert.Equal(25000, resultado.HistorialKilometrajes[0].KilometrajeAnterior);
        Assert.Equal(26500, resultado.HistorialKilometrajes[0].KilometrajeNuevo);

        _vehiculoRepositoryMock.Verify(
            repo => repo.RegistrarActualizacionKilometrajeAsync(
                vehiculo,
                It.Is<HistorialKilometrajeVehiculo>(
                    h => h.KilometrajeAnterior == 25000 &&
                         h.KilometrajeNuevo == 26500 &&
                         h.ActualizadoPor == "Delia Leslie")),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_UnidadVendida_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.Estado = EstadoOperativo.Vendido;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 50000,
            NotasInspeccion = "Nueva nota",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ActualizarKilometrajeYNotasAsync(
                vehiculo.Id,
                dto,
                usuarioId: 1,
                nombreUsuario: "Delia Leslie",
                rolUsuario: "Gerente de Ventas"));

        Assert.Contains("Las unidades con estado Vendido no se pueden editar", ex.Message);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_KmMenorSiendoEncargado_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.Kilometraje = 40000;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 35000,
            NotasInspeccion = "Corrección por error",
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ActualizarKilometrajeYNotasAsync(
                vehiculo.Id,
                dto,
                usuarioId: 2,
                nombreUsuario: "Operador Patio",
                rolUsuario: "Encargado de Inventario"));

        Assert.Contains("El kilometraje no puede ser menor al registrado", ex.Message);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_KmMenorSiendoGerenteSinMotivo_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.Kilometraje = 40000;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 35000,
            NotasInspeccion = "Corrección",
            MotivoCorreccion = "abc", // Menor a 5 caracteres
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ActualizarKilometrajeYNotasAsync(
                vehiculo.Id,
                dto,
                usuarioId: 1,
                nombreUsuario: "Gerente Demo",
                rolUsuario: "Gerente de Ventas"));

        Assert.Contains("Debes escribir el motivo de la corrección en las notas para registrar un kilometraje menor", ex.Message);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_KmMenorSiendoGerenteConMotivo_ActualizaExitosamente()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");
        vehiculo.Kilometraje = 40000;

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 35000,
            NotasInspeccion = "Se corrigió lectura previa",
            MotivoCorreccion = "Error tipográfico al capturar el odómetro en la recepción",
        };

        var resultado = await _vehiculosService.ActualizarKilometrajeYNotasAsync(
            vehiculo.Id,
            dto,
            usuarioId: 1,
            nombreUsuario: "Gerente Demo",
            rolUsuario: "Gerente de Ventas");

        Assert.Equal(35000, resultado.Kilometraje);
        Assert.Single(resultado.HistorialKilometrajes);
        Assert.Equal("Error tipográfico al capturar el odómetro en la recepción", resultado.HistorialKilometrajes[0].MotivoCorreccion);
        Assert.Equal(40000, resultado.HistorialKilometrajes[0].KilometrajeAnterior);
        Assert.Equal(35000, resultado.HistorialKilometrajes[0].KilometrajeNuevo);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_KmInvalido_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = -50,
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ActualizarKilometrajeYNotasAsync(
                vehiculo.Id,
                dto,
                usuarioId: 1,
                nombreUsuario: "Gerente Demo",
                rolUsuario: "Gerente de Ventas"));

        Assert.Contains("El kilometraje debe ser un número entero entre 0 y 999,999 km", ex.Message);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_NotasExcedenLimite_LanzaValidationAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");

        _vehiculoRepositoryMock
            .Setup(repo => repo.ObtenerPorIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 50000,
            NotasInspeccion = new string('A', 501),
        };

        var ex = await Assert.ThrowsAsync<ValidationAppException>(
            () => _vehiculosService.ActualizarKilometrajeYNotasAsync(
                vehiculo.Id,
                dto,
                usuarioId: 1,
                nombreUsuario: "Gerente Demo",
                rolUsuario: "Gerente de Ventas"));

        Assert.Contains("Las notas de inspección no pueden exceder 500 caracteres", ex.Message);
    }

    [Fact]
    public async Task ActualizarKilometrajeYNotasAsync_SinPermiso_LanzaForbiddenAppException()
    {
        var vehiculo = CrearVehiculoParaBusqueda(0, "Toyota", "Corolla", "ABC-123", "3AUTST25000000456");

        _permisosServiceMock
            .Setup(p => p.ValidarPermisoAsync(99, CodigosPermiso.EditarKilometrajeNotas))
            .ThrowsAsync(new ForbiddenAppException("No tienes permiso"));

        var dto = new ActualizarKilometrajeDto
        {
            Kilometraje = 50000,
        };

        await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _vehiculosService.ActualizarKilometrajeYNotasAsync(
                vehiculo.Id,
                dto,
                usuarioId: 99,
                nombreUsuario: "Usuario Sin Permiso",
                rolUsuario: "Vendedor"));
    }
}
