namespace AutoStock.Application.DTOs;

public class UsuarioListaDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string EmpleadoId { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
