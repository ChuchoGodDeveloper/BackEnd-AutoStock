using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface ITokenRecuperacionRepository
{
    Task<TokenRecuperacionPassword?> ObtenerPorTokenAsync(string token);

    Task<IReadOnlyList<TokenRecuperacionPassword>> ObtenerActivosPorUsuarioIdAsync(int usuarioId);

    Task<int> ContarSolicitudesRecientesPorCorreoAsync(string correo, DateTime desde);

    Task AgregarAsync(TokenRecuperacionPassword tokenRecuperacion);

    void Actualizar(TokenRecuperacionPassword tokenRecuperacion);
}
