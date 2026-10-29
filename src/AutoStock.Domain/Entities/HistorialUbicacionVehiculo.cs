namespace AutoStock.Domain.Entities;

public class HistorialUbicacionVehiculo
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public string ZonaAnterior { get; set; } = string.Empty;

    public string CajonAnterior { get; set; } = string.Empty;

    public string ZonaNueva { get; set; } = string.Empty;

    public string CajonNuevo { get; set; } = string.Empty;

    public int? UsuarioId { get; set; }

    public string ReubicadoPor { get; set; } = string.Empty;

    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    public Vehiculo? Vehiculo { get; set; }
}
