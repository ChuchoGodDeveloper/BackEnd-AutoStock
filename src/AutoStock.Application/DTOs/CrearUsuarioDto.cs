namespace AutoStock.Application.DTOs;

public class CrearUsuarioDto
{
    public string EmpleadoId { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public int RolId { get; set; }
}
