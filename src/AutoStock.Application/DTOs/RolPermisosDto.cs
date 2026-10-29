namespace AutoStock.Application.DTOs;

public class RolPermisosDto
{
    public int RolId { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public Dictionary<string, bool> Permisos { get; set; } = new();
}
