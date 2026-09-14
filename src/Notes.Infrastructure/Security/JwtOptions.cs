namespace Notes.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "NotesApi";
    public string Audience { get; init; } = "NotesApiUser";
    public string SecretKey { get; init; } = "SuperSecretKeyNotesApiVeryLongSecretKey12345!";
    public int ExpiryMinutes { get; init; } = 60;
}
