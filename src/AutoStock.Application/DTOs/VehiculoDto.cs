namespace AutoStock.Application.DTOs;

public class VehiculoDto
{
    public Guid Id { get; set; }

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public decimal Precio { get; set; }

    public int Kilometraje { get; set; }

    public string? Placa { get; set; }

    public string ZonaLote { get; set; } = string.Empty;

    public string CajonLote { get; set; } = string.Empty;

    public string? FotoPrincipalUrl { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }
}
