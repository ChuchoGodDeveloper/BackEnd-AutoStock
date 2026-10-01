using AutoStock.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStock.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogoController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogoController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCatalogo([FromQuery] int pagina = 1)
    {
        var resultado = await _catalogoService.ObtenerCatalogoAsync(pagina);
        return Ok(resultado);
    }
}
