using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.PrecioFinal)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.FechaVenta)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(v => v.FechaRegistro)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(v => v.AsesorNombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(v => v.RegistradoPorNombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(v => v.MotivoAnulacion)
            .HasMaxLength(500);

        builder.Property(v => v.AnuladaPorNombre)
            .HasMaxLength(150);

        builder.Property(v => v.FechaAnulacion)
            .HasColumnType("datetime2");

        builder.HasOne(v => v.Vehiculo)
            .WithMany(veh => veh.Ventas)
            .HasForeignKey(v => v.VehiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.VehiculoId);
        builder.HasIndex(v => v.FechaVenta);
        builder.HasIndex(v => v.Anulada);
    }
}

