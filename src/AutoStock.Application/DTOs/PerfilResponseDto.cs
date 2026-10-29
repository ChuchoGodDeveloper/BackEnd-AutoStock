namespace AutoStock.Application.DTOs;

public class PerfilResponseDto
{
    public int Id { get; set; }

    public string EmpleadoId { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public string Rol { get; set; } = string.Empty;

    public List<string> Permisos { get; set; } = new();

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}
