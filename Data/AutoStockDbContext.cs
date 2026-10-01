using AutoStock.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Data;

public class AutoStockDbContext : DbContext
{
    public AutoStockDbContext(DbContextOptions<AutoStockDbContext> options) : base(options) { }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Sesion> Sesiones { get; set; }
    public DbSet<Estado_Operativo> EstadoOperativos { get; set; }
    public DbSet<Ubicacion_Lote> UbicacionLotes { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Imagen_Vehiculo> ImagenesVehiculo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreCompleto)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.IdEmpleado)
                .IsRequired()
                .HasMaxLength(50);
            entity.HasIndex(e => e.IdEmpleado).IsUnique();
            entity.Property(e => e.Correo)
                .IsRequired()
                .HasMaxLength(200);
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Activo)
                .HasDefaultValue(true);
            entity.Property(e => e.IntentosFallidos)
                .HasDefaultValue(0);
            entity.Property(e => e.BloqueadoHasta)
                .IsRequired(false);
            entity.Property(e => e.RequiereCambioPassword)
                .HasDefaultValue(false);

            entity.HasOne(e => e.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(e => e.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sesion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.FechaCreacion)
                .IsRequired();
            entity.Property(e => e.FechaExpiracion)
                .IsRequired();
            entity.Property(e => e.Activa)
                .HasDefaultValue(true);

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Sesiones)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Estado_Operativo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ColorHex)
                .IsRequired()
                .HasMaxLength(7);
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<Ubicacion_Lote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Zona)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Cajon)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Vin)
                .IsRequired()
                .HasMaxLength(17);
            entity.HasIndex(e => e.Vin).IsUnique();
            entity.Property(e => e.Placa)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Marca)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Modelo)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Anio)
                .IsRequired();
            entity.Property(e => e.Precio)
                .IsRequired()
                .HasPrecision(18, 2);
            entity.Property(e => e.Kilometraje)
                .IsRequired();
            entity.Property(e => e.FechaRegistro)
                .IsRequired();

            entity.HasOne(e => e.EstadoOperativo)
                .WithMany(eo => eo.Vehiculos)
                .HasForeignKey(e => e.EstadoOperativoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UbicacionLote)
                .WithMany(ul => ul.Vehiculos)
                .HasForeignKey(e => e.UbicacionLoteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Imagen_Vehiculo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.EsPrincipal)
                .HasDefaultValue(false);

            entity.HasOne(e => e.Vehiculo)
                .WithMany(v => v.Imagenes)
                .HasForeignKey(e => e.VehiculoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Asesor de Ventas" },
            new Rol { Id = 2, Nombre = "Encargado de Lote y Recepción" },
            new Rol { Id = 3, Nombre = "Gerente de Ventas" },
            new Rol { Id = 4, Nombre = "Administrador" }
        );

        modelBuilder.Entity<Estado_Operativo>().HasData(
            new Estado_Operativo { Id = 1, Nombre = "Disponible", ColorHex = "#28a745" },
            new Estado_Operativo { Id = 2, Nombre = "Test Drive", ColorHex = "#007bff" },
            new Estado_Operativo { Id = 3, Nombre = "En Taller", ColorHex = "#fd7e14" },
            new Estado_Operativo { Id = 4, Nombre = "En Lavado", ColorHex = "#ffc107" },
            new Estado_Operativo { Id = 5, Nombre = "Reservado", ColorHex = "#dc3545" },
            new Estado_Operativo { Id = 6, Nombre = "Vendido", ColorHex = "#6c757d" }
        );

        modelBuilder.Entity<Ubicacion_Lote>().HasData(
            new Ubicacion_Lote { Id = 1, Zona = "A", Cajon = "A-01" },
            new Ubicacion_Lote { Id = 2, Zona = "A", Cajon = "A-02" },
            new Ubicacion_Lote { Id = 3, Zona = "B", Cajon = "B-01" },
            new Ubicacion_Lote { Id = 4, Zona = "B", Cajon = "B-02" },
            new Ubicacion_Lote { Id = 5, Zona = "C", Cajon = "C-01" }
        );
    }
}
