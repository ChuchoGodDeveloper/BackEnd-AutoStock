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

    public DbSet<HistorialUbicacionVehiculo> HistorialUbicacionesVehiculo => Set<HistorialUbicacionVehiculo>();

    public DbSet<Permiso> Permisos => Set<Permiso>();

    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoStockDbContext).Assembly);
    }
}
