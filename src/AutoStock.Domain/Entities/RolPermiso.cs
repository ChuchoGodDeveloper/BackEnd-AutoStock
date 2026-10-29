namespace AutoStock.Domain.Entities;

public class RolPermiso
{
    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public int PermisoId { get; set; }

    public Permiso Permiso { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

    public int? ModificadoPorUsuarioId { get; set; }

    public string? ModificadoPorNombre { get; set; }
}
