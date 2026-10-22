using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;

namespace AutoStock.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AutoStockDbContext _context;

    public UnitOfWork(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
