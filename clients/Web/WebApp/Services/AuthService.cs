using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApp.Services;

public sealed class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (result is null)
            return false;

        await SignInWithTokenAsync(email, result);
        return true;
    }

    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext!;
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Devuelve el access token vigente, refrescándolo automáticamente si está a punto de expirar.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return null;

        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) return null;

        var expiresStr = user.FindFirstValue("token_expires");
        if (expiresStr is not null
            && DateTime.TryParse(expiresStr, out var expiresAt)
            && expiresAt <= DateTime.UtcNow.AddMinutes(5))
        {
            // El token expira en menos de 5 minutos: refrescamos
            var refreshed = await RefreshTokenAsync();
            if (!refreshed) return null;

            // Releer el claim actualizado
            return _httpContextAccessor.HttpContext?.User.FindFirstValue("access_token");
        }

        return user.FindFirstValue("access_token");
    }

    /// <summary>
    /// Refresca el par de tokens usando el refresh token almacenado en la cookie.
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return false;

        var refreshToken = context.User.FindFirstValue("refresh_token");
        if (string.IsNullOrEmpty(refreshToken)) return false;

        var client = _httpClientFactory.CreateClient("ApiGateway");
        var response = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (result is null) return false;

        var email = context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        await SignInWithTokenAsync(email, result);
        return true;
    }

    private async Task SignInWithTokenAsync(string email, AuthTokenResponse token)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("access_token", token.AccessToken),
            new("refresh_token", token.RefreshToken),
            new("token_expires", token.ExpiresAt.ToString("O"))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var context = _httpContextAccessor.HttpContext!;
        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = token.ExpiresAt
            });
    }

    private sealed record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
