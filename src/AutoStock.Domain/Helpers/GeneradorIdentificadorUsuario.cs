using System.Globalization;
using System.Text;

namespace AutoStock.Domain.Helpers;

public static class GeneradorIdentificadorUsuario
{
    private static readonly HashSet<string> PalabrasAIgnorar = new(StringComparer.OrdinalIgnoreCase)
    {
        "DE", "DEL", "LA", "LAS", "LOS", "Y", "E", "SAN", "SANTA"
    };

    public static string ObtenerIniciales(string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            return "USR";
        }

        var normalizado = NormalizarTexto(nombreCompleto);
        var partes = normalizado
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !PalabrasAIgnorar.Contains(p) && p.Length > 0)
            .ToList();

        if (partes.Count == 0)
        {
            partes = normalizado
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        if (partes.Count == 0)
        {
            return "USR";
        }

        if (partes.Count == 1)
        {
            var unica = partes[0];
            return unica.Length >= 2 ? unica[..2] : unica.PadRight(2, 'X');
        }

        var iniciales = string.Concat(partes.Take(4).Select(p => p[0]));
        return iniciales.Length < 2 ? iniciales.PadRight(2, 'X') : iniciales;
    }

    public static string FormatearFechaAAMMDD(DateOnly fechaNacimiento)
    {
        var anio = (fechaNacimiento.Year % 100).ToString("D2");
        var mes = fechaNacimiento.Month.ToString("D2");
        var dia = fechaNacimiento.Day.ToString("D2");
        return $"{anio}{mes}{dia}";
    }

    public static string GenerarBase(string nombreCompleto, DateOnly fechaNacimiento)
    {
        var iniciales = ObtenerIniciales(nombreCompleto);
        var fechaStr = FormatearFechaAAMMDD(fechaNacimiento);
        return $"{iniciales}-{fechaStr}";
    }

    public static string GenerarConConsecutivo(string baseId, int consecutivo)
    {
        return $"{baseId}-{consecutivo:D2}";
    }

    private static string NormalizarTexto(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetter(c))
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append(' ');
                }
            }
        }

        return sb.ToString();
    }
}
