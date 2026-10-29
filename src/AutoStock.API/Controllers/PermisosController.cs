using System.Security.Claims;
using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class PermisosController : ControllerBase
{
    private readonly IPermisosService _permisosService;

    public PermisosController(IPermisosService permisosService)
    {
        _permisosService = permisosService;
    }

    [HttpGet("matriz")]
    public async Task<ActionResult<MatrizPermisosResponseDto>> GetMatriz()
    {
        var matriz = await _permisosService.GetMatrizPermisosAsync();
        return Ok(matriz);
    }

    [HttpPut("matriz")]
    public async Task<ActionResult<RespuestaOperacionDto>> ActualizarMatriz(
        [FromBody] ActualizarMatrizPermisosRequestDto dto)
    {
        var (usuarioId, nombreUsuario) = ObtenerDatosUsuarioAutenticado();
        var resultado = await _permisosService.ActualizarMatrizPermisosAsync(dto, usuarioId, nombreUsuario);
        return Ok(resultado);
    }

    [HttpPost("restablecer")]
    public async Task<ActionResult<RespuestaOperacionDto>> RestablecerMatriz()
    {
        var (usuarioId, nombreUsuario) = ObtenerDatosUsuarioAutenticado();
        var resultado = await _permisosService.RestablecerMatrizBaseAsync(usuarioId, nombreUsuario);
        return Ok(resultado);
    }

    private (int UsuarioId, string NombreUsuario) ObtenerDatosUsuarioAutenticado()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var usuarioId = int.TryParse(sub, out var parsedId) ? parsedId : 1;

        var nombreUsuario = User.FindFirst("nombre")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "Administrador";

        return (usuarioId, nombreUsuario);
    }
}
