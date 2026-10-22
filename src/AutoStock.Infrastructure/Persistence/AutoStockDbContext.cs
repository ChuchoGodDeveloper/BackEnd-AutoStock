using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Persistence;

public class AutoStockDbContext : DbContext
{
    public AutoStockDbContext(DbContextOptions<AutoStockDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Sesion> Sesiones => Set<Sesion>();

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();

    public DbSet<ImagenVehiculo> ImagenesVehiculo => Set<ImagenVehiculo>();

    public DbSet<HistorialEstadoVehiculo> HistorialEstadosVehiculo => Set<HistorialEstadoVehiculo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoStockDbContext).Assembly);
    }
}
