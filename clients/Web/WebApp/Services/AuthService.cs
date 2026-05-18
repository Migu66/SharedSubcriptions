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

        // Almacenar token JWT en claim para usarlo en peticiones posteriores
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("access_token", result.AccessToken),
            new("refresh_token", result.RefreshToken),
            new("token_expires", result.ExpiresAt.ToString("O"))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var context = _httpContextAccessor.HttpContext!;
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = result.ExpiresAt
        });

        return true;
    }

    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext!;
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private sealed record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
