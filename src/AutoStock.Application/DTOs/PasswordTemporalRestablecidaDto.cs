namespace AutoStock.Application.DTOs;

public class PasswordTemporalRestablecidaDto
{
    public int Id { get; set; }

    public string EmpleadoId { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string PasswordTemporal { get; set; } = string.Empty;
}
