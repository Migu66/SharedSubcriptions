using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace MobileApp.Platforms.Android.Services;

/// <summary>
/// Servicio que recibe mensajes FCM cuando la app está en segundo plano o cerrada.
/// Plugin.Firebase ya registra su propio servicio, pero este nos permite crear el
/// canal de notificación y personalizar cómo se muestran los mensajes de datos.
/// </summary>
[Service(Exported = false, Name = "com.sharedsubscriptions.app.AppFirebaseMessagingService")]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class AppFirebaseMessagingService : FirebaseMessagingService
{
    internal const string ChannelId = "shared_subscriptions_channel";
    internal const string ChannelName = "SharedSubscriptions";

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        // FCM muestra las notificaciones automáticamente cuando la app está en background.
        // Este código se ejecuta cuando la app está en PRIMER PLANO y llega un data message.
        var notification = message.GetNotification();
        if (notification is not null)
            ShowLocalNotification(notification.Title ?? ChannelName, notification.Body ?? "");
    }

    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
        // El token ha cambiado — se registrará de nuevo en el próximo login
        System.Diagnostics.Debug.WriteLine($"[FCM] Token renovado: {token[..Math.Min(12, token.Length)]}...");
    }

    internal static void EnsureChannelExists(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26)) return;

        var manager = (NotificationManager?)context.GetSystemService(NotificationService);
        if (manager?.GetNotificationChannel(ChannelId) is not null) return;

        var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.High)
        {
            Description = "Recordatorios de pago y alertas de SharedSubscriptions"
        };
        channel.EnableVibration(true);
        manager?.CreateNotificationChannel(channel);
    }

    private void ShowLocalNotification(string title, string body)
    {
        EnsureChannelExists(this);

        var intent = PackageManager?.GetLaunchIntentForPackage(PackageName ?? "")
                     ?? new Intent(this, typeof(MauiApplication));
        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var pendingIntent = PendingIntent.GetActivity(
            this, 0, intent,
            PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);

        var notification = new Notification.Builder(this, ChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent)
            .Build();

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.Notify(Random.Shared.Next(1, int.MaxValue), notification);
    }
}
