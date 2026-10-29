namespace AutoStock.Application.DTOs;

public class CrearUsuarioDto
{
    public string? EmpleadoId { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public int RolId { get; set; }
}
