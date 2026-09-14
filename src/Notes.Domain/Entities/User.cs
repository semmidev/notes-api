namespace Notes.Domain.Entities;

public sealed class User
{
    private User()
    {
    }

    private User(Guid id, string username, string email, string passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static User Create(string username, string email, string passwordHash)
    {
        Validate(username, email, passwordHash);

        return new User(
            Guid.NewGuid(),
            username.Trim().ToLowerInvariant(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            DateTimeOffset.UtcNow);
    }

    public void UpdateRefreshToken(string refreshToken, DateTimeOffset expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    private static void Validate(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username wajib diisi.", nameof(username));

        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("Username harus antara 3 hingga 50 karakter.", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email wajib diisi.", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Format email tidak valid.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash wajib diisi.", nameof(passwordHash));
    }
}
