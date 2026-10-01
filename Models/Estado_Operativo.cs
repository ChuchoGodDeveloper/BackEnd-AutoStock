namespace AutoStock.Models;

public class Estado_Operativo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
