using Microsoft.EntityFrameworkCore;
using Notes.Api.Extensions;
using Notes.Api.Middleware;
using Notes.Infrastructure;
using Notes.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configuration otomatis membaca appsettings + environment variables.
// Contoh environment variable: ConnectionStrings__Default
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBasicAuthentication(builder.Configuration);

var app = builder.Build();

// Middleware custom diletakkan sebelum endpoint agar exception dari layer bawah
// bisa diubah menjadi HTTP response yang konsisten.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

// Untuk project belajar kita membuat schema otomatis jika database masih kosong.
// Production sebaiknya memakai EF Core migrations yang dijalankan secara terkontrol.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();

// Dibutuhkan integration test untuk mengambil entry point aplikasi.
public partial class Program;
