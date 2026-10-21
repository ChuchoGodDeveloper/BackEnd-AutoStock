using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdentifierAsync(string identificador);

    Task<Usuario?> GetByIdAsync(int id);

    Task<bool> ExisteEmpleadoIdOCorreoAsync(string empleadoId, string correo);

    Task AddAsync(Usuario usuario);

    Task<IReadOnlyList<Usuario>> GetAllAsync();

    void Update(Usuario usuario);
}
