using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
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

        builder.Property(v => v.Version)
            .HasMaxLength(100);

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

        builder.Property(v => v.FotoPrincipalUrl);

        builder.Property(v => v.Estado)
            .IsRequired();

        builder.Property(v => v.Tipo)
            .IsRequired()
            .HasDefaultValue(TipoVehiculo.Seminuevo);

        builder.Property(v => v.FechaRegistro)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(v => v.RegistradoPor)
            .HasMaxLength(150);

        builder.Property(v => v.VIN)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(v => v.NumeroStock)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Motor)
            .HasMaxLength(100);

        builder.Property(v => v.Potencia)
            .HasMaxLength(50);

        builder.Property(v => v.Transmision)
            .HasMaxLength(50);

        builder.Property(v => v.ColorExterior)
            .HasMaxLength(50);

        builder.Property(v => v.ColorInterior)
            .HasMaxLength(50);

        builder.Property(v => v.Equipamiento)
            .HasMaxLength(500);

        builder.Property(v => v.NotasInspeccion)
            .HasMaxLength(1000);

        builder.HasMany(v => v.Imagenes)
            .WithOne(i => i.Vehiculo)
            .HasForeignKey(i => i.VehiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.Estado);

        builder.HasIndex(v => v.FechaRegistro);

        builder.HasIndex(v => v.VIN)
            .IsUnique();

        builder.HasIndex(v => v.NumeroStock)
            .IsUnique();
    }
}
