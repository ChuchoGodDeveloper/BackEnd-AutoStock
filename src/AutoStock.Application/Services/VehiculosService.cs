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

    public async Task<PaginatedResult<VehiculoDto>> GetCatalogoAsync(int page = 1, int pageSize = 20)
    {
        var vehiculos = await _vehiculoRepository.ObtenerTodosAsync();

        var catalogo = vehiculos
            .Where(v => v.Estado != EstadoOperativo.Vendido)
            .OrderByDescending(v => v.FechaRegistro)
            .ToList();

        var pagina = page < 1 ? 1 : page;
        var tamanio = pageSize < 1
            ? TamanioPaginaPorDefecto
            : Math.Min(pageSize, TamanioPaginaMaximo);

        var items = catalogo
            .Skip((pagina - 1) * tamanio)
            .Take(tamanio)
            .Select(CrearDto)
            .ToList();

        return new PaginatedResult<VehiculoDto>
        {
            Items = items,
            TotalCount = catalogo.Count,
            Page = pagina,
            PageSize = tamanio,
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
