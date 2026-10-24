using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoStock.Infrastructure.Persistence;

public class AutoStockDbContextFactory : IDesignTimeDbContextFactory<AutoStockDbContext>
{
    public AutoStockDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? (args.Length > 0 ? args[0] : null)
            ?? "Server=localhost;Database=AutoStockDb;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<AutoStockDbContext>();

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null));

        return new AutoStockDbContext(optionsBuilder.Options);
    }
}
