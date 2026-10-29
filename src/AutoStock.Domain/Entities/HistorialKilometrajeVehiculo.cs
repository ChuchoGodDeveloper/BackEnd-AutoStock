namespace AutoStock.Domain.Entities;

public class HistorialKilometrajeVehiculo
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public int KilometrajeAnterior { get; set; }

    public int KilometrajeNuevo { get; set; }

    public string? NotasInspeccion { get; set; }

    public string? MotivoCorreccion { get; set; }

    public int? UsuarioId { get; set; }

    public string ActualizadoPor { get; set; } = string.Empty;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public Vehiculo? Vehiculo { get; set; }
}
