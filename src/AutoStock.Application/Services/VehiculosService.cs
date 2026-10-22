using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
using AutoStock.Domain.Interfaces;

namespace AutoStock.Application.Services;

public class VehiculosService : IVehiculosService
{
    private const int TamanioPaginaPorDefecto = 20;
    private const int TamanioPaginaMaximo = 100;
    private const int LongitudVin = 17;
    private const int AnioMinimo = 1990;

    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IUnitOfWork? _unitOfWork;

    public VehiculosService(
        IVehiculoRepository vehiculoRepository,
        IUnitOfWork? unitOfWork = null)
    {
        _vehiculoRepository = vehiculoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<VehiculoDto>> GetCatalogoAsync(
        int page = 1,
        int pageSize = 20,
        string? busqueda = null,
        List<EstadoOperativo>? estados = null,
        TipoVehiculo? tipo = null,
        decimal? precioMin = null,
        decimal? precioMax = null)
    {
        var vehiculos = await _vehiculoRepository.ObtenerTodosAsync();

        IEnumerable<Vehiculo> catalogo = vehiculos;

        if (estados is { Count: > 0 })
        {
            catalogo = catalogo.Where(v => estados.Contains(v.Estado));
        }
        else
        {
            catalogo = catalogo.Where(v => v.Estado != EstadoOperativo.Vendido);
        }

        if (tipo.HasValue)
        {
            catalogo = catalogo.Where(v => v.Tipo == tipo.Value);
        }

        if (precioMin.HasValue)
        {
            catalogo = catalogo.Where(v => v.Precio >= precioMin.Value);
        }

        if (precioMax.HasValue)
        {
            catalogo = catalogo.Where(v => v.Precio <= precioMax.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = NormalizarTexto(busqueda);

            catalogo = catalogo.Where(v =>
                Contiene(v.Marca, termino) ||
                Contiene(v.Modelo, termino) ||
                Contiene(v.Anio.ToString(), termino) ||
                Contiene(v.VIN, termino) ||
                Contiene(v.Placa, termino));
        }

        var catalogoFiltrado = catalogo
            .OrderByDescending(v => v.FechaRegistro)
            .ToList();

        var pagina = page < 1 ? 1 : page;
        var tamanio = pageSize < 1
            ? TamanioPaginaPorDefecto
            : Math.Min(pageSize, TamanioPaginaMaximo);

        var items = catalogoFiltrado
            .Skip((pagina - 1) * tamanio)
            .Take(tamanio)
            .Select(CrearDto)
            .ToList();

        return new PaginatedResult<VehiculoDto>
        {
            Items = items,
            TotalCount = catalogoFiltrado.Count,
            Page = pagina,
            PageSize = tamanio,
        };
    }

    public async Task<ConteosRapidosDto> GetConteosRapidosAsync()
    {
        var vehiculos = await _vehiculoRepository.ObtenerTodosAsync();

        var activos = vehiculos
            .Where(v => v.Estado != EstadoOperativo.Vendido)
            .ToList();

        return new ConteosRapidosDto
        {
            Todos = activos.Count,
            Disponibles = activos.Count(v => v.Estado == EstadoOperativo.Disponible),
            Seminuevos = activos.Count(v => v.Tipo == TipoVehiculo.Seminuevo),
            Nuevos = activos.Count(v => v.Tipo == TipoVehiculo.Nuevo),
        };
    }

    public async Task<VehiculoDetalleDto> GetVehiculoByIdAsync(Guid id)
    {
        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(id);

        if (vehiculo is null)
        {
            throw new NotFoundException("Vehículo no encontrado");
        }

        return CrearDetalleDto(vehiculo);
    }

    public async Task<VehiculoDetalleDto> RegistrarVehiculoAsync(
        RegistrarVehiculoDto dto,
        string registradoPor)
    {
        if (string.IsNullOrWhiteSpace(dto.VIN) ||
            string.IsNullOrWhiteSpace(dto.Marca) ||
            string.IsNullOrWhiteSpace(dto.Modelo) ||
            string.IsNullOrWhiteSpace(dto.ZonaLote) ||
            string.IsNullOrWhiteSpace(dto.CajonLote) ||
            string.IsNullOrWhiteSpace(dto.Transmision) ||
            string.IsNullOrWhiteSpace(dto.Motor) ||
            string.IsNullOrWhiteSpace(dto.ColorExterior) ||
            string.IsNullOrWhiteSpace(dto.ColorInterior))
        {
            throw new ValidationAppException("Todos los campos obligatorios deben ser completados");
        }

        var vin = NormalizarTexto(dto.VIN);

        if (!EsVinValido(vin))
        {
            throw new ValidationAppException(
                "El VIN debe tener 17 caracteres alfanuméricos y no puede contener las letras I, O ni Q");
        }

        var anioMaximo = DateTime.UtcNow.Year + 1;
        if (dto.Anio < AnioMinimo || dto.Anio > anioMaximo)
        {
            throw new ValidationAppException($"El año debe estar entre {AnioMinimo} y {anioMaximo}");
        }

        if (dto.Precio <= 0)
        {
            throw new ValidationAppException("El precio debe ser mayor a cero");
        }

        if (!Enum.IsDefined(dto.Estado) || dto.Estado == EstadoOperativo.Vendido)
        {
            throw new ValidationAppException("El estado inicial no puede ser Vendido");
        }

        if (!Enum.IsDefined(dto.Tipo))
        {
            throw new ValidationAppException("El tipo de vehículo no es válido");
        }

        string? placaNormalizada = null;
        string? placaGuardar = null;
        int kilometrajeGuardar;

        if (dto.Tipo == TipoVehiculo.Nuevo)
        {
            kilometrajeGuardar = 0;
            placaGuardar = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Placa))
            {
                throw new ValidationAppException("La placa es obligatoria para vehículos seminuevos");
            }

            if (dto.Kilometraje < 0)
            {
                throw new ValidationAppException("El kilometraje no puede ser negativo");
            }

            kilometrajeGuardar = dto.Kilometraje;
            placaGuardar = dto.Placa.Trim().ToUpperInvariant();
            placaNormalizada = NormalizarTexto(placaGuardar);
        }

        var vehiculosExistentes = await _vehiculoRepository.ObtenerTodosAsync();

        var duplicadoPorVin = await _vehiculoRepository.ObtenerPorVinAsync(vin)
            ?? vehiculosExistentes.FirstOrDefault(
                v => string.Equals(NormalizarTexto(v.VIN), vin, StringComparison.Ordinal));

        if (duplicadoPorVin is not null)
        {
            throw new ValidationAppException("Ya existe una unidad con ese VIN", duplicadoPorVin.Id);
        }

        if (placaNormalizada is not null)
        {
            var duplicadoPorPlaca = vehiculosExistentes.FirstOrDefault(
                v => !string.IsNullOrWhiteSpace(v.Placa) &&
                     string.Equals(NormalizarTexto(v.Placa), placaNormalizada, StringComparison.Ordinal));

            if (duplicadoPorPlaca is not null)
            {
                throw new ValidationAppException("Ya existe una unidad con esa placa", duplicadoPorPlaca.Id);
            }
        }

        var numeroStock = GenerarSiguienteNumeroStock(vehiculosExistentes);

        var nuevoVehiculo = new Vehiculo
        {
            Id = Guid.NewGuid(),
            VIN = vin,
            NumeroStock = numeroStock,
            Tipo = dto.Tipo,
            Marca = dto.Marca.Trim(),
            Modelo = dto.Modelo.Trim(),
            Anio = dto.Anio,
            Version = LimpiarOpcional(dto.Version),
            Placa = placaGuardar,
            Kilometraje = kilometrajeGuardar,
            Transmision = LimpiarOpcional(dto.Transmision),
            Motor = LimpiarOpcional(dto.Motor),
            Potencia = LimpiarOpcional(dto.Potencia),
            ColorExterior = LimpiarOpcional(dto.ColorExterior),
            ColorInterior = LimpiarOpcional(dto.ColorInterior),
            Equipamiento = LimpiarOpcional(dto.Equipamiento),
            NotasInspeccion = LimpiarOpcional(dto.NotasInspeccion),
            Precio = dto.Precio,
            Estado = dto.Estado,
            ZonaLote = dto.ZonaLote.Trim().ToUpperInvariant(),
            CajonLote = dto.CajonLote.Trim().ToUpperInvariant(),
            FechaRegistro = DateTime.UtcNow,
            RegistradoPor = string.IsNullOrWhiteSpace(registradoPor)
                ? "Encargado de Lote"
                : registradoPor.Trim(),
        };

        await _vehiculoRepository.AgregarAsync(nuevoVehiculo);

        if (_unitOfWork is not null)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return CrearDetalleDto(nuevoVehiculo);
    }

    private static bool EsVinValido(string vin)
    {
        if (vin.Length != LongitudVin)
        {
            return false;
        }

        foreach (var caracter in vin)
        {
            var esLetraValida = caracter is >= 'A' and <= 'Z' &&
                caracter != 'I' &&
                caracter != 'O' &&
                caracter != 'Q';
            var esDigito = caracter is >= '0' and <= '9';

            if (!esLetraValida && !esDigito)
            {
                return false;
            }
        }

        return true;
    }

    private static string GenerarSiguienteNumeroStock(IReadOnlyList<Vehiculo> vehiculos)
    {
        var maxConsecutivo = vehiculos.Count;

        foreach (var vehiculo in vehiculos)
        {
            if (string.IsNullOrWhiteSpace(vehiculo.NumeroStock))
            {
                continue;
            }

            var digitos = new string(vehiculo.NumeroStock.Where(char.IsDigit).ToArray());
            if (int.TryParse(digitos, out var numero) && numero > maxConsecutivo)
            {
                maxConsecutivo = numero;
            }
        }

        return $"STK-{(maxConsecutivo + 1):D4}";
    }

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static string NormalizarTexto(string valor)
    {
        return valor.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
    }

    private static bool Contiene(string? valor, string terminoNormalizado)
    {
        return !string.IsNullOrWhiteSpace(valor) &&
               NormalizarTexto(valor).Contains(terminoNormalizado, StringComparison.Ordinal);
    }

    private static VehiculoDto CrearDto(Vehiculo vehiculo) => new()
    {
        Id = vehiculo.Id,
        Marca = vehiculo.Marca,
        Modelo = vehiculo.Modelo,
        Anio = vehiculo.Anio,
        Precio = vehiculo.Precio,
        Kilometraje = vehiculo.Kilometraje,
        Placa = vehiculo.Placa,
        ZonaLote = vehiculo.ZonaLote,
        CajonLote = vehiculo.CajonLote,
        FotoPrincipalUrl = vehiculo.FotoPrincipalUrl,
        Estado = vehiculo.Estado.ToString(),
        Tipo = vehiculo.Tipo.ToString(),
        FechaRegistro = vehiculo.FechaRegistro,
    };

    private static VehiculoDetalleDto CrearDetalleDto(Vehiculo vehiculo) => new()
    {
        Id = vehiculo.Id,
        Marca = vehiculo.Marca,
        Modelo = vehiculo.Modelo,
        Anio = vehiculo.Anio,
        Version = vehiculo.Version,
        Precio = vehiculo.Precio,
        Kilometraje = vehiculo.Kilometraje,
        Placa = vehiculo.Placa,
        ZonaLote = vehiculo.ZonaLote,
        CajonLote = vehiculo.CajonLote,
        FotoPrincipalUrl = vehiculo.FotoPrincipalUrl,
        Estado = vehiculo.Estado.ToString(),
        Tipo = vehiculo.Tipo.ToString(),
        FechaRegistro = vehiculo.FechaRegistro,
        RegistradoPor = vehiculo.RegistradoPor,
        VIN = vehiculo.VIN,
        NumeroStock = vehiculo.NumeroStock,
        Motor = vehiculo.Motor,
        Potencia = vehiculo.Potencia,
        Transmision = vehiculo.Transmision,
        ColorExterior = vehiculo.ColorExterior,
        ColorInterior = vehiculo.ColorInterior,
        Equipamiento = vehiculo.Equipamiento,
        NotasInspeccion = vehiculo.NotasInspeccion,
        Imagenes = vehiculo.Imagenes
            .OrderBy(imagen => imagen.EsPrincipal ? 0 : 1)
            .ThenBy(imagen => imagen.Orden)
            .Select(imagen => new ImagenDto
            {
                Url = imagen.Url,
                EsPrincipal = imagen.EsPrincipal,
                Orden = imagen.Orden,
            })
            .ToList(),
    };
}
