using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Notes.Api.Authentication;

/// <summary>
/// Basic Authentication sederhana untuk tujuan belajar.
///
/// Header yang diterima:
/// Authorization: Basic base64(username:password)
///
/// Catatan: Basic Auth hanya aman jika request melewati HTTPS/TLS.
/// Jangan gunakan password sederhana seperti contoh ini untuk production.
/// </summary>
public sealed class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    IOptions<BasicAuthOptions> basicAuthOptions,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out var headerValue))
            return Task.FromResult(AuthenticateResult.Fail("Authorization header tidak valid."));

        if (!string.Equals(headerValue.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (string.IsNullOrWhiteSpace(headerValue.Parameter))
            return Task.FromResult(AuthenticateResult.Fail("Basic credentials tidak ditemukan."));

        string username;
        string password;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue.Parameter));
            var separatorIndex = decoded.IndexOf(':');

            if (separatorIndex <= 0)
                return Task.FromResult(AuthenticateResult.Fail("Format Basic credentials tidak valid."));

            username = decoded[..separatorIndex];
            password = decoded[(separatorIndex + 1)..];
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Basic credentials bukan Base64 yang valid."));
        }

        var configured = basicAuthOptions.Value;

        // Untuk demo kita membandingkan credential dari environment/configuration.
        // Di production, gunakan identity provider atau password hash yang proper.
        var valid =
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(username),
                Encoding.UTF8.GetBytes(configured.Username)) &&
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(configured.Password));

        if (!valid)
            return Task.FromResult(AuthenticateResult.Fail("Username atau password salah."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Basic realm=NotesApi";
        return Task.CompletedTask;
    }
}
