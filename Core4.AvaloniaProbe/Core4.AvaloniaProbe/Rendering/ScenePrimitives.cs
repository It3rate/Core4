using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Reusable low-level glyphs for Core4 scene renderers.
/// </summary>
public static class ScenePrimitives
{
    public static void DrawLabel(
        DrawingContext context,
        string text,
        Point point,
        double size,
        IBrush brush,
        FontWeight weight = FontWeight.Normal,
        FontFamily? fontFamily = null)
    {
        context.DrawText(
            new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily ?? FontFamily.Default, FontStyle.Normal, weight),
                size,
                brush),
            point);
    }

    public static void DrawCenteredLabel(
        DrawingContext context,
        string text,
        Point center,
        double size,
        IBrush brush,
        FontWeight weight = FontWeight.Normal,
        FontFamily? fontFamily = null)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily ?? FontFamily.Default, FontStyle.Normal, weight),
            size,
            brush);

        context.DrawText(formatted, new Point(center.X - (formatted.Width * 0.5d), center.Y));
    }

    public static void DrawCircle(DrawingContext context, Point center, double radius, IBrush fill, Pen? stroke = null)
    {
        context.DrawEllipse(fill, stroke, center, radius, radius);
    }

    public static void DrawSquare(DrawingContext context, Point center, double radius, IBrush fill, Pen? stroke = null, double cornerRadius = 2d)
    {
        context.DrawRectangle(fill, stroke, new Rect(center.X - radius, center.Y - radius, radius * 2d, radius * 2d), cornerRadius, cornerRadius);
    }

    public static void DrawTriangle(DrawingContext context, Point tip, double size, IBrush fill, Pen? stroke = null, bool pointsLeft = false)
    {
        context.DrawGeometry(fill, stroke, TriangleGeometry(tip, size, pointsLeft));
    }

    public static double Distance(Point left, Point right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static StreamGeometry TriangleGeometry(Point tip, double size, bool pointsLeft)
    {
        var height = size * 0.58d;
        var shoulder = size * 0.34d;
        var back = size * 0.92d;
        var control = size * 0.14d;
        var direction = pointsLeft ? -1d : 1d;
        var tipPoint = tip;
        var backX = tip.X - (direction * back);
        var shoulderX = tip.X - (direction * shoulder);
        var controlX = tip.X - (direction * control);
        var geometry = new StreamGeometry();
        using var stream = geometry.Open();
        stream.BeginFigure(new Point(backX, tip.Y - height), true);
        stream.QuadraticBezierTo(
            new Point(backX - (direction * (size * 0.08d)), tip.Y - (height * 0.72d)),
            new Point(shoulderX, tip.Y - height));
        stream.QuadraticBezierTo(
            new Point(controlX, tip.Y - (height * 0.34d)),
            tipPoint);
        stream.QuadraticBezierTo(
            new Point(controlX, tip.Y + (height * 0.34d)),
            new Point(shoulderX, tip.Y + height));
        stream.QuadraticBezierTo(
            new Point(backX - (direction * (size * 0.08d)), tip.Y + (height * 0.72d)),
            new Point(backX, tip.Y + height));
        stream.EndFigure(true);
        return geometry;
    }
}
