namespace MobileApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Intentar inicializar FCM si el usuario ya tiene sesión iniciada.
        // Si no lo está, PushNotificationService se inicializará al hacer login
        // gracias al evento MobileAuthService.AuthStateChanged.
        if (IPlatformApplication.Current?.Services is { } services)
        {
            var pushService = services.GetService<PushNotificationService>();
            if (pushService is not null)
                await pushService.InitializeAsync();
        }
    }
}
