using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IUsuariosService
{
    Task<UsuarioCreadoDto> CrearUsuarioAsync(CrearUsuarioDto dto);

    Task<IReadOnlyList<UsuarioListaDto>> ObtenerUsuariosAsync();
}
