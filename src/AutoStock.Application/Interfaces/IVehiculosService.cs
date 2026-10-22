using AutoStock.Application.DTOs;
using AutoStock.Domain.Enums;

namespace AutoStock.Application.Interfaces;

public interface IVehiculosService
{
    Task<PaginatedResult<VehiculoDto>> GetCatalogoAsync(
        int page = 1,
        int pageSize = 20,
        string? busqueda = null,
        List<EstadoOperativo>? estados = null,
        TipoVehiculo? tipo = null,
        decimal? precioMin = null,
        decimal? precioMax = null);

    Task<VehiculoDetalleDto> GetVehiculoByIdAsync(Guid id);

    Task<ConteosRapidosDto> GetConteosRapidosAsync();

    Task<VehiculoDetalleDto> RegistrarVehiculoAsync(RegistrarVehiculoDto dto, string registradoPor);
}
