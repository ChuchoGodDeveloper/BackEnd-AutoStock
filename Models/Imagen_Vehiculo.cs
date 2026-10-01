namespace AutoStock.Models;

public class Imagen_Vehiculo
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }

    public Vehiculo Vehiculo { get; set; } = null!;
}
