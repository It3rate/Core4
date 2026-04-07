using Avalonia.Media;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Shared scene colors and pens for the Avalonia Core4 demos.
/// </summary>
public static class ProbePalette
{
    public static readonly FontFamily DisplayFont = new("Palatino Linotype, Book Antiqua, Palatino, Georgia");
    public static readonly FontFamily MonoFont = new("Cascadia Code, Consolas, Menlo");
    public static readonly FontFamily UiFont = new("Trebuchet MS, Aptos, Segoe UI");

    public static readonly IBrush Surface = Brush("#f7f1e4");
    public static readonly IBrush SurfaceSoft = Brush("#efe8da");
    public static readonly IBrush Axis = Brush("#c8c4b8");
    public static readonly IBrush Tick = Brush("#ddd8cc");
    public static readonly IBrush Ink = Brush("#243336");
    public static readonly IBrush Muted = Brush("#67757a");
    public static readonly IBrush Dominant = Brush("#0f4fa4");
    public static readonly IBrush DominantSoft = Brush("#4a74f3");
    public static readonly IBrush Origin = Brush("#b00fb3");
    public static readonly IBrush Orthogonal = Brush("#d10d1f");
    public static readonly IBrush OrthogonalInput = Brush("#ff7a00");
    public static readonly IBrush TensionPositive = Brush("#699165");
    public static readonly IBrush TensionNegative = Brush("#9a6b5e");
    public static readonly IBrush Hidden = Brush("#8a8d87");
    public static readonly IBrush HiddenZone = Brush("#ebe6df");
    public static readonly IBrush Overlay = Brush("#e1cc9d");

    public static readonly Pen AxisPen = new(Axis, 1.8d, lineCap: PenLineCap.Round);
    public static readonly Pen TickPen = new(Tick, 1d, lineCap: PenLineCap.Round);
    public static readonly Pen MajorTickPen = new(Dominant, 1d, lineCap: PenLineCap.Round);
    public static readonly Pen DominantUnitPen = new(Dominant, 4.6d, lineCap: PenLineCap.Round);
    public static readonly Pen DominantValuePen = new(Dominant, 4.4d, lineCap: PenLineCap.Round);
    public static readonly Pen OrthogonalPen = new(Orthogonal, 4.6d, lineCap: PenLineCap.Round);
    public static readonly Pen OrthogonalInputPen = new(OrthogonalInput, 4.6d, lineCap: PenLineCap.Round);
    public static readonly Pen TensionPositivePen = new(TensionPositive, 2.6d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    public static readonly Pen TensionNegativePen = new(TensionNegative, 2.6d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    public static readonly Pen HiddenValuePen = new(Hidden, 4.4d, dashStyle: new DashStyle(new[] { 1.6d, 1.6d }, 0d), lineCap: PenLineCap.Round);
    public static readonly Pen OriginPen = new(Origin, 4.8d, lineCap: PenLineCap.Round);
    public static readonly Pen HiddenZonePen = new(Tick, 1d, lineCap: PenLineCap.Round);

    private static SolidColorBrush Brush(string value) => new(Color.Parse(value));
}
