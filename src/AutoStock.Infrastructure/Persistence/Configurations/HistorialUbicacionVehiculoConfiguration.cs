using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class HistorialUbicacionVehiculoConfiguration : IEntityTypeConfiguration<HistorialUbicacionVehiculo>
{
    public void Configure(EntityTypeBuilder<HistorialUbicacionVehiculo> builder)
    {
        builder.ToTable("HistorialUbicacionesVehiculo");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.VehiculoId)
            .IsRequired();

        builder.Property(h => h.ZonaAnterior)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(h => h.CajonAnterior)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(h => h.ZonaNueva)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(h => h.CajonNuevo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(h => h.UsuarioId);

        builder.Property(h => h.ReubicadoPor)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.FechaMovimiento)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasIndex(h => h.VehiculoId);

        builder.HasIndex(h => h.FechaMovimiento);

        builder.Navigation(h => h.Vehiculo)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
