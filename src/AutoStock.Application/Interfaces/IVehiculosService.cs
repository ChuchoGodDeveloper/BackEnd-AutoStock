using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IVehiculosService
{
    Task<PaginatedResult<VehiculoDto>> GetCatalogoAsync(int page = 1, int pageSize = 20, string? busqueda = null);

    Task<VehiculoDetalleDto> GetVehiculoByIdAsync(Guid id);
}
