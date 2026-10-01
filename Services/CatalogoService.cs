using AutoStock.Data;
using AutoStock.DTOs;
using AutoStock.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Services;

public class CatalogoService : ICatalogoService
{
    private readonly AutoStockDbContext _context;
    private const int TamanioPagina = 20;

    public CatalogoService(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<PaginacionDto<VehiculoCatalogoDto>> ObtenerCatalogoAsync(int pagina)
    {
        if (pagina < 1) pagina = 1;

        var query = _context.Vehiculos
            .AsNoTracking()
            .Include(v => v.EstadoOperativo)
            .Include(v => v.UbicacionLote)
            .Include(v => v.Imagenes)
            .Where(v => v.EstadoOperativo.Nombre != "Vendido")
            .OrderByDescending(v => v.FechaRegistro);

        var totalUnidades = await query.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalUnidades / (double)TamanioPagina);

        var items = await query
            .Skip((pagina - 1) * TamanioPagina)
            .Take(TamanioPagina)
            .Select(v => new VehiculoCatalogoDto
            {
                Id = v.Id,
                FotoPrincipalUrl = v.Imagenes
                    .Where(i => i.EsPrincipal)
                    .Select(i => i.Url)
                    .FirstOrDefault() ?? v.Imagenes
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                Marca = v.Marca,
                Modelo = v.Modelo,
                Anio = v.Anio,
                PrecioMXN = v.Precio,
                Kilometraje = v.Kilometraje,
                Placa = v.Placa,
                Zona = v.UbicacionLote.Zona,
                Cajon = v.UbicacionLote.Cajon,
                EstadoOperativo = v.EstadoOperativo.Nombre,
                ColorEstado = v.EstadoOperativo.ColorHex
            })
            .ToListAsync();

        return new PaginacionDto<VehiculoCatalogoDto>
        {
            TotalUnidades = totalUnidades,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            Items = items
        };
    }
}
