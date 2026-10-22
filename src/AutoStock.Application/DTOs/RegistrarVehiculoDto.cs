using AutoStock.Domain.Enums;

namespace AutoStock.Application.DTOs;

public class RegistrarVehiculoDto
{
    public TipoVehiculo Tipo { get; set; } = TipoVehiculo.Seminuevo;

    public string VIN { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public string? Version { get; set; }

    public string? Placa { get; set; }

    public int Kilometraje { get; set; }

    public string? Transmision { get; set; }

    public string? Motor { get; set; }

    public string? Potencia { get; set; }

    public string? ColorExterior { get; set; }

    public string? ColorInterior { get; set; }

    public string? Equipamiento { get; set; }

    public string? NotasInspeccion { get; set; }

    public decimal Precio { get; set; }

    public EstadoOperativo Estado { get; set; } = EstadoOperativo.Disponible;

    public string ZonaLote { get; set; } = string.Empty;

    public string CajonLote { get; set; } = string.Empty;
}
