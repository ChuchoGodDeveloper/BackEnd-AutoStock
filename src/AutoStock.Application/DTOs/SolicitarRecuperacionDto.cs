using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs;

public class SolicitarRecuperacionDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
    public string Correo { get; set; } = string.Empty;
}
