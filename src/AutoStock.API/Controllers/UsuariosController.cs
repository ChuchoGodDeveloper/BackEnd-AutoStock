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

    [HttpPost]
    public async Task<ActionResult<UsuarioCreadoDto>> CrearUsuario([FromBody] CrearUsuarioDto dto)
    {
        var usuarioCreado = await _usuariosService.CrearUsuarioAsync(dto);
        return CreatedAtAction(nameof(GetUsuarios), new { id = usuarioCreado.Id }, usuarioCreado);
    }
}
