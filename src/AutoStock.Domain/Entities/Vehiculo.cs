using AutoStock.Domain.Enums;

namespace AutoStock.Domain.Entities;

public class Vehiculo
{
    public Guid Id { get; set; }

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public string? Version { get; set; }

    public decimal Precio { get; set; }

    public int Kilometraje { get; set; }

    public string? Placa { get; set; }

    public string ZonaLote { get; set; } = string.Empty;

    public string CajonLote { get; set; } = string.Empty;

    public string? FotoPrincipalUrl { get; set; }

    public EstadoOperativo Estado { get; set; }

    public TipoVehiculo Tipo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? RegistradoPor { get; set; }

    public DateTime? FechaCambioEstado { get; set; }

    public string? EstadoActualizadoPor { get; set; }

    public int? EstadoActualizadoPorUsuarioId { get; set; }

    public string? ResponsableTestDrive { get; set; }

    public int? ReservadoPorUsuarioId { get; set; }

    public string? ReservadoPorNombre { get; set; }

    public DateTime? FechaReubicacion { get; set; }

    public string? ReubicadoPor { get; set; }

    public int? ReubicadoPorUsuarioId { get; set; }

    public DateTime? FechaActualizacionKilometraje { get; set; }

    public string? KilometrajeActualizadoPor { get; set; }

    public int? KilometrajeActualizadoPorUsuarioId { get; set; }

    public string VIN { get; set; } = string.Empty;

    public string NumeroStock { get; set; } = string.Empty;

    public string? Motor { get; set; }

    public string? Potencia { get; set; }

    public string? Transmision { get; set; }

    public string? ColorExterior { get; set; }

    public string? ColorInterior { get; set; }

    public string? Equipamiento { get; set; }

    public string? NotasInspeccion { get; set; }

    public List<ImagenVehiculo> Imagenes { get; set; } = [];

    public List<HistorialEstadoVehiculo> HistorialEstados { get; set; } = [];

    public List<HistorialUbicacionVehiculo> HistorialUbicaciones { get; set; } = [];

    public List<HistorialKilometrajeVehiculo> HistorialKilometrajes { get; set; } = [];
}
