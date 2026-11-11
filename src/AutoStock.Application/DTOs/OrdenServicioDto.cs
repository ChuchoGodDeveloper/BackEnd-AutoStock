namespace AutoStock.Application.DTOs;

public class CrearOrdenServicioDto
{
    public string TipoTrabajo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Prioridad { get; set; } = "Media";

    public decimal? CostoEstimado { get; set; }

    public string Responsable { get; set; } = string.Empty;

    public DateTime? FechaEstimada { get; set; }
}

public class CerrarOrdenServicioDto
{
    public decimal CostoFinal { get; set; }

    public string EstadoSalida { get; set; } = "Disponible";
}

public class AgregarNotaAvanceDto
{
    public string Nota { get; set; } = string.Empty;
}

public class NotaAvanceDto
{
    public Guid Id { get; set; }

    public string Nota { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public int CreadoPorUsuarioId { get; set; }

    public string CreadoPorNombre { get; set; } = string.Empty;
}

public class OrdenServicioDto
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

    public DateTime FechaCreacion { get; set; }

    public int CreadoPorUsuarioId { get; set; }

    public string CreadoPorNombre { get; set; } = string.Empty;

    public DateTime? FechaCierre { get; set; }

    public string? CerradoPorNombre { get; set; }

    public decimal? CostoFinal { get; set; }

    public string? EstadoSalida { get; set; }

    public IReadOnlyList<NotaAvanceDto> NotasAvance { get; set; } = [];
}

