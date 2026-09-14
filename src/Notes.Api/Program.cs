using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Api.Extensions;
using Notes.Api.Middleware;
using Notes.Application.Common.Models;
using Notes.Infrastructure;
using Notes.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configuration otomatis membaca appsettings + environment variables.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => new FieldError(
                    Field: e.Key,
                    Message: string.IsNullOrWhiteSpace(err.ErrorMessage) ? "Format data tidak valid." : err.ErrorMessage)))
                .ToList();

            var errorResponse = ErrorResponse.Create(
                statusCode: StatusCodes.Status400BadRequest,
                errorCode: "INVALID_INPUT",
                message: "Format input atau validasi data tidak valid.",
                errors: errors,
                traceId: context.HttpContext.TraceIdentifier);

            return new BadRequestObjectResult(errorResponse);
        };
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBasicAuthentication(builder.Configuration);

var app = builder.Build();

// Middleware custom diletakkan sebelum endpoint agar exception dari layer bawah
// bisa diubah menjadi HTTP response yang konsisten.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    var statusCode = httpContext.Response.StatusCode;

    if (httpContext.Response.HasStarted)
        return;

    httpContext.Response.ContentType = "application/json";

    string errorCode = statusCode switch
    {
        StatusCodes.Status404NotFound => "RESOURCE_NOT_FOUND",
        StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
        StatusCodes.Status403Forbidden => "FORBIDDEN",
        _ => "HTTP_ERROR"
    };

    string message = statusCode switch
    {
        StatusCodes.Status404NotFound => "Endpoint atau resource tidak ditemukan.",
        StatusCodes.Status401Unauthorized => "Akses ditolak. Silakan sertakan kredensial autentikasi.",
        StatusCodes.Status403Forbidden => "Anda tidak memiliki hak akses untuk resource ini.",
        _ => "Terjadi kesalahan HTTP."
    };

    var errorResponse = ErrorResponse.Create(
        statusCode: statusCode,
        errorCode: errorCode,
        message: message,
        traceId: httpContext.TraceIdentifier);

    await httpContext.Response.WriteAsJsonAsync(errorResponse);
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

// Eksekusi EF Core Migrations secara otomatis pada startup aplikasi.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

// Dibutuhkan integration test untuk mengambil entry point aplikasi.
public partial class Program;
