namespace AutoStock.Domain.Entities;

public class OrdenServicio
{
    public Guid Id { get; set; }

    public string Folio { get; set; } = string.Empty;

    public Guid VehiculoId { get; set; }

    public string TipoTrabajo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Prioridad { get; set; } = "Media";

    public decimal? CostoEstimado { get; set; }

    public string Responsable { get; set; } = string.Empty;

    public DateTime? FechaEstimada { get; set; }

    public string Estado { get; set; } = "Abierta";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int CreadoPorUsuarioId { get; set; }

    public string CreadoPorNombre { get; set; } = string.Empty;

    public DateTime? FechaCierre { get; set; }

    public int? CerradoPorUsuarioId { get; set; }

    public string? CerradoPorNombre { get; set; }

    public decimal? CostoFinal { get; set; }

    public string? EstadoSalida { get; set; }

    public Vehiculo? Vehiculo { get; set; }

    public List<NotaAvanceOrden> NotasAvance { get; set; } = [];
}

