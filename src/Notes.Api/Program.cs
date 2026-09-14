using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Api.Extensions;
using Notes.Api.Middleware;
using Notes.Application.Common.Models;
using Notes.Infrastructure;
using Notes.Infrastructure.Persistence;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

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

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc().AddApiExplorer();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
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

app.MapHealthChecks("/health");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
}

app.Run();

public partial class Program;
