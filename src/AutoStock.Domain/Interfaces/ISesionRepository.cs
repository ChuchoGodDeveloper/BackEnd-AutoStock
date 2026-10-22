using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface ISesionRepository
{
    Task AddAsync(Sesion sesion);

    Task<Sesion?> GetByJtiAsync(string jti);

    void Update(Sesion sesion);
}
