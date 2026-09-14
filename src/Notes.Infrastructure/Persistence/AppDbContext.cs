using Microsoft.EntityFrameworkCore;
using Notes.Domain.Entities;

namespace Notes.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var note = modelBuilder.Entity<Note>();

        note.ToTable("notes");
        note.HasKey(x => x.Id);

        note.Property(x => x.Id)
            .HasColumnName("id");

        note.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        note.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired();

        note.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        note.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        note.HasIndex(x => x.CreatedAt);
    }
}
