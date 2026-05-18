using Foundation;
using UIKit;

namespace MobileApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Inicializar Firebase desde GoogleService-Info.plist antes de cualquier otra cosa
        Firebase.Core.App.Configure();
        return base.FinishedLaunching(application, launchOptions);
    }
}
