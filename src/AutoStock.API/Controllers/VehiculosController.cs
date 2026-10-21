using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculosService _vehiculosService;

    public VehiculosController(IVehiculosService vehiculosService)
    {
        _vehiculosService = vehiculosService;
    }

    [HttpGet("catalogo")]
    public async Task<ActionResult<PaginatedResult<VehiculoDto>>> GetCatalogo(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? busqueda = null,
        [FromQuery(Name = "estados[]")] List<EstadoOperativo>? estados = null,
        [FromQuery] TipoVehiculo? tipo = null,
        [FromQuery] decimal? precioMin = null,
        [FromQuery] decimal? precioMax = null)
    {
        var resultado = await _vehiculosService.GetCatalogoAsync(
            page, pageSize, busqueda, estados, tipo, precioMin, precioMax);
        return Ok(resultado);
    }

    [HttpGet("conteos-rapidos")]
    public async Task<ActionResult<ConteosRapidosDto>> GetConteosRapidos()
    {
        var resultado = await _vehiculosService.GetConteosRapidosAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehiculoDetalleDto>> GetVehiculoById(Guid id)
    {
        var resultado = await _vehiculosService.GetVehiculoByIdAsync(id);
        return Ok(resultado);
    }
}
