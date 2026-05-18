using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MobileApp.Services;

namespace MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Blazor Hybrid WebView
        builder.Services.AddMauiBlazorWebView();

        // Autorización (necesario para [Authorize] y AuthorizeView en Blazor)
        builder.Services.AddAuthorizationCore();

        // Autenticación con SecureStorage
        builder.Services.AddSingleton<MobileAuthService>();
        builder.Services.AddSingleton<MobileAuthStateProvider>();
        builder.Services.AddSingleton<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<MobileAuthStateProvider>());

        // Handler que adjunta Bearer token a cada petición protegida
        builder.Services.AddTransient<MobileAuthTokenHandler>();

        // URL base del API Gateway según la plataforma de desarrollo:
        //   Android emulador  → 10.0.2.2 (alias especial de Android que apunta al localhost del PC)
        //   iOS simulador     → localhost (el simulador comparte la red del Mac)
        //   Windows           → localhost (para pruebas desde el mismo equipo)
#if ANDROID
        const string apiBaseUrl = "http://10.0.2.2:5000";
#else
        const string apiBaseUrl = "http://localhost:5000";
#endif

        // Cliente HTTP protegido — para llamadas de negocio (con token)
        builder.Services.AddHttpClient("ApiGateway", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        })
        .AddHttpMessageHandler<MobileAuthTokenHandler>();

        // Cliente HTTP sin autenticación — solo para login y refresh (evita recursión)
        builder.Services.AddHttpClient("ApiGatewayAuth", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // Servicios de negocio
        builder.Services.AddTransient<MobileDashboardService>();
        builder.Services.AddTransient<MobileGroupService>();
        builder.Services.AddTransient<MobilePaymentService>();

        // Notificaciones push (Singleton: gestiona eventos y suscripciones)
        builder.Services.AddSingleton<PushNotificationService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

