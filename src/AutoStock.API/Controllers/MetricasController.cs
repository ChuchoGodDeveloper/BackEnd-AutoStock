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
}
