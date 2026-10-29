using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class HistorialKilometrajeVehiculoConfiguration : IEntityTypeConfiguration<HistorialKilometrajeVehiculo>
{
    public void Configure(EntityTypeBuilder<HistorialKilometrajeVehiculo> builder)
    {
        builder.ToTable("HistorialKilometrajesVehiculo");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.VehiculoId)
            .IsRequired();

        builder.Property(h => h.KilometrajeAnterior)
            .IsRequired();

        builder.Property(h => h.KilometrajeNuevo)
            .IsRequired();

        builder.Property(h => h.NotasInspeccion)
            .HasMaxLength(500);

        builder.Property(h => h.MotivoCorreccion)
            .HasMaxLength(500);

        builder.Property(h => h.UsuarioId);

        builder.Property(h => h.ActualizadoPor)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.FechaActualizacion)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasIndex(h => h.VehiculoId);

        builder.HasIndex(h => h.FechaActualizacion);

        builder.Navigation(h => h.Vehiculo)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
