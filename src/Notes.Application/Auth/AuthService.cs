using Notes.Application.Abstractions;
using Notes.Domain.Entities;

namespace Notes.Application.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<AuthResponse> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var existingUsername = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
        if (existingUsername is not null)
            throw new ArgumentException("Username sudah digunakan.", nameof(command.Username));

        var existingEmail = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingEmail is not null)
            throw new ArgumentException("Email sudah terdaftar.", nameof(command.Email));

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            throw new ArgumentException("Password minimal 6 karakter.", nameof(command.Password));

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var user = User.Create(command.Username, command.Email, passwordHash);

        var (accessToken, expiresIn) = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(7);

        user.UpdateRefreshToken(refreshToken, refreshExpiry);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Username, user.Email, accessToken, refreshToken, expiresIn);
    }

    public async Task<AuthResponse> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAsync(command.UsernameOrEmail, cancellationToken)
            ?? await userRepository.GetByEmailAsync(command.UsernameOrEmail, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Username/Email atau password salah.");

        var (accessToken, expiresIn) = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(7);

        user.UpdateRefreshToken(refreshToken, refreshExpiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Username, user.Email, accessToken, refreshToken, expiresIn);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (user is null || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
            throw new UnauthorizedAccessException("Refresh token tidak valid atau telah kedaluwarsa.");

        var (accessToken, expiresIn) = jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = jwtTokenGenerator.GenerateRefreshToken();
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(7);

        user.UpdateRefreshToken(newRefreshToken, refreshExpiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Username, user.Email, accessToken, newRefreshToken, expiresIn);
    }
}
