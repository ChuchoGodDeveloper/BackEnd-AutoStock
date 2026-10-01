namespace AutoStock.Models;

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string IdEmpleado { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public int IntentosFallidos { get; set; } = 0;
    public DateTime? BloqueadoHasta { get; set; }
    public bool RequiereCambioPassword { get; set; }

    public Rol Rol { get; set; } = null!;
    public ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();
}
