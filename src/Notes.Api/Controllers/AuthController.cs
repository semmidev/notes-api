using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notes.Application.Auth;
using Notes.Application.Common.Models;

namespace Notes.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(command, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Registrasi pengguna berhasil."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(command, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login berhasil."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var response = await authService.RefreshTokenAsync(command, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Refresh token berhasil diperbarui."));
    }
}
