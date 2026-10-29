using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IUsuariosService
{
    Task<UsuarioCreadoDto> CrearUsuarioAsync(CrearUsuarioDto dto);

    Task<IReadOnlyList<UsuarioListaDto>> ObtenerUsuariosAsync();

    Task<UsuarioDetalleAdminDto> ObtenerUsuarioPorIdAsync(int id);

    Task<UsuarioListaDto> EditarUsuarioAsync(int id, EditarUsuarioDto dto, int administradorAutenticadoId);

    Task<UsuarioListaDto> CambiarEstatusUsuarioAsync(int id, CambiarEstatusUsuarioDto dto, int administradorAutenticadoId);

    Task<PasswordTemporalRestablecidaDto> RestablecerPasswordPorAdminAsync(int id);
}
