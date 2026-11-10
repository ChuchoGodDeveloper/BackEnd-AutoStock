namespace AutoStock.Application.DTOs;

public class ResumenMetricasDto
{
    public int Disponibles { get; set; }

    public int PruebaDeManejo { get; set; }

    public int EnTaller { get; set; }

    public DateTime FechaHoraServidor { get; set; } = DateTime.UtcNow;

    public List<MovimientoRecienteDto> UltimosMovimientos { get; set; } = [];
}
