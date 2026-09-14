using Microsoft.EntityFrameworkCore;
using Notes.Domain.Entities;

namespace Notes.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<User> Users => Set<User>();

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

        note.Property(x => x.Version)
            .HasColumnName("xmin")
            .HasColumnType("xmin")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        note.HasIndex(x => x.CreatedAt);

        var user = modelBuilder.Entity<User>();

        user.ToTable("users");
        user.HasKey(x => x.Id);

        user.Property(x => x.Id).HasColumnName("id");
        user.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
        user.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
        user.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        user.Property(x => x.RefreshToken).HasColumnName("refresh_token");
        user.Property(x => x.RefreshTokenExpiryTime).HasColumnName("refresh_token_expiry_time");
        user.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        user.HasIndex(x => x.Username).IsUnique();
        user.HasIndex(x => x.Email).IsUnique();
    }
}
