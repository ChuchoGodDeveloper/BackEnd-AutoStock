using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class ImagenVehiculoConfiguration : IEntityTypeConfiguration<ImagenVehiculo>
{
    public void Configure(EntityTypeBuilder<ImagenVehiculo> builder)
    {
        builder.ToTable("ImagenesVehiculo");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.VehiculoId)
            .IsRequired();

        builder.Property(i => i.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.EsPrincipal)
            .IsRequired();

        builder.Property(i => i.Orden)
            .IsRequired();

        builder.HasIndex(i => i.VehiculoId);

        builder.Navigation(i => i.Vehiculo)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
