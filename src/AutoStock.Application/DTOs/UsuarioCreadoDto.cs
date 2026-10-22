namespace AutoStock.Application.DTOs;

public class UsuarioCreadoDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string EmpleadoId { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public bool RequiereCambioPassword { get; set; }

    public string PasswordTemporal { get; set; } = string.Empty;
}
