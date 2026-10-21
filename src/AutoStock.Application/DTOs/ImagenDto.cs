namespace AutoStock.Application.DTOs;

public class ImagenDto
{
    public string Url { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; }

    public int Orden { get; set; }
}
