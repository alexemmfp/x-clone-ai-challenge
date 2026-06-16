using System.Text.Json;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Api.Middleware;

internal sealed partial class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandled(ILogger logger, Exception ex);

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            LogUnhandled(logger, ex);
            await WriteErrorAsync(ctx, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            DomainException => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });
        return ctx.Response.WriteAsync(body);
    }
}
