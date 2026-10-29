namespace AutoStock.Application.DTOs;

public class HistorialKilometrajeDto
{
    public Guid Id { get; set; }

    public int KilometrajeAnterior { get; set; }

    public int KilometrajeNuevo { get; set; }

    public string? NotasInspeccion { get; set; }

    public string? MotivoCorreccion { get; set; }

    public string ActualizadoPor { get; set; } = string.Empty;

    public DateTime FechaActualizacion { get; set; }
}
