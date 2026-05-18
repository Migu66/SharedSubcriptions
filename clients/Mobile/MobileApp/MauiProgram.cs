using Microsoft.Extensions.Logging;

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

        // HttpClient apuntando al API Gateway
        // En Android, 10.0.2.2 equivale a localhost del PC de desarrollo
        builder.Services.AddHttpClient("ApiGateway", client =>
        {
            client.BaseAddress = new Uri("http://10.0.2.2:5000");
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
