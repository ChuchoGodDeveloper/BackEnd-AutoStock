namespace AutoStock.DTOs;

public class VehiculoCatalogoDto
{
    public int Id { get; set; }
    public string? FotoPrincipalUrl { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public decimal PrecioMXN { get; set; }
    public int Kilometraje { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string Cajon { get; set; } = string.Empty;
    public string EstadoOperativo { get; set; } = string.Empty;
    public string ColorEstado { get; set; } = string.Empty;
}
