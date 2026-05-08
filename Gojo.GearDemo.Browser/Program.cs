using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Gojo.GearDemo;

internal sealed partial class Program
{
    private static Task Main(string[] args) =>
        AppBuilder.Configure<GearDemoApp>()
            .WithInterFont()
            .StartBrowserAppAsync("out");
}
