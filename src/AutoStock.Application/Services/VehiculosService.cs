using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
using AutoStock.Domain.Interfaces;

namespace AutoStock.Application.Services;

public class VehiculosService : IVehiculosService
{
    private const int TamanioPaginaPorDefecto = 20;
    private const int TamanioPaginaMaximo = 100;

    private readonly IVehiculoRepository _vehiculoRepository;

    public VehiculosService(IVehiculoRepository vehiculoRepository)
    {
        _vehiculoRepository = vehiculoRepository;
    }

    public async Task<PaginatedResult<VehiculoDto>> GetCatalogoAsync(
        int page = 1,
        int pageSize = 20,
        string? busqueda = null,
        List<EstadoOperativo>? estados = null,
        TipoVehiculo? tipo = null,
        decimal? precioMin = null,
        decimal? precioMax = null)
    {
        var vehiculos = await _vehiculoRepository.ObtenerTodosAsync();

        IEnumerable<Vehiculo> catalogo = vehiculos;

        if (estados is { Count: > 0 })
        {
            catalogo = catalogo.Where(v => estados.Contains(v.Estado));
        }
        else
        {
            catalogo = catalogo.Where(v => v.Estado != EstadoOperativo.Vendido);
        }

        if (tipo.HasValue)
        {
            catalogo = catalogo.Where(v => v.Tipo == tipo.Value);
        }

        if (precioMin.HasValue)
        {
            catalogo = catalogo.Where(v => v.Precio >= precioMin.Value);
        }

        if (precioMax.HasValue)
        {
            catalogo = catalogo.Where(v => v.Precio <= precioMax.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = NormalizarTexto(busqueda);

            catalogo = catalogo.Where(v =>
                Contiene(v.Marca, termino) ||
                Contiene(v.Modelo, termino) ||
                Contiene(v.Anio.ToString(), termino) ||
                Contiene(v.VIN, termino) ||
                Contiene(v.Placa, termino));
        }

        var catalogoFiltrado = catalogo
            .OrderByDescending(v => v.FechaRegistro)
            .ToList();

        var pagina = page < 1 ? 1 : page;
        var tamanio = pageSize < 1
            ? TamanioPaginaPorDefecto
            : Math.Min(pageSize, TamanioPaginaMaximo);

        var items = catalogoFiltrado
            .Skip((pagina - 1) * tamanio)
            .Take(tamanio)
            .Select(CrearDto)
            .ToList();

        return new PaginatedResult<VehiculoDto>
        {
            Items = items,
            TotalCount = catalogoFiltrado.Count,
            Page = pagina,
            PageSize = tamanio,
        };
    }

    public async Task<ConteosRapidosDto> GetConteosRapidosAsync()
    {
        var vehiculos = await _vehiculoRepository.ObtenerTodosAsync();

        var activos = vehiculos
            .Where(v => v.Estado != EstadoOperativo.Vendido)
            .ToList();

        return new ConteosRapidosDto
        {
            Todos = activos.Count,
            Disponibles = activos.Count(v => v.Estado == EstadoOperativo.Disponible),
            Seminuevos = activos.Count(v => v.Tipo == TipoVehiculo.Seminuevo),
            Nuevos = activos.Count(v => v.Tipo == TipoVehiculo.Nuevo),
        };
    }

    public async Task<VehiculoDetalleDto> GetVehiculoByIdAsync(Guid id)
    {
        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(id);

        if (vehiculo is null)
        {
            throw new NotFoundException("Vehículo no encontrado");
        }

        return CrearDetalleDto(vehiculo);
    }

    private static string NormalizarTexto(string valor)
    {
        return valor.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
    }

    private static bool Contiene(string? valor, string terminoNormalizado)
    {
        return !string.IsNullOrWhiteSpace(valor) &&
               NormalizarTexto(valor).Contains(terminoNormalizado, StringComparison.Ordinal);
    }

    private static VehiculoDto CrearDto(Vehiculo vehiculo) => new()
    {
        Id = vehiculo.Id,
        Marca = vehiculo.Marca,
        Modelo = vehiculo.Modelo,
        Anio = vehiculo.Anio,
        Precio = vehiculo.Precio,
        Kilometraje = vehiculo.Kilometraje,
        Placa = vehiculo.Placa,
        ZonaLote = vehiculo.ZonaLote,
        CajonLote = vehiculo.CajonLote,
        FotoPrincipalUrl = vehiculo.FotoPrincipalUrl,
        Estado = vehiculo.Estado.ToString(),
        Tipo = vehiculo.Tipo.ToString(),
        FechaRegistro = vehiculo.FechaRegistro,
    };

    private static VehiculoDetalleDto CrearDetalleDto(Vehiculo vehiculo) => new()
    {
        Id = vehiculo.Id,
        Marca = vehiculo.Marca,
        Modelo = vehiculo.Modelo,
        Anio = vehiculo.Anio,
        Precio = vehiculo.Precio,
        Kilometraje = vehiculo.Kilometraje,
        Placa = vehiculo.Placa,
        ZonaLote = vehiculo.ZonaLote,
        CajonLote = vehiculo.CajonLote,
        FotoPrincipalUrl = vehiculo.FotoPrincipalUrl,
        Estado = vehiculo.Estado.ToString(),
        FechaRegistro = vehiculo.FechaRegistro,
        VIN = vehiculo.VIN,
        NumeroStock = vehiculo.NumeroStock,
        Motor = vehiculo.Motor,
        Potencia = vehiculo.Potencia,
        Transmision = vehiculo.Transmision,
        ColorExterior = vehiculo.ColorExterior,
        ColorInterior = vehiculo.ColorInterior,
        Equipamiento = vehiculo.Equipamiento,
        NotasInspeccion = vehiculo.NotasInspeccion,
        Imagenes = vehiculo.Imagenes
            .OrderBy(imagen => imagen.EsPrincipal ? 0 : 1)
            .ThenBy(imagen => imagen.Orden)
            .Select(imagen => new ImagenDto
            {
                Url = imagen.Url,
                EsPrincipal = imagen.EsPrincipal,
                Orden = imagen.Orden,
            })
            .ToList(),
    };
}
