namespace AutoStock.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool RequiereCambioPassword { get; set; }
}
