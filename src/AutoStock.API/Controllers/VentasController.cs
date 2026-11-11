using System.Security.Claims;
using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVehiculosService _vehiculosService;

    public VentasController(IVehiculosService vehiculosService)
    {
        _vehiculosService = vehiculosService;
    }

    [HttpGet("reporte")]
    public async Task<ActionResult<ReporteVentasDto>> ObtenerReporte(
        [FromQuery] string? periodo,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var nombreUsuario = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Usuario";

        var reporte = await _vehiculosService.GenerarReporteVentasAsync(
            periodo,
            fechaInicio,
            fechaFin,
            usuarioId,
            nombreUsuario);

        return Ok(reporte);
    }
}

