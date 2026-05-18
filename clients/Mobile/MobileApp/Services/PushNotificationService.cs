using System.Net.Http.Json;

#if ANDROID || IOS
using Plugin.Firebase.CloudMessaging;
#endif

namespace MobileApp.Services;

/// <summary>
/// Gestiona el ciclo de vida de las notificaciones push de Firebase Cloud Messaging:
/// obtiene el token del dispositivo tras el login y lo registra en Identity Service.
/// </summary>
public sealed class PushNotificationService
{
    private readonly MobileAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;

    public PushNotificationService(MobileAuthService authService, IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _httpClientFactory = httpClientFactory;

        // Registrar automáticamente el token cada vez que el usuario hace login
        authService.AuthStateChanged += async (_, isAuthenticated) =>
        {
            if (isAuthenticated)
                await InitializeAsync();
        };
    }

    /// <summary>
    /// Inicializa FCM, solicita permiso (iOS) y registra el device token en el servidor.
    /// Llamar al arrancar la app y tras el login.
    /// </summary>
    public async Task InitializeAsync()
    {
#if ANDROID || IOS
        try
        {
            // Suscribirse a notificaciones en primer plano
            CrossFirebaseCloudMessaging.Current.NotificationReceived -= HandleForegroundNotification;
            CrossFirebaseCloudMessaging.Current.NotificationReceived += HandleForegroundNotification;

#if IOS
            // En iOS es obligatorio pedir permiso explícito al usuario
            await CrossFirebaseCloudMessaging.Current.RequestPermissionAsync();
#endif

            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FCM] Token obtenido: {token[..Math.Min(12, token.Length)]}...");
                await RegisterDeviceTokenAsync(token);
            }
        }
        catch (Exception ex)
        {
            // FCM puede fallar en emuladores sin Google Play Services o en simuladores iOS
            System.Diagnostics.Debug.WriteLine($"[FCM] Error de inicialización: {ex.Message}");
        }
#else
        await Task.CompletedTask;
#endif
    }

    private async Task RegisterDeviceTokenAsync(string token)
    {
        var userId = await _authService.GetUserIdAsync();
        if (userId == Guid.Empty) return;

        try
        {
            var client = _httpClientFactory.CreateClient("ApiGateway");
            var response = await client.PostAsJsonAsync(
                $"/api/users/{userId}/device-token",
                new { Token = token, Platform = GetPlatformName() });

            if (response.IsSuccessStatusCode)
                System.Diagnostics.Debug.WriteLine("[FCM] Token registrado en el servidor.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] Error al registrar token: {ex.Message}");
        }
    }

    private static string GetPlatformName()
    {
#if ANDROID
        return "android";
#elif IOS
        return "ios";
#else
        return "unknown";
#endif
    }

#if ANDROID || IOS
    private static void HandleForegroundNotification(object? sender, FCMNotificationReceivedEventArgs e)
    {
        // Notificación recibida con la app abierta — el canal de Android mostrará el aviso
        var title = e.Notification.Title ?? "SharedSubscriptions";
        var body  = e.Notification.Body  ?? "";
        System.Diagnostics.Debug.WriteLine($"[FCM] Notificación en primer plano: {title} — {body}");
    }
#endif
}
