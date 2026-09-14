using Notes.Application.Common.Models;

namespace Notes.Api.Middleware;

/// <summary>
/// Menyatukan error application/domain menjadi response HTTP yang konsisten.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validasi domain gagal.");
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception pada request {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "Terjadi kesalahan internal pada server.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ErrorResponse.Create(
            statusCode: statusCode,
            errorCode: errorCode,
            message: message,
            traceId: context.TraceIdentifier);

        await context.Response.WriteAsJsonAsync(response);
    }
}
