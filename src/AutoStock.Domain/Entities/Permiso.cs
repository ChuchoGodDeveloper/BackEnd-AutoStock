namespace AutoStock.Domain.Entities;

public class Permiso
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Modulo { get; set; } = string.Empty;

    public bool EsFijo { get; set; }

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
