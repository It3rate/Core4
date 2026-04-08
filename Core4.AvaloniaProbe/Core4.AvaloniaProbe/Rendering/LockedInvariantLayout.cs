using Avalonia;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted scene geometry for the locked-invariant explorer.
/// </summary>
public readonly record struct LockedInvariantLayout(Rect SceneRect, double Scale, LockedInvariantDemoState State)
{
    public const double SceneWidth = 920d;
    public const double SceneHeight = 340d;
    public const double TickPixels = 4.5d;
    public const double UnitPixels = TickPixels * 8d;

    public Point Map(double x, double y) => new(SceneRect.X + (x * Scale), SceneRect.Y + (y * Scale));

    public double MapX(double x) => SceneRect.X + (x * Scale);

    public double MapY(double y) => SceneRect.Y + (y * Scale);

    public Point ToScene(Point view) => new((view.X - SceneRect.X) / Scale, (view.Y - SceneRect.Y) / Scale);
}
