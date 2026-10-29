namespace AutoStock.Application.DTOs;

public class PermisoDto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Modulo { get; set; } = string.Empty;

    public bool EsFijo { get; set; }
}
