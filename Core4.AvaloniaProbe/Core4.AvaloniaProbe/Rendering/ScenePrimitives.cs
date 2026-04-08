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

    public static void DrawVerticalTriangle(
        DrawingContext context,
        Point tip,
        double size,
        IBrush fill,
        Pen? stroke = null,
        bool pointsUp = true)
    {
        context.DrawGeometry(fill, stroke, VerticalTriangleGeometry(tip, size, pointsUp));
    }

    public static void DrawDirectedTriangle(
        DrawingContext context,
        Point tip,
        double size,
        IBrush fill,
        Vector direction,
        Pen? stroke = null)
    {
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        var unit = length <= 0d
            ? new Vector(1d, 0d)
            : new Vector(direction.X / length, direction.Y / length);
        var perpendicular = new Vector(-unit.Y, unit.X);
        var back = size * 0.92d;
        var width = size * 0.68d;
        var backCenter = tip - (unit * back);
        var left = backCenter + (perpendicular * width);
        var right = backCenter - (perpendicular * width);

        var geometry = new StreamGeometry();
        using var stream = geometry.Open();
        stream.BeginFigure(left, true);
        stream.LineTo(tip);
        stream.LineTo(right);
        stream.EndFigure(true);
        context.DrawGeometry(fill, stroke, geometry);
    }

    public static void DrawWave(DrawingContext context, Point start, Point end, double amplitude, Pen stroke, int minimumCycles = 3)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var distance = Math.Sqrt((dx * dx) + (dy * dy));
        if (distance < 1d || amplitude <= 0d)
        {
            return;
        }

        var ux = dx / distance;
        var uy = dy / distance;
        var px = -uy;
        var py = ux;
        var cycles = Math.Max(minimumCycles, (int)Math.Ceiling(distance / 24d));
        var samples = Math.Max(32, cycles * 18);
        var geometry = new StreamGeometry();
        using var stream = geometry.Open();

        for (var index = 0; index <= samples; index++)
        {
            var t = index / (double)samples;
            var alongX = start.X + (ux * distance * t);
            var alongY = start.Y + (uy * distance * t);
            var offset = Math.Sin(t * Math.PI * 2d * cycles) * Math.Sin(t * Math.PI) * amplitude;
            var point = new Point(alongX + (px * offset), alongY + (py * offset));

            if (index == 0)
            {
                stream.BeginFigure(point, false);
            }
            else
            {
                stream.LineTo(point);
            }
        }

        stream.EndFigure(false);
        context.DrawGeometry(null, stroke, geometry);
    }

    public static double Distance(Point left, Point right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static StreamGeometry TriangleGeometry(Point tip, double size, bool pointsLeft)
    {
        var height = size * 0.68d;
        var back = size * 0.92d;
        var direction = pointsLeft ? -1d : 1d;
        var backX = tip.X - (direction * back);
        var geometry = new StreamGeometry();
        using var stream = geometry.Open();
        stream.BeginFigure(new Point(backX, tip.Y - height), true);
        stream.LineTo(tip);
        stream.LineTo(new Point(backX, tip.Y + height));
        stream.EndFigure(true);
        return geometry;
    }

    private static StreamGeometry VerticalTriangleGeometry(Point tip, double size, bool pointsUp)
    {
        var width = size * 0.68d;
        var back = size * 0.92d;
        var direction = pointsUp ? 1d : -1d;
        var backY = tip.Y + (direction * back);
        var geometry = new StreamGeometry();
        using var stream = geometry.Open();
        stream.BeginFigure(new Point(tip.X - width, backY), true);
        stream.LineTo(tip);
        stream.LineTo(new Point(tip.X + width, backY));
        stream.EndFigure(true);
        return geometry;
    }
}
