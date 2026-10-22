namespace AutoStock.Application.DTOs;

public class ImagenDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; }

    public int Orden { get; set; }

    public string TipoToma { get; set; } = "Frontal";

    public DateTime FechaCaptura { get; set; }

    public string? CapturadaPor { get; set; }
}
