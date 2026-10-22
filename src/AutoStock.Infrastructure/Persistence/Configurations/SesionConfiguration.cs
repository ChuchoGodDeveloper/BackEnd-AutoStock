using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("Sesiones");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.UsuarioId)
            .IsRequired();

        builder.Property(s => s.TokenJti)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.TokenJti)
            .IsUnique();

        builder.Property(s => s.FechaInicio)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(s => s.FechaExpiracion)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(s => s.Activa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.FechaCierre)
            .HasColumnType("datetime2");

        builder.HasOne(s => s.Usuario)
            .WithMany(u => u.Sesiones)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
