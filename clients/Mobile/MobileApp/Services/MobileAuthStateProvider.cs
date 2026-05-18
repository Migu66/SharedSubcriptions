using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MobileApp.Services;

/// <summary>
/// Puente entre MobileAuthService y el sistema de autorización de Blazor.
/// Permite usar [Authorize] y AuthorizeView en los componentes MAUI.
/// </summary>
public sealed class MobileAuthStateProvider : AuthenticationStateProvider
{
    private readonly MobileAuthService _authService;

    public MobileAuthStateProvider(MobileAuthService authService)
    {
        _authService = authService;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
            return Unauthenticated();

        var email = await _authService.GetUserEmailAsync() ?? string.Empty;

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private void OnAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static AuthenticationState Unauthenticated()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));
}
