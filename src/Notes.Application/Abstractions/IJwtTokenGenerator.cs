using Notes.Domain.Entities;

namespace Notes.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
