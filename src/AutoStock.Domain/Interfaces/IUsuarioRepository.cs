using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdentifierAsync(string identificador);

    Task<Usuario?> GetByIdAsync(int id);

    void Update(Usuario usuario);
}
