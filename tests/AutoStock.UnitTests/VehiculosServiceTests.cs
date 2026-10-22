using AutoStock.Application.DTOs;
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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VehiculosService _vehiculosService;

    public VehiculosServiceTests()
    {
        _vehiculosService = new VehiculosService(
            _vehiculoRepositoryMock.Object,
            _unitOfWorkMock.Object);
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
}
