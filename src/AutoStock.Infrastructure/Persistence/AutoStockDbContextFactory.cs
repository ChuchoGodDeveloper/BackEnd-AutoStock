using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoStock.Infrastructure.Persistence;

public class AutoStockDbContextFactory : IDesignTimeDbContextFactory<AutoStockDbContext>
{
    public AutoStockDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AutoStockDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=AutoStockDb;Trusted_Connection=True;TrustServerCertificate=True");

        return new AutoStockDbContext(optionsBuilder.Options);
    }
}
