using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface IPermisoRepository
{
    Task<List<Permiso>> GetAllPermisosAsync();

    Task<List<RolPermiso>> GetAllRolPermisosAsync();

    Task<List<RolPermiso>> GetRolPermisosByRolIdAsync(int rolId);

    Task<RolPermiso?> GetRolPermisoAsync(int rolId, int permisoId);

    Task AddRolPermisosAsync(IEnumerable<RolPermiso> rolPermisos);
}
