namespace AutoStock.Domain.Constants;

public static class CatalogoMarcasModelos
{
    private static readonly Dictionary<string, HashSet<string>> MarcasYModelos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Toyota"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Corolla", "RAV4", "Tacoma", "Yaris", "Camry", "Hilux", "Prius", "Avanza", "Highlander"
        },
        ["Nissan"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Versa", "Kicks", "Sentra", "Frontier", "March", "X-Trail", "Altima", "NP300", "Urvan"
        },
        ["Chevrolet"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Aveo", "Tracker", "Onix", "Captiva", "Silverado", "S10", "Groove", "Cavalier", "Trax"
        },
        ["Mazda"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "3", "CX-5", "CX-30", "2", "CX-50", "CX-90", "MX-5"
        },
        ["Volkswagen"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Jetta", "Tiguan", "Taos", "Virtus", "Polo", "Nivus", "Saveiro", "T-Cross", "Teramont"
        },
        ["Honda"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Civic", "CR-V", "HR-V", "Accord", "City", "BR-V", "Pilot", "Odyssey"
        },
        ["Kia"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Sportage", "Rio", "Forte", "Seltos", "K3", "Soul", "Sorento", "Carnival"
        },
        ["Hyundai"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Elantra", "Tucson", "Creta", "Grand i10", "Santa Fe", "HB20", "Palisade"
        },
        ["Ford"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Maverick", "Explorer", "Ranger", "Bronco", "Bronco Sport", "Escape", "F-150", "Mustang", "Territory"
        },
        ["Renault"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Kwid", "Duster", "Oroch", "Stepway", "Koleos", "Megane", "Captur"
        },
        ["MG"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "ZS", "GT", "MG5", "RX5", "ONE", "MG4", "HS"
        },
        ["Suzuki"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Swift", "Vitara", "Jimny", "Baleno", "S-Cross", "Ertiga", "Ignis"
        },
        ["Jeep"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Compass", "Renegade", "Grand Cherokee", "Wrangler", "Gladiator", "Commander"
        },
        ["Subaru"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Forester", "Crosstrek", "Outback", "WRX", "BRZ", "Evoltis"
        },
        ["BMW"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Serie 1", "Serie 2", "Serie 3", "Serie 4", "Serie 5", "X1", "X3", "X5"
        },
        ["Audi"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "A1", "A3", "A4", "A5", "Q2", "Q3", "Q5", "Q7"
        },
        ["Mercedes-Benz"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Clase A", "Clase C", "Clase E", "GLA", "GLB", "GLC", "GLE"
        },
    };

    public static bool EsMarcaConocida(string marca)
    {
        return !string.IsNullOrWhiteSpace(marca) && MarcasYModelos.ContainsKey(marca.Trim());
    }

    public static bool EsCombinacionValida(string marca, string modelo)
    {
        if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo))
        {
            return false;
        }

        var marcaTrim = marca.Trim();
        var modeloTrim = modelo.Trim();

        if (MarcasYModelos.TryGetValue(marcaTrim, out var modelos))
        {
            return modelos.Contains(modeloTrim);
        }

        // Si es una marca externa que no está en el catálogo oficial,
        // permitimos el registro siempre que cumpla con los límites de longitud.
        return modeloTrim.Length is >= 1 and <= 50;
    }

    public static IReadOnlyCollection<string> ObtenerMarcas()
    {
        return MarcasYModelos.Keys.OrderBy(m => m).ToList();
    }

    public static IReadOnlyCollection<string> ObtenerModelosPorMarca(string marca)
    {
        if (!string.IsNullOrWhiteSpace(marca) && MarcasYModelos.TryGetValue(marca.Trim(), out var modelos))
        {
            return modelos.OrderBy(m => m).ToList();
        }

        return Array.Empty<string>();
    }
}
