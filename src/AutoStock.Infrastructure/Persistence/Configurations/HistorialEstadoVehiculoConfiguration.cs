using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class HistorialEstadoVehiculoConfiguration : IEntityTypeConfiguration<HistorialEstadoVehiculo>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoVehiculo> builder)
    {
        builder.ToTable("HistorialEstadosVehiculo");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.VehiculoId)
            .IsRequired();

        builder.Property(h => h.EstadoAnterior)
            .IsRequired();

        builder.Property(h => h.EstadoNuevo)
            .IsRequired();

        builder.Property(h => h.UsuarioId);

        builder.Property(h => h.CambiadoPor)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.FechaCambio)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasIndex(h => h.VehiculoId);

        builder.HasIndex(h => h.FechaCambio);

        builder.Navigation(h => h.Vehiculo)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
