namespace AutoStock.Application.DTOs;

public class HistorialUbicacionDto
{
    public Guid Id { get; set; }

    public string ZonaAnterior { get; set; } = string.Empty;

    public string CajonAnterior { get; set; } = string.Empty;

    public string ZonaNueva { get; set; } = string.Empty;

    public string CajonNuevo { get; set; } = string.Empty;

    public string ReubicadoPor { get; set; } = string.Empty;

    public DateTime FechaMovimiento { get; set; }
}
