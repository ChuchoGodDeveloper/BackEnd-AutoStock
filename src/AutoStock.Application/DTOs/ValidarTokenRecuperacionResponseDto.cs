namespace AutoStock.Application.DTOs;

public class ValidarTokenRecuperacionResponseDto
{
    public bool EsValido { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public string? CorreoEnmascarado { get; set; }
}
