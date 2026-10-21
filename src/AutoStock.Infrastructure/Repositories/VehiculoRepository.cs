using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly AutoStockDbContext _context;

    public VehiculoRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Vehiculo>> ObtenerTodosAsync()
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Vehiculo?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .Include(v => v.Imagenes)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}
