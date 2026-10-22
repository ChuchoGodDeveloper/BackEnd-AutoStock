namespace AutoStock.Application.DTOs;

public class HistorialEstadoDto
{
    public Guid Id { get; set; }

    public string EstadoAnterior { get; set; } = string.Empty;

    public string EstadoNuevo { get; set; } = string.Empty;

    public int? UsuarioId { get; set; }

    public string CambiadoPor { get; set; } = string.Empty;

    public DateTime FechaCambio { get; set; }
}
