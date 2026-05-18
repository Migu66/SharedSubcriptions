using System.Net.Http.Headers;

namespace WebApp.Services;

/// <summary>
/// Interceptor HTTP que añade el Bearer token a todas las peticiones al API Gateway.
/// Si el token está a punto de expirar, lo refresca antes de enviarlo.
/// </summary>
public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthTokenHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _authService.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
