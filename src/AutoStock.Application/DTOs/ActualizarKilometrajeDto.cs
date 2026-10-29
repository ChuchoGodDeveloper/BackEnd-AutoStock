using System.ComponentModel.DataAnnotations;

namespace AutoStock.Application.DTOs;

public class ActualizarKilometrajeDto
{
    [Required(ErrorMessage = "El kilometraje es obligatorio")]
    [Range(0, 999999, ErrorMessage = "El kilometraje debe ser un número entero entre 0 y 999,999 km")]
    public int Kilometraje { get; set; }

    [MaxLength(500, ErrorMessage = "Las notas de inspección no pueden exceder 500 caracteres")]
    public string? NotasInspeccion { get; set; }

    [MaxLength(500, ErrorMessage = "El motivo de la corrección no puede exceder 500 caracteres")]
    public string? MotivoCorreccion { get; set; }
}
