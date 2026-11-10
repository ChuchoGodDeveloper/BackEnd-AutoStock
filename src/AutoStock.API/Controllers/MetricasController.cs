using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricasController : ControllerBase
{
    private readonly IVehiculosService _vehiculosService;

    public MetricasController(IVehiculosService vehiculosService)
    {
        _vehiculosService = vehiculosService;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenMetricasDto>> GetResumen()
    {
        var resumen = await _vehiculosService.GetResumenMetricasAsync();
        return Ok(resumen);
    }

    [HttpGet("lote-completo")]
    public async Task<ActionResult<MetricasCompletasLoteDto>> GetMetricasCompletas()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var metricas = await _vehiculosService.GetMetricasCompletasLoteAsync(usuarioId);
        return Ok(metricas);
    }
}
