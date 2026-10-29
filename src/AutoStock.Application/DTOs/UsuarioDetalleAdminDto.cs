namespace AutoStock.Application.DTOs;

public class UsuarioDetalleAdminDto
{
    public int Id { get; set; }

    public string EmpleadoId { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public string Rol { get; set; } = string.Empty;

    public int RolId { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool RequiereCambioPassword { get; set; }
}
