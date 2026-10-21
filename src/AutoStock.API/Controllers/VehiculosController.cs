using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
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
        [FromQuery] int pageSize = 20)
    {
        var resultado = await _vehiculosService.GetCatalogoAsync(page, pageSize);
        return Ok(resultado);
    }
}
