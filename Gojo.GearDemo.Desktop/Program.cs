using System;
using Avalonia;
using Gojo.GearDemo;
using Gojo.GearDemo.Desktop.Services;

namespace Gojo.GearDemo.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        GearDemoHostServices.PublishWebAsync = GearDemoWebPublisher.PublishDeployAndOpenAsync;

        AppBuilder.Configure<GearDemoApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
