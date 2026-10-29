using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IPermisosService
{
    Task<MatrizPermisosResponseDto> GetMatrizPermisosAsync();

    Task<List<string>> ObtenerCodigosPermisosActivosPorRolAsync(int rolId, string nombreRol);

    Task ValidarPermisoAsync(int usuarioId, string codigoPermiso);

    Task<RespuestaOperacionDto> ActualizarMatrizPermisosAsync(
        ActualizarMatrizPermisosRequestDto dto,
        int usuarioId,
        string nombreUsuario);

    Task<RespuestaOperacionDto> RestablecerMatrizBaseAsync(
        int usuarioId,
        string nombreUsuario);
}
