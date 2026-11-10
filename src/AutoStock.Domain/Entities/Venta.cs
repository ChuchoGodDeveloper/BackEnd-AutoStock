namespace AutoStock.Domain.Entities;

public class Venta
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public DateTime FechaVenta { get; set; }

    public int AsesorId { get; set; }

    public string AsesorNombre { get; set; } = string.Empty;

    public decimal PrecioFinal { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public int RegistradoPorUsuarioId { get; set; }

    public string RegistradoPorNombre { get; set; } = string.Empty;

    public bool Anulada { get; set; }

    public string? MotivoAnulacion { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public int? AnuladaPorUsuarioId { get; set; }

    public string? AnuladaPorNombre { get; set; }

    public Vehiculo? Vehiculo { get; set; }
}

