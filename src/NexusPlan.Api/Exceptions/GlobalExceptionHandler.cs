using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NexusPlan.Domain.Exceptions;

namespace NexusPlan.Api.Exceptions;

/// <summary>Traduce las excepciones de dominio a respuestas ProblemDetails (RFC 7807).</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            BusinessRuleException => (StatusCodes.Status409Conflict, "Regla de negocio violada"),
            ValidationException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status >= 500)
            logger.LogError(exception, "Error no controlado procesando {Path}", httpContext.Request.Path);
        else
            logger.LogWarning("{Title}: {Message}", title, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= 500 && !env.IsDevelopment()
                ? "Ocurrió un error inesperado. Intente nuevamente más tarde."
                : exception.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
