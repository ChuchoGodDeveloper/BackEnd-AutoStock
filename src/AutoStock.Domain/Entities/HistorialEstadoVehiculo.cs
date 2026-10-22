using AutoStock.Domain.Enums;

namespace AutoStock.Domain.Entities;

public class HistorialEstadoVehiculo
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public EstadoOperativo EstadoAnterior { get; set; }

    public EstadoOperativo EstadoNuevo { get; set; }

    public int? UsuarioId { get; set; }

    public string CambiadoPor { get; set; } = string.Empty;

    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    public Vehiculo? Vehiculo { get; set; }
}
