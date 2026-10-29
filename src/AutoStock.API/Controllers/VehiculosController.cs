using System.Security.Claims;
using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculosService _vehiculosService;

    public VehiculosController(IVehiculosService vehiculosService)
    {
        _vehiculosService = vehiculosService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<VehiculoDto>>> GetCatalogo(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? busqueda = null,
        [FromQuery] List<EstadoOperativo>? estados = null,
        [FromQuery] TipoVehiculo? tipo = null,
        [FromQuery] decimal? precioMin = null,
        [FromQuery] decimal? precioMax = null)
    {
        var resultado = await _vehiculosService.GetCatalogoAsync(
            page,
            pageSize,
            busqueda,
            estados,
            tipo,
            precioMin,
            precioMax);

        return Ok(resultado);
    }

    [HttpGet("conteos-rapidos")]
    public async Task<ActionResult<ConteosRapidosDto>> GetConteosRapidos()
    {
        var resultado = await _vehiculosService.GetConteosRapidosAsync();
        return Ok(resultado);
    }

    [HttpGet("zonas")]
    public async Task<ActionResult<IReadOnlyList<string>>> ObtenerZonas()
    {
        var zonas = await _vehiculosService.ObtenerZonasLoteAsync();
        return Ok(zonas);
    }

    [HttpGet("cajon-disponible")]
    public async Task<ActionResult<VehiculoOcupanteDto>> ValidarCajonDisponible(
        [FromQuery] string zona,
        [FromQuery] string cajon,
        [FromQuery] Guid? ignorarVehiculoId = null)
    {
        var resultado = await _vehiculosService.ValidarCajonDisponibleAsync(zona, cajon, ignorarVehiculoId);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehiculoDetalleDto>> GetVehiculoById(Guid id)
    {
        var resultado = await _vehiculosService.GetVehiculoByIdAsync(id);
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<VehiculoDetalleDto>> RegistrarVehiculo(
        [FromBody] RegistrarVehiculoDto dto)
    {
        var registradoPor = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Encargado de Lote";

        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var creado = await _vehiculosService.RegistrarVehiculoAsync(dto, registradoPor, usuarioId);
        return CreatedAtAction(nameof(GetVehiculoById), new { id = creado.Id }, creado);
    }

    [HttpPost("{id:guid}/fotografias")]
    [Authorize]
    public async Task<ActionResult<VehiculoDetalleDto>> GuardarFotografia(
        Guid id,
        [FromBody] GuardarFotografiaDto dto)
    {
        var capturadaPor = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Encargado de Lote";

        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var actualizado = await _vehiculosService.GuardarFotografiaAsync(id, dto, capturadaPor, usuarioId);
        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}/fotografias/{imagenId:guid}")]
    [Authorize]
    public async Task<ActionResult<VehiculoDetalleDto>> EliminarFotografia(
        Guid id,
        Guid imagenId)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var actualizado = await _vehiculosService.EliminarFotografiaAsync(id, imagenId, usuarioId);
        return Ok(actualizado);
    }

    [HttpPatch("{id:guid}/estado")]
    [HttpPut("{id:guid}/estado")]
    [Authorize]
    public async Task<ActionResult<VehiculoDetalleDto>> CambiarEstadoOperativo(
        Guid id,
        [FromBody] CambiarEstadoOperativoDto dto)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var nombreUsuario = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Usuario";

        var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value
            ?? User.FindFirst("role")?.Value
            ?? string.Empty;

        var actualizado = await _vehiculosService.CambiarEstadoOperativoAsync(
            id,
            dto,
            usuarioId,
            nombreUsuario,
            rolUsuario);

        return Ok(actualizado);
    }

    [HttpPost("{id:guid}/reubicar")]
    [HttpPut("{id:guid}/reubicar")]
    [Authorize]
    public async Task<ActionResult<VehiculoDetalleDto>> ReubicarVehiculo(
        Guid id,
        [FromBody] ReubicarVehiculoDto dto)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var nombreUsuario = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Usuario";

        var actualizado = await _vehiculosService.ReubicarVehiculoAsync(
            id,
            dto,
            usuarioId,
            nombreUsuario);

        return Ok(actualizado);
    }
}
