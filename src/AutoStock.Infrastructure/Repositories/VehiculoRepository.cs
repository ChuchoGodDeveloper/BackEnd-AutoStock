using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly AutoStockDbContext _context;

    public VehiculoRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Vehiculo>> ObtenerTodosAsync()
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .Include(v => v.Imagenes)
            .ToListAsync();
    }

    public async Task<Vehiculo?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .Include(v => v.Imagenes)
            .Include(v => v.HistorialEstados)
            .Include(v => v.HistorialUbicaciones)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vehiculo?> ObtenerPorVinAsync(string vin)
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VIN == vin);
    }

    public async Task AgregarAsync(Vehiculo vehiculo)
    {
        await _context.Vehiculos.AddAsync(vehiculo);
    }

    public async Task GuardarImagenAsync(
        Vehiculo vehiculo,
        ImagenVehiculo nuevaImagen,
        ImagenVehiculo? imagenReemplazada = null)
    {
        if (imagenReemplazada is not null)
        {
            var existente = await _context.ImagenesVehiculo
                .FirstOrDefaultAsync(i => i.Id == imagenReemplazada.Id);
            if (existente is not null)
            {
                _context.ImagenesVehiculo.Remove(existente);
            }
        }

        await _context.ImagenesVehiculo.AddAsync(nuevaImagen);

        var vehiculoDb = await _context.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculo.Id);
        if (vehiculoDb is not null)
        {
            vehiculoDb.FotoPrincipalUrl = vehiculo.FotoPrincipalUrl;
        }
    }

    public async Task EliminarImagenAsync(Vehiculo vehiculo, ImagenVehiculo imagen)
    {
        var existente = await _context.ImagenesVehiculo
            .FirstOrDefaultAsync(i => i.Id == imagen.Id);
        if (existente is not null)
        {
            _context.ImagenesVehiculo.Remove(existente);
        }

        var vehiculoDb = await _context.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculo.Id);
        if (vehiculoDb is not null)
        {
            vehiculoDb.FotoPrincipalUrl = vehiculo.FotoPrincipalUrl;
        }
    }

    public async Task RegistrarCambioEstadoAsync(
        Vehiculo vehiculo,
        HistorialEstadoVehiculo registroHistorial)
    {
        await _context.HistorialEstadosVehiculo.AddAsync(registroHistorial);

        var vehiculoDb = await _context.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculo.Id);

        if (vehiculoDb is not null)
        {
            vehiculoDb.Estado = vehiculo.Estado;
            vehiculoDb.FechaCambioEstado = vehiculo.FechaCambioEstado;
            vehiculoDb.EstadoActualizadoPor = vehiculo.EstadoActualizadoPor;
            vehiculoDb.EstadoActualizadoPorUsuarioId = vehiculo.EstadoActualizadoPorUsuarioId;
            vehiculoDb.ResponsableTestDrive = vehiculo.ResponsableTestDrive;
            vehiculoDb.ReservadoPorUsuarioId = vehiculo.ReservadoPorUsuarioId;
            vehiculoDb.ReservadoPorNombre = vehiculo.ReservadoPorNombre;
        }
    }

    public async Task<Vehiculo?> ObtenerOcupanteCajonAsync(
        string zona,
        string cajon,
        Guid? ignorarVehiculoId = null)
    {
        var zonaNorm = zona.Trim().ToUpper();
        var cajonNorm = cajon.Trim().ToUpper();

        return await _context.Vehiculos
            .AsNoTracking()
            .Where(v => v.Estado != EstadoOperativo.Vendido)
            .Where(v => ignorarVehiculoId == null || v.Id != ignorarVehiculoId.Value)
            .FirstOrDefaultAsync(v => v.ZonaLote.ToUpper() == zonaNorm && v.CajonLote.ToUpper() == cajonNorm);
    }

    public async Task RegistrarReubicacionAsync(
        Vehiculo vehiculo,
        HistorialUbicacionVehiculo registroHistorial)
    {
        await _context.HistorialUbicacionesVehiculo.AddAsync(registroHistorial);

        var vehiculoDb = await _context.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculo.Id);

        if (vehiculoDb is not null)
        {
            vehiculoDb.ZonaLote = vehiculo.ZonaLote;
            vehiculoDb.CajonLote = vehiculo.CajonLote;
            vehiculoDb.FechaReubicacion = vehiculo.FechaReubicacion;
            vehiculoDb.ReubicadoPor = vehiculo.ReubicadoPor;
            vehiculoDb.ReubicadoPorUsuarioId = vehiculo.ReubicadoPorUsuarioId;
        }
    }
}
