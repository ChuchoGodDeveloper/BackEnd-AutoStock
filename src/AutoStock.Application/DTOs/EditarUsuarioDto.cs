using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs;

public class EditarUsuarioDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 150 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
    [MaxLength(100, ErrorMessage = "El correo electrónico no puede exceder 100 caracteres")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio")]
    public string Rol { get; set; } = string.Empty;

    public DateOnly? FechaNacimiento { get; set; }
}
