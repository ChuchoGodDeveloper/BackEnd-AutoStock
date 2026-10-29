namespace AutoStock.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }

    public string EmpleadoId { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public bool RequiereCambioPassword { get; set; }

    public int IntentosFallidos { get; set; }

    public DateTime? BloqueadoHasta { get; set; }

    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();
}
