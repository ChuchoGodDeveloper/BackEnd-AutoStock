using System.ComponentModel.DataAnnotations;

namespace AutoStock.DTOs;

public class LoginRequestDto
{
    [Required]
    public string Credencial { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
