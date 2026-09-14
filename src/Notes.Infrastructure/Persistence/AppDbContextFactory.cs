using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notes.Infrastructure.Persistence;

/// <summary>
/// Design-Time DbContext Factory untuk EF Core CLI Tooling (dotnet ef migrations).
/// Memungkinkan ef CLI membuat migration file tanpa memerlukan koneksi runtime aktif.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=notesdb;Username=notes;Password=notes_dev_password");

        return new AppDbContext(optionsBuilder.Options);
    }
}
