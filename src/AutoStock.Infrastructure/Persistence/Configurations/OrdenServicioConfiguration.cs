using AutoStock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStock.Infrastructure.Persistence.Configurations;

public class OrdenServicioConfiguration : IEntityTypeConfiguration<OrdenServicio>
{
    public void Configure(EntityTypeBuilder<OrdenServicio> builder)
    {
        builder.ToTable("OrdenesServicio");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.Folio)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(o => o.Folio)
            .IsUnique();

        builder.Property(o => o.TipoTrabajo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Prioridad)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.CostoEstimado)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.CostoFinal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Responsable)
            .HasMaxLength(150);

        builder.Property(o => o.Estado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.CreadoPorNombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(o => o.CerradoPorNombre)
            .HasMaxLength(150);

        builder.Property(o => o.EstadoSalida)
            .HasMaxLength(50);

        builder.Property(o => o.FechaCreacion)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(o => o.FechaEstimada)
            .HasColumnType("datetime2");

        builder.Property(o => o.FechaCierre)
            .HasColumnType("datetime2");

        builder.HasOne(o => o.Vehiculo)
            .WithMany(v => v.OrdenesServicio)
            .HasForeignKey(o => o.VehiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.NotasAvance)
            .WithOne(n => n.OrdenServicio)
            .HasForeignKey(n => n.OrdenServicioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotaAvanceOrdenConfiguration : IEntityTypeConfiguration<NotaAvanceOrden>
{
    public void Configure(EntityTypeBuilder<NotaAvanceOrden> builder)
    {
        builder.ToTable("NotasAvanceOrdenes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.Nota)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.CreadoPorNombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(n => n.FechaCreacion)
            .IsRequired()
            .HasColumnType("datetime2");
    }
}

