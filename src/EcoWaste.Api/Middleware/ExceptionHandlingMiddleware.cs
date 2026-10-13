using System.Net;
using System.Text.Json;
using EcoWaste.Api.Exceptions;

namespace EcoWaste.Api.Middleware;

/// <summary>
/// Middleware centralizado de tratamento de exceções. Converte exceções de domínio
/// em respostas HTTP padronizadas (RFC 7807 - ProblemDetails).
/// </summary>
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
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            BusinessRuleException => (HttpStatusCode.UnprocessableEntity, "Regra de negócio violada"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Não autorizado"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Erro não tratado");
        else
            _logger.LogWarning("{Title}: {Message}", title, ex.Message);

        var problem = new
        {
            type = $"https://httpstatuses.io/{(int)status}",
            title,
            status = (int)status,
            detail = ex.Message,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
