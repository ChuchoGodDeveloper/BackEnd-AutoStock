using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class PermisoRepository : IPermisoRepository
{
    private readonly AutoStockDbContext _context;

    public PermisoRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permiso>> GetAllPermisosAsync()
    {
        return await _context.Permisos
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<List<RolPermiso>> GetAllRolPermisosAsync()
    {
        return await _context.RolPermisos
            .Include(rp => rp.Rol)
            .Include(rp => rp.Permiso)
            .ToListAsync();
    }

    public async Task<List<RolPermiso>> GetRolPermisosByRolIdAsync(int rolId)
    {
        return await _context.RolPermisos
            .Include(rp => rp.Permiso)
            .Where(rp => rp.RolId == rolId)
            .ToListAsync();
    }

    public async Task<RolPermiso?> GetRolPermisoAsync(int rolId, int permisoId)
    {
        return await _context.RolPermisos
            .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);
    }

    public async Task AddRolPermisosAsync(IEnumerable<RolPermiso> rolPermisos)
    {
        await _context.RolPermisos.AddRangeAsync(rolPermisos);
    }
}
