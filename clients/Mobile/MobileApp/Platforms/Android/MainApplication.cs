using Android.App;
using Android.Runtime;
using MobileApp.Platforms.Android.Services;

namespace MobileApp;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
        // Crear el canal de notificación al arrancar la app para que
        // las notificaciones FCM en background usen siempre el canal correcto.
        AppFirebaseMessagingService.EnsureChannelExists(this);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
