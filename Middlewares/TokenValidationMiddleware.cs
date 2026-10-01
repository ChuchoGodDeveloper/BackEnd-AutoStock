using System.IdentityModel.Tokens.Jwt;
using AutoStock.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Middlewares;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AutoStockDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                var sesion = await dbContext.Sesiones
                    .FirstOrDefaultAsync(s => s.Token == token && s.Activa && s.FechaExpiracion > DateTime.UtcNow);

                if (sesion == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Token inválido o revocado" });
                    return;
                }
            }
        }

        await _next(context);
    }
}
