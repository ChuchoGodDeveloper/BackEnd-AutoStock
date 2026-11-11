using System.Security.Claims;
using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Authorize]
public class OrdenesServicioController : ControllerBase
{
    private readonly IVehiculosService _vehiculosService;

    public OrdenesServicioController(IVehiculosService vehiculosService)
    {
        _vehiculosService = vehiculosService;
    }

    [HttpPost("api/vehiculos/{vehiculoId:guid}/ordenes-servicio")]
    public async Task<ActionResult<OrdenServicioDto>> CrearOrdenServicio(
        Guid vehiculoId,
        [FromBody] CrearOrdenServicioDto dto)
    {
        var (usuarioId, nombre, rol) = ObtenerDatosUsuario();
        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var orden = await _vehiculosService.CrearOrdenServicioAsync(
            vehiculoId,
            dto,
            usuarioId.Value,
            nombre,
            rol);

        return Ok(orden);
    }

    [HttpGet("api/vehiculos/{vehiculoId:guid}/orden-servicio-abierta")]
    public async Task<ActionResult<OrdenServicioDto?>> ObtenerOrdenAbierta(Guid vehiculoId)
    {
        var orden = await _vehiculosService.ObtenerOrdenServicioAbiertaAsync(vehiculoId);
        return Ok(orden);
    }

    [HttpPost("api/ordenes-servicio/{ordenId:guid}/notas")]
    public async Task<ActionResult<OrdenServicioDto>> AgregarNotaAvance(
        Guid ordenId,
        [FromBody] AgregarNotaAvanceDto dto)
    {
        var (usuarioId, nombre, rol) = ObtenerDatosUsuario();
        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var orden = await _vehiculosService.AgregarNotaAvanceAsync(
            ordenId,
            dto,
            usuarioId.Value,
            nombre,
            rol);

        return Ok(orden);
    }

    [HttpPost("api/ordenes-servicio/{ordenId:guid}/cerrar")]
    public async Task<ActionResult<OrdenServicioDto>> CerrarOrden(
        Guid ordenId,
        [FromBody] CerrarOrdenServicioDto dto)
    {
        var (usuarioId, nombre, rol) = ObtenerDatosUsuario();
        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var orden = await _vehiculosService.CerrarOrdenServicioAsync(
            ordenId,
            dto,
            usuarioId.Value,
            nombre,
            rol);

        return Ok(orden);
    }

    [HttpGet("api/vehiculos/{vehiculoId:guid}/mantenimiento")]
    public async Task<ActionResult<IReadOnlyList<OrdenServicioDto>>> ObtenerHistorialMantenimiento(Guid vehiculoId)
    {
        var (_, _, rol) = ObtenerDatosUsuario();
        var historial = await _vehiculosService.ObtenerHistorialMantenimientoAsync(vehiculoId, rol);
        return Ok(historial);
    }

    private (int? UsuarioId, string Nombre, string Rol) ObtenerDatosUsuario()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : null;

        var nombre = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Usuario";

        var rol = User.FindFirst("rol")?.Value
            ?? User.FindFirst(ClaimTypes.Role)?.Value
            ?? "Usuario";

        return (usuarioId, nombre, rol);
    }
}

