using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs;

public class ReubicarVehiculoDto
{
    [Required(ErrorMessage = "La zona es obligatoria")]
    [MaxLength(20, ErrorMessage = "La zona no puede exceder 20 caracteres")]
    public string Zona { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cajón es obligatorio")]
    [MaxLength(6, ErrorMessage = "El cajón no puede exceder 6 caracteres")]
    public string Cajon { get; set; } = string.Empty;
}
