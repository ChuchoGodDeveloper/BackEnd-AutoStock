namespace AutoStock.Application.DTOs;

public class ElementoReporteVentaDto
{
    public Guid VentaId { get; set; }

    public DateTime FechaVenta { get; set; }

    public Guid VehiculoId { get; set; }

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public string NumeroStock { get; set; } = string.Empty;

    public string Vin { get; set; } = string.Empty;

    public int AsesorId { get; set; }

    public string AsesorNombre { get; set; } = string.Empty;

    public decimal PrecioFinal { get; set; }
}

public class ReporteVentasDto
{
    public string Periodo { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int TotalUnidades { get; set; }

    public decimal MontoTotal { get; set; }

    public string GeneradoPor { get; set; } = string.Empty;

    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<ElementoReporteVentaDto> Ventas { get; set; } = [];
}

