using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Gojo.GearDemo;

public sealed class GearDemoApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new GearDemoWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new GearDemoCanvas();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
