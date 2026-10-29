namespace AutoStock.Application.DTOs;

public class ActualizarRolPermisosDto
{
    public int RolId { get; set; }

    public Dictionary<string, bool> Permisos { get; set; } = new();
}

public class ActualizarMatrizPermisosRequestDto
{
    public List<ActualizarRolPermisosDto> Roles { get; set; } = new();
}
