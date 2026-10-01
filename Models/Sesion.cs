namespace AutoStock.Models;

public class Sesion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool Activa { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
