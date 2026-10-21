using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.EmpleadoId)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.EmpleadoId)
            .IsUnique();

        builder.Property(u => u.Correo)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Correo)
            .IsUnique();

        builder.Property(u => u.NombreCompleto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.IntentosFallidos)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.BloqueadoHasta)
            .HasColumnType("datetime2");

        builder.Property(u => u.RolId)
            .IsRequired();

        builder.Property(u => u.FechaCreacion)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Sesiones)
            .WithOne(s => s.Usuario)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
