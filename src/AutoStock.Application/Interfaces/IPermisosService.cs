using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IPermisosService
{
    Task<MatrizPermisosResponseDto> GetMatrizPermisosAsync();

    Task<RespuestaOperacionDto> ActualizarMatrizPermisosAsync(
        ActualizarMatrizPermisosRequestDto dto,
        int usuarioId,
        string nombreUsuario);

    Task<RespuestaOperacionDto> RestablecerMatrizBaseAsync(
        int usuarioId,
        string nombreUsuario);
}
