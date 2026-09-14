namespace Notes.Domain.Entities;

/// <summary>
/// Aggregate root sederhana untuk fitur Notes.
/// Domain tidak mengetahui database, HTTP, ASP.NET, atau framework lainnya.
/// </summary>
public sealed class Note
{
    private Note()
    {
        // Constructor kosong ini diperlukan EF Core untuk materialisasi entity.
    }

    private Note(Guid id, string title, string content, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        Title = title;
        Content = content;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public uint Version { get; private set; }

    public static Note Create(string title, string content)
    {
        Validate(title, content);

        var now = DateTimeOffset.UtcNow;
        return new Note(Guid.NewGuid(), title.Trim(), content.Trim(), now, now);
    }

    public void Update(string title, string content)
    {
        Validate(title, content);

        Title = title.Trim();
        Content = content.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void Validate(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Judul note wajib diisi.", nameof(title));

        if (title.Trim().Length > 200)
            throw new ArgumentException("Judul note maksimal 200 karakter.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Isi note wajib diisi.", nameof(content));
    }
}
