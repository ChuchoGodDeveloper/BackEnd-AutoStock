namespace AutoStock.Domain.Entities;

public class NotaAvanceOrden
{
    public Guid Id { get; set; }

    public Guid OrdenServicioId { get; set; }

    public string Nota { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int CreadoPorUsuarioId { get; set; }

    public string CreadoPorNombre { get; set; } = string.Empty;

    public OrdenServicio? OrdenServicio { get; set; }
}

