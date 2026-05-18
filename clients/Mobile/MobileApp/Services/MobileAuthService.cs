using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MobileApp.Services;

/// <summary>
/// Gestiona el ciclo de vida del token JWT en el contexto móvil.
/// Los tokens se almacenan en SecureStorage para que nunca queden en texto plano.
/// </summary>
public sealed class MobileAuthService
{
    // Claves de SecureStorage
    private const string KeyAccessToken = "access_token";
    private const string KeyRefreshToken = "refresh_token";
    private const string KeyTokenExpires = "token_expires";
    private const string KeyUserEmail = "user_email";
    private const string KeyUserId = "user_id";

    private readonly IHttpClientFactory _httpClientFactory;

    public MobileAuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>Evento que se dispara cuando cambia el estado de autenticación.</summary>
    public event Action? AuthStateChanged;

    // ── Consulta de estado ───────────────────────────────────────────────────

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await SecureStorage.GetAsync(KeyAccessToken);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetUserEmailAsync()
        => await SecureStorage.GetAsync(KeyUserEmail);

    public async Task<Guid> GetUserIdAsync()
    {
        var idStr = await SecureStorage.GetAsync(KeyUserId);
        return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
    }

    // ── Login / Logout ───────────────────────────────────────────────────────

    public async Task<bool> LoginAsync(string email, string password)
    {
        // Usamos el cliente sin handler para evitar dependencia circular
        var client = _httpClientFactory.CreateClient("ApiGatewayAuth");
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (result is null) return false;

        await SaveTokensAsync(email, result);
        AuthStateChanged?.Invoke();
        return true;
    }

    public void Logout()
    {
        SecureStorage.Remove(KeyAccessToken);
        SecureStorage.Remove(KeyRefreshToken);
        SecureStorage.Remove(KeyTokenExpires);
        SecureStorage.Remove(KeyUserEmail);
        SecureStorage.Remove(KeyUserId);
        AuthStateChanged?.Invoke();
    }

    // ── Gestión del token ────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve el access token vigente, refrescándolo automáticamente
    /// si le quedan menos de 5 minutos de vida.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var token = await SecureStorage.GetAsync(KeyAccessToken);
        if (string.IsNullOrEmpty(token)) return null;

        var expiresStr = await SecureStorage.GetAsync(KeyTokenExpires);
        if (expiresStr is not null
            && DateTime.TryParse(expiresStr, out var expiresAt)
            && expiresAt <= DateTime.UtcNow.AddMinutes(5))
        {
            var refreshed = await RefreshTokenAsync();
            if (!refreshed)
            {
                // Token inválido: limpiar sesión
                Logout();
                return null;
            }
            return await SecureStorage.GetAsync(KeyAccessToken);
        }

        return token;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await SecureStorage.GetAsync(KeyRefreshToken);
        if (string.IsNullOrEmpty(refreshToken)) return false;

        // Cliente sin handler para evitar recursión infinita
        var client = _httpClientFactory.CreateClient("ApiGatewayAuth");
        var response = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (result is null) return false;

        var email = await SecureStorage.GetAsync(KeyUserEmail) ?? string.Empty;
        await SaveTokensAsync(email, result);
        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task SaveTokensAsync(string email, AuthTokenResponse token)
    {
        await SecureStorage.SetAsync(KeyAccessToken, token.AccessToken);
        await SecureStorage.SetAsync(KeyRefreshToken, token.RefreshToken);
        await SecureStorage.SetAsync(KeyTokenExpires, token.ExpiresAt.ToString("O"));
        await SecureStorage.SetAsync(KeyUserEmail, email);

        var userId = ExtractUserIdFromJwt(token.AccessToken);
        if (userId != Guid.Empty)
            await SecureStorage.SetAsync(KeyUserId, userId.ToString());
    }

    private static Guid ExtractUserIdFromJwt(string accessToken)
    {
        try
        {
            var parts = accessToken.Split('.');
            if (parts.Length < 2) return Guid.Empty;

            var payload = parts[1];
            var remainder = payload.Length % 4;
            if (remainder > 0) payload += new string('=', 4 - remainder);

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("sub", out var sub))
                return Guid.TryParse(sub.GetString(), out var id) ? id : Guid.Empty;
        }
        catch { /* Token malformado: ignorar */ }
        return Guid.Empty;
    }

    private sealed record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
