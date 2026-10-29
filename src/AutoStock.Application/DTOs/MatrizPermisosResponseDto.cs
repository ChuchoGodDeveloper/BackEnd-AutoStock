namespace AutoStock.Application.DTOs;

public class MatrizPermisosResponseDto
{
    public List<PermisoDto> Acciones { get; set; } = new();

    public List<RolPermisosDto> Roles { get; set; } = new();

    public DateTime? UltimaModificacion { get; set; }

    public string? ModificadoPorNombre { get; set; }
}
