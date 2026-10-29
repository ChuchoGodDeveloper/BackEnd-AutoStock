using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface IRolRepository
{
    Task<Rol?> GetByIdAsync(int id);

    Task<List<Rol>> GetAllAsync();
}
