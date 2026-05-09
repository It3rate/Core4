using Avalonia;
using Gojo.GearDemo.Adapters;
using SkiaSharp;

namespace Gojo.GearDemo.Components;

internal static class GojoDemoUi
{
    public static void DrawButton(SKCanvas canvas, Rect rect, string text, Point? hover)
    {
        var active = hover is { } point && rect.Contains(point);
        using var fill = new SKPaint { IsAntialias = true, Color = active ? Palette.ButtonHot : Palette.Button, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Color = active ? Palette.DriverDark : Palette.Separator, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
        canvas.DrawRoundRect(GearDemoAdapters.ToRect(rect), 8f, 8f, fill);
        canvas.DrawRoundRect(GearDemoAdapters.ToRect(rect), 8f, 8f, stroke);
        DrawCenteredText(canvas, text, rect, 13f, Palette.Ink, true);
    }

    public static void DrawSlider(SKCanvas canvas, SliderLayout slider, Point? hover)
    {
        DrawText(canvas, slider.Label, slider.Bounds.X, slider.Bounds.Y, 12f, Palette.Muted, false);
        DrawText(canvas, slider.Readout, slider.Bounds.Right - 42d, slider.Bounds.Y, 13f, Palette.Ink, true);

        using var track = new SKPaint { IsAntialias = true, Color = Palette.SliderTrack, Style = SKPaintStyle.Stroke, StrokeWidth = GearDemoAdapters.ToFloat(slider.Track.Height), StrokeCap = SKStrokeCap.Round };
        using var fill = new SKPaint { IsAntialias = true, Color = Palette.Driver, Style = SKPaintStyle.Stroke, StrokeWidth = GearDemoAdapters.ToFloat(slider.Track.Height), StrokeCap = SKStrokeCap.Round };
        canvas.DrawLine(GearDemoAdapters.ToFloat(slider.Track.Left), GearDemoAdapters.ToFloat(slider.Track.Center.Y), GearDemoAdapters.ToFloat(slider.Track.Right), GearDemoAdapters.ToFloat(slider.Track.Center.Y), track);
        canvas.DrawLine(GearDemoAdapters.ToFloat(slider.Track.Left), GearDemoAdapters.ToFloat(slider.Track.Center.Y), GearDemoAdapters.ToFloat(slider.Knob.X), GearDemoAdapters.ToFloat(slider.Knob.Y), fill);

        var hot = hover is { } point && slider.Bounds.Contains(point);
        using var knob = new SKPaint { IsAntialias = true, Color = hot ? Palette.ButtonHot : Palette.Background, Style = SKPaintStyle.Fill };
        using var ring = new SKPaint { IsAntialias = true, Color = Palette.DriverDark, Style = SKPaintStyle.Stroke, StrokeWidth = 2.1f };
        canvas.DrawCircle(GearDemoAdapters.ToFloat(slider.Knob.X), GearDemoAdapters.ToFloat(slider.Knob.Y), hot ? 8.2f : 7.2f, knob);
        canvas.DrawCircle(GearDemoAdapters.ToFloat(slider.Knob.X), GearDemoAdapters.ToFloat(slider.Knob.Y), hot ? 8.2f : 7.2f, ring);
    }

    public static void DrawCaption(SKCanvas canvas, Rect rect, string line1, string line2)
    {
        DrawCenteredLine(canvas, line1, rect.Center.X, rect.Y, 14f, Palette.Ink, true);
        DrawCenteredLine(canvas, line2, rect.Center.X, rect.Y + 24d, 12f, Palette.Muted, false);
    }

    public static void DrawReadout(SKCanvas canvas, double x, ref double y, string label, string value)
    {
        DrawText(canvas, label, x, y, 11f, Palette.Muted, false);
        y += 17d;
        DrawText(canvas, value, x, y, 13f, Palette.Ink, true);
        y += 30d;
    }

    public static void DrawCenteredText(SKCanvas canvas, string text, Rect rect, float size, SKColor color, bool bold)
    {
        using var textStyle = TextStyle.Create(size, color, bold);
        var bounds = new SKRect();
        textStyle.Font.MeasureText(text, out bounds, textStyle.Paint);
        var x = rect.Center.X - (bounds.Width * 0.5d);
        var y = rect.Center.Y - (bounds.Height * 0.5d) - bounds.Top;
        canvas.DrawText(text, GearDemoAdapters.ToFloat(x), GearDemoAdapters.ToFloat(y), SKTextAlign.Left, textStyle.Font, textStyle.Paint);
    }

    public static void DrawCenteredLine(SKCanvas canvas, string text, double centerX, double y, float size, SKColor color, bool bold)
    {
        using var textStyle = TextStyle.Create(size, color, bold);
        var width = textStyle.Font.MeasureText(text, textStyle.Paint);
        canvas.DrawText(text, GearDemoAdapters.ToFloat(centerX - width * 0.5d), GearDemoAdapters.ToFloat(y - textStyle.Font.Metrics.Ascent), SKTextAlign.Left, textStyle.Font, textStyle.Paint);
    }

    public static void DrawText(SKCanvas canvas, string text, double x, double y, float size, SKColor color, bool bold)
    {
        using var textStyle = TextStyle.Create(size, color, bold);
        canvas.DrawText(text, GearDemoAdapters.ToFloat(x), GearDemoAdapters.ToFloat(y - textStyle.Font.Metrics.Ascent), SKTextAlign.Left, textStyle.Font, textStyle.Paint);
    }

    private sealed class TextStyle : IDisposable
    {
        private readonly SKTypeface typeface;

        private TextStyle(float size, SKColor color, bool bold)
        {
            typeface = SKTypeface.FromFamilyName("Inter", bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            Font = new SKFont(typeface, size)
            {
                Subpixel = true
            };
            Paint = new SKPaint
            {
                IsAntialias = true,
                Color = color
            };
        }

        public SKFont Font { get; }

        public SKPaint Paint { get; }

        public static TextStyle Create(float size, SKColor color, bool bold) =>
            new(size, color, bold);

        public void Dispose()
        {
            Paint.Dispose();
            Font.Dispose();
            typeface.Dispose();
        }
    }

    public static class Palette
    {
        public static readonly SKColor Background = new(249, 247, 242);
        public static readonly SKColor Separator = new(210, 204, 194);
        public static readonly SKColor Ink = new(35, 48, 52);
        public static readonly SKColor Muted = new(94, 106, 110);
        public static readonly SKColor Button = new(239, 244, 247);
        public static readonly SKColor ButtonHot = new(219, 236, 244);
        public static readonly SKColor SliderTrack = new(212, 221, 224);
        public static readonly SKColor Driver = new(36, 124, 156);
        public static readonly SKColor DriverDark = new(20, 88, 116);
        public static readonly SKColor DriverSoft = new(78, 164, 185);
        public static readonly SKColor Driven = new(194, 88, 68);
        public static readonly SKColor DrivenDark = new(135, 52, 42);
        public static readonly SKColor DrivenSoft = new(224, 125, 96);
        public static readonly SKColor Mesh = new(96, 88, 160);
    }
}
