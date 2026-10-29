using System.Security.Claims;
using AutoStock.Application.DTOs;
using AutoStock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst("jti")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(jti))
        {
            return Unauthorized(new { mensaje = "Token inválido" });
        }

        await _authService.LogoutAsync(jti);
        return Ok(new { mensaje = "Sesión cerrada correctamente" });
    }

    [HttpGet("perfil")]
    [Authorize]
    public async Task<ActionResult<PerfilResponseDto>> GetPerfil()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(sub, out var usuarioId))
        {
            return Unauthorized(new { mensaje = "Token inválido" });
        }

        var perfil = await _authService.GetPerfilActualAsync(usuarioId);
        return Ok(perfil);
    }

    [HttpPost("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPasswordPrimerAcceso([FromBody] CambiarPasswordDto dto)
    {
        var sub = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(sub, out var usuarioId))
        {
            return Unauthorized(new { mensaje = "Token inválido" });
        }

        await _authService.CambiarPasswordPrimerAccesoAsync(dto, usuarioId);
        return Ok(new { mensaje = "Contraseña actualizada correctamente" });
    }

    [HttpPost("recuperar-password")]
    [AllowAnonymous]
    public async Task<ActionResult<SolicitarRecuperacionResponseDto>> SolicitarRecuperacionPassword(
        [FromBody] SolicitarRecuperacionDto dto)
    {
        var origin = Request.Headers.Origin.FirstOrDefault();
        var response = await _authService.SolicitarRecuperacionPasswordAsync(dto, origin);
        return Ok(response);
    }

    [HttpGet("validar-token-recuperacion")]
    [AllowAnonymous]
    public async Task<ActionResult<ValidarTokenRecuperacionResponseDto>> ValidarTokenRecuperacion(
        [FromQuery] string token)
    {
        var response = await _authService.ValidarTokenRecuperacionAsync(token);
        return Ok(response);
    }

    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordDto dto)
    {
        await _authService.RestablecerPasswordAsync(dto);
        return Ok(new { mensaje = "Contraseña actualizada" });
    }
}
