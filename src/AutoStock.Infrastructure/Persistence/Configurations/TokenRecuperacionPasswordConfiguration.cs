using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class TokenRecuperacionPasswordConfiguration : IEntityTypeConfiguration<TokenRecuperacionPassword>
{
    public void Configure(EntityTypeBuilder<TokenRecuperacionPassword> builder)
    {
        builder.ToTable("TokensRecuperacionPassword");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(t => t.Token);

        builder.Property(t => t.Correo)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Correo);

        builder.Property(t => t.FechaCreacion)
            .IsRequired();

        builder.Property(t => t.FechaExpiracion)
            .IsRequired();

        builder.Property(t => t.Usado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.FechaUso)
            .IsRequired(false);

        builder.HasOne(t => t.Usuario)
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
