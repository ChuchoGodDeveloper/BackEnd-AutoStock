namespace AutoStock.Application.DTOs;

public class MovimientoRecienteDto
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public string VehiculoTitulo { get; set; } = string.Empty;

    public string NumeroStock { get; set; } = string.Empty;

    public string EstadoAnterior { get; set; } = string.Empty;

    public string EstadoNuevo { get; set; } = string.Empty;

    public string CambiadoPor { get; set; } = string.Empty;

    public DateTime FechaCambio { get; set; }
}

public class MetricasCompletasLoteDto
{
    public int TotalActivas { get; set; }

    public int Disponibles { get; set; }

    public int EnTestDrive { get; set; }

    public int EnTaller { get; set; }

    public int EnLavado { get; set; }

    public int Reservadas { get; set; }

    public int Nuevas { get; set; }

    public int Seminuevas { get; set; }

    public int VentasMesActual { get; set; }

    public decimal PromedioDiasEnLote { get; set; }

    public DateTime FechaHoraServidor { get; set; } = DateTime.UtcNow;

    public List<MovimientoRecienteDto> UltimosMovimientos { get; set; } = [];
}

