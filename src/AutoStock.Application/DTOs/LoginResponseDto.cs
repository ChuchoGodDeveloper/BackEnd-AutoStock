namespace AutoStock.Application.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEn { get; set; }

    public UsuarioLoginDto Usuario { get; set; } = null!;
}
