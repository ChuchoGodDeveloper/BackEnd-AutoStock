using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class SesionRepository : ISesionRepository
{
    private readonly AutoStockDbContext _context;

    public SesionRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Sesion sesion)
    {
        await _context.Sesiones.AddAsync(sesion);
    }

    public async Task<Sesion?> GetByJtiAsync(string jti)
    {
        return await _context.Sesiones
            .FirstOrDefaultAsync(s => s.TokenJti == jti);
    }

    public void Update(Sesion sesion)
    {
        _context.Sesiones.Update(sesion);
    }
}
