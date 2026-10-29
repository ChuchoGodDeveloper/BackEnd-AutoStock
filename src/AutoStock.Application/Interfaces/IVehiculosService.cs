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

    Task<ConteosRapidosDto> GetConteosRapidosAsync();

    Task<ResumenMetricasDto> GetResumenMetricasAsync();

    Task<VehiculoDetalleDto> GetVehiculoByIdAsync(Guid id);

    Task<VehiculoDetalleDto> RegistrarVehiculoAsync(
        RegistrarVehiculoDto dto,
        string registradoPor,
        int? usuarioId = null);

    Task<VehiculoDetalleDto> GuardarFotografiaAsync(
        Guid vehiculoId,
        GuardarFotografiaDto dto,
        string capturadaPor,
        int? usuarioId = null);

    Task<VehiculoDetalleDto> EliminarFotografiaAsync(
        Guid vehiculoId,
        Guid imagenId,
        int? usuarioId = null);

    Task<VehiculoDetalleDto> CambiarEstadoOperativoAsync(
        Guid vehiculoId,
        CambiarEstadoOperativoDto dto,
        int? usuarioId,
        string nombreUsuario,
        string rolUsuario);

    Task<IReadOnlyList<string>> ObtenerZonasLoteAsync();

    Task<VehiculoOcupanteDto> ValidarCajonDisponibleAsync(
        string zona,
        string cajon,
        Guid? ignorarVehiculoId = null);

    Task<VehiculoDetalleDto> ReubicarVehiculoAsync(
        Guid vehiculoId,
        ReubicarVehiculoDto dto,
        int? usuarioId,
        string nombreUsuario);
}
