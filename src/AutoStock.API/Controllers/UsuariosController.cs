using System.Security.Claims;
using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;

    public UsuariosController(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListaDto>>> GetUsuarios()
    {
        var usuarios = await _usuariosService.ObtenerUsuariosAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDetalleAdminDto>> GetUsuarioPorId(int id)
    {
        var usuario = await _usuariosService.ObtenerUsuarioPorIdAsync(id);
        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioCreadoDto>> CrearUsuario([FromBody] CrearUsuarioDto dto)
    {
        var usuarioCreado = await _usuariosService.CrearUsuarioAsync(dto);
        return CreatedAtAction(nameof(GetUsuarioPorId), new { id = usuarioCreado.Id }, usuarioCreado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioListaDto>> EditarUsuario(int id, [FromBody] EditarUsuarioDto dto)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int adminId = int.TryParse(sub, out var parsedId) ? parsedId : 0;

        var usuarioActualizado = await _usuariosService.EditarUsuarioAsync(id, dto, adminId);
        return Ok(usuarioActualizado);
    }

    [HttpPatch("{id:int}/estatus")]
    [HttpPut("{id:int}/estatus")]
    public async Task<ActionResult<UsuarioListaDto>> CambiarEstatusUsuario(int id, [FromBody] CambiarEstatusUsuarioDto dto)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int adminId = int.TryParse(sub, out var parsedId) ? parsedId : 0;

        var usuarioActualizado = await _usuariosService.CambiarEstatusUsuarioAsync(id, dto, adminId);
        return Ok(usuarioActualizado);
    }

    [HttpPost("{id:int}/restablecer-password")]
    public async Task<ActionResult<PasswordTemporalRestablecidaDto>> RestablecerPasswordPorAdmin(int id)
    {
        var resultado = await _usuariosService.RestablecerPasswordPorAdminAsync(id);
        return Ok(resultado);
    }
}
