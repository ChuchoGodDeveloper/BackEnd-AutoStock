namespace AutoStock.Domain.Entities;

public class ImagenVehiculo
{
    public Guid Id { get; set; }

    public Guid VehiculoId { get; set; }

    public string Url { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; }

    public int Orden { get; set; }

    public string TipoToma { get; set; } = "Frontal";

    public DateTime FechaCaptura { get; set; } = DateTime.UtcNow;

    public string? CapturadaPor { get; set; }

    public Vehiculo? Vehiculo { get; set; }
}
