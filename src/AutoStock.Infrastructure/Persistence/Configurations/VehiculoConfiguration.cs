using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable("Vehiculos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.Marca)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Modelo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Anio)
            .IsRequired();

        builder.Property(v => v.Precio)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.Kilometraje)
            .IsRequired();

        builder.Property(v => v.Placa)
            .HasMaxLength(20);

        builder.Property(v => v.ZonaLote)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.CajonLote)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.FotoPrincipalUrl)
            .HasMaxLength(255);

        builder.Property(v => v.Estado)
            .IsRequired();

        builder.Property(v => v.FechaRegistro)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasIndex(v => v.Estado);

        builder.HasIndex(v => v.FechaRegistro);
    }
}
