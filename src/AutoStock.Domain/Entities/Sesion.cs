namespace AutoStock.Domain.Entities;

public class Sesion
{
    public Guid Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string TokenJti { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime? FechaCierre { get; set; }
}
