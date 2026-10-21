using System.Text.Json;
using AutoStock.Application.Exceptions;

namespace AutoStock.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationAppException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (AuthenticationFailedException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (AccountLockedException ex)
        {
            await WriteErrorResponseAsync(context, 423, ex.Message);
        }
        catch (AccountDisabledException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = JsonSerializer.Serialize(new { mensaje = message });
        await context.Response.WriteAsync(payload);
    }
}
