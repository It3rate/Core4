using Avalonia;
using Avalonia.Controls;

namespace Gojo.GearDemo;

public sealed class GearDemoWindow : Window
{
    public GearDemoWindow()
    {
        Title = "Gojo Gear Projection Demo";
        Width = 1180;
        Height = 760;
        MinWidth = 960;
        MinHeight = 620;
        Content = new GearDemoCanvas();
    }
}
