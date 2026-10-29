using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class RolRepository : IRolRepository
{
    private readonly AutoStockDbContext _context;

    public RolRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<Rol?> GetByIdAsync(int id)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Rol>> GetAllAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.Id)
            .ToListAsync();
    }
}
