namespace Notes.Application.Auth;

public sealed record RegisterUserCommand(string Username, string Email, string Password);

public sealed record LoginUserCommand(string UsernameOrEmail, string Password);

public sealed record RefreshTokenCommand(string RefreshToken);

public sealed record AuthResponse(
    Guid Id,
    string Username,
    string Email,
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);
