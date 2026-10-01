namespace AutoStock.DTOs;

public class PaginacionDto<T>
{
    public int TotalUnidades { get; set; }
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}
