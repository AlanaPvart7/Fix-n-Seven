using FluentValidation;
using Microsoft.AspNetCore.Mvc;
namespace FixnSeven.API.Middleware;

// Atrapa los errores de TODA la API en un solo lugar y responde con ProblemDetails
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (ValidationException ex)   // FluentValidation: datos invalidos -> 400
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
            {
                Title = "Datos inválidos",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)              // cualquier otro error -> 500 sin detalles internos
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Error interno del servidor",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
