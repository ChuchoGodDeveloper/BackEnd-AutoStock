namespace AutoStock.Models;

public class Ubicacion_Lote
{
    public int Id { get; set; }
    public string Zona { get; set; } = string.Empty;
    public string Cajon { get; set; } = string.Empty;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
