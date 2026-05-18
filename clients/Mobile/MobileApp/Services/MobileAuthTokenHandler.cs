using System.Net.Http.Headers;

namespace MobileApp.Services;

/// <summary>
/// DelegatingHandler que adjunta automáticamente el Bearer token
/// a todas las peticiones al API Gateway.
/// </summary>
public sealed class MobileAuthTokenHandler : DelegatingHandler
{
    private readonly MobileAuthService _authService;

    public MobileAuthTokenHandler(MobileAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _authService.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
