using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El identificador es obligatorio")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}
