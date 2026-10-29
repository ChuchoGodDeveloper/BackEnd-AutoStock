namespace AutoStock.Domain.Entities;

public class TokenRecuperacionPassword
{
    public Guid Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string Token { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; }

    public DateTime? FechaUso { get; set; }
}
