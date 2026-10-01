namespace AutoStock.Models;

public class Vehiculo
{
    public int Id { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public decimal Precio { get; set; }
    public int Kilometraje { get; set; }
    public int EstadoOperativoId { get; set; }
    public int UbicacionLoteId { get; set; }
    public DateTime FechaRegistro { get; set; }

    public Estado_Operativo EstadoOperativo { get; set; } = null!;
    public Ubicacion_Lote UbicacionLote { get; set; } = null!;
    public ICollection<Imagen_Vehiculo> Imagenes { get; set; } = new List<Imagen_Vehiculo>();
}
