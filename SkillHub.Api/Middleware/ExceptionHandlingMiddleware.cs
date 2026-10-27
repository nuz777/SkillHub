using System.Net;
using System.Text.Json;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning(ex, "Regla de negocio no cumplida: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Error de dominio: {Message}", ex.Message);
            await HandleExceptionAsync(context, (int)HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en la API: {Message}", ex.Message);
            await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError, "Ocurrió un error inesperado en el servidor.");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
