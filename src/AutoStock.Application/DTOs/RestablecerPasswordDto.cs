using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs;

public class RestablecerPasswordDto
{
    [Required(ErrorMessage = "El token es obligatorio")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres, con letras y números")]
    public string PasswordNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
