namespace AutoStock.Application.DTOs;

public class RegistrarVentaDto
{
    public DateTime FechaVenta { get; set; }

    public int AsesorId { get; set; }

    public decimal PrecioFinal { get; set; }
}

public class RevertirVentaDto
{
    public string Motivo { get; set; } = string.Empty;

    public string ZonaLote { get; set; } = string.Empty;

    public string CajonLote { get; set; } = string.Empty;
}

public class VentaDto
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public DateTime FechaVenta { get; set; }

    public int AsesorId { get; set; }

    public string AsesorNombre { get; set; } = string.Empty;

    public decimal PrecioFinal { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string RegistradoPorNombre { get; set; } = string.Empty;

    public bool Anulada { get; set; }

    public string? MotivoAnulacion { get; set; }
}

