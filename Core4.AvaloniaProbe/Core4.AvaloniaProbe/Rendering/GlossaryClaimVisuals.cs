using System;
using Avalonia;
using Avalonia.Media;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryClaimVisuals
{
    public static string Label(Carrier carrier) => carrier.Id switch
    {
        1L => "Heat",
        2L => "Height",
        3L => "Density",
        _ => $"Carrier {carrier.Id}"
    };

    public static string Summary(Carrier carrier) => carrier.Id switch
    {
        1L => "Ordered warmth from cooler to warmer.",
        2L => "Ordered elevation from lower to higher.",
        3L => "Ordered density from sparse to dense.",
        _ => "Ordered support."
    };

    public static string SiteCaption(Carrier carrier, double ratio) => carrier.Id switch
    {
        1L when ratio < 0.2d => "cool site",
        1L when ratio < 0.45d => "tempered site",
        1L when ratio < 0.75d => "warm site",
        1L => "hot site",
        2L when ratio < 0.25d => "low site",
        2L when ratio < 0.6d => "middle site",
        2L => "high site",
        3L when ratio < 0.25d => "light site",
        3L when ratio < 0.6d => "mixed site",
        3L => "dense site",
        _ => "site"
    };

    public static string MeasureLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "heat",
        2L => "height",
        3L => "density",
        _ => "measure"
    };

    public static IBrush TrackBrush(Carrier carrier) => carrier.Id switch
    {
        1L => new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1d, 0d, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse("#4f95ff"), 0d),
                new GradientStop(Color.Parse("#f4703b"), 1d)
            ]
        },
        2L => new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0d, 1d, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse("#7db86e"), 0d),
                new GradientStop(Color.Parse("#194f2f"), 1d)
            ]
        },
        3L => new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1d, 0d, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse("#d9d9d9"), 0d),
                new GradientStop(Color.Parse("#484f57"), 1d)
            ]
        },
        _ => new SolidColorBrush(Color.Parse("#243336"))
    };

    public static IBrush SurfaceBrush(Carrier carrier, double ratio) => new SolidColorBrush(carrier.Id switch
    {
        1L => Blend(Color.Parse("#4f95ff"), Color.Parse("#e3532f"), ratio),
        2L => Blend(Color.Parse("#94cf79"), Color.Parse("#194f2f"), ratio),
        3L => Blend(Color.Parse("#f2f2f2"), Color.Parse("#35393f"), ratio),
        _ => Color.Parse("#243336")
    });

    public static void DrawCarrier(DrawingContext context, Carrier carrier, Point start, Point end)
    {
        switch (carrier.Id)
        {
            case 1L:
                context.DrawLine(new Pen(TrackBrush(carrier), 8.5d, lineCap: PenLineCap.Round), start, end);
                break;

            case 2L:
                context.DrawLine(new Pen(TrackBrush(carrier), 7.5d, lineCap: PenLineCap.Round), start, end);
                DrawHeightTicks(context, start, end);
                break;

            case 3L:
                DrawDensityCarrier(context, start, end);
                break;

            default:
                context.DrawLine(new Pen(TrackBrush(carrier), 7d, lineCap: PenLineCap.Round), start, end);
                break;
        }
    }

    public static void DrawSiteGlyph(DrawingContext context, Carrier carrier, Point center, double ratio, double scale = 1d)
    {
        var stroke = new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 1.4d);
        switch (carrier.Id)
        {
            case 1L:
                ScenePrimitives.DrawCircle(context, center, 10d * scale, SurfaceBrush(carrier, ratio), stroke);
                break;

            case 2L:
                DrawHeightGlyph(context, center, ratio, scale, stroke);
                break;

            case 3L:
                DrawDensityGlyph(context, center, ratio, scale, stroke);
                break;

            default:
                ScenePrimitives.DrawCircle(context, center, 8d * scale, SurfaceBrush(carrier, ratio), stroke);
                break;
        }
    }

    public static void DrawMeasureGlyph(DrawingContext context, Carrier carrier, Point center, double ratio)
    {
        DrawSiteGlyph(context, carrier, center, ratio, 0.72d);
    }

    public static Pen HandlePen => new(new SolidColorBrush(Color.Parse("#c8bca8")), 1.2d);

    private static void DrawHeightTicks(DrawingContext context, Point start, Point end)
    {
        var direction = end - start;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= 1d)
        {
            return;
        }

        var unit = direction / length;
        var perpendicular = new Vector(-unit.Y, unit.X);
        for (var step = 1; step < 5; step++)
        {
            var t = step / 5d;
            var point = start + (direction * t);
            var inset = 5d;
            context.DrawLine(
                new Pen(new SolidColorBrush(Color.Parse("#2f6a3f")), 1.1d),
                point - (perpendicular * inset),
                point + (perpendicular * inset));
        }
    }

    private static void DrawDensityCarrier(DrawingContext context, Point start, Point end)
    {
        var direction = end - start;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= 1d)
        {
            return;
        }

        var unit = direction / length;
        var perpendicular = new Vector(-unit.Y, unit.X);
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start - (perpendicular * 5d), true);
            stream.LineTo(start + (perpendicular * 5d));
            stream.LineTo(end + (perpendicular * 17d));
            stream.LineTo(end - (perpendicular * 17d));
            stream.EndFigure(true);
        }

        context.DrawGeometry(
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1d, 0d, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.FromArgb(45, 74, 82, 90), 0d),
                    new GradientStop(Color.FromArgb(135, 52, 57, 63), 1d)
                ]
            },
            new Pen(TrackBrush(new Carrier(3)), 1.2d),
            geometry);

        for (var row = 0; row < 8; row++)
        {
            var t = (row + 1) / 9d;
            var point = start + (direction * t);
            var spread = 4d + (t * 10d);
            var dots = 1 + (int)Math.Round(t * 4d, MidpointRounding.AwayFromZero);
            for (var dot = 0; dot < dots; dot++)
            {
                var offset = dots == 1 ? 0d : -spread + ((spread * 2d) * dot / (dots - 1d));
                ScenePrimitives.DrawCircle(
                    context,
                    point + (perpendicular * offset),
                    1.35d + (t * 0.65d),
                    new SolidColorBrush(Color.Parse("#40464d")));
            }
        }
    }

    private static void DrawHeightGlyph(DrawingContext context, Point center, double ratio, double scale, Pen stroke)
    {
        var height = (13d + (ratio * 14d)) * scale;
        var shaft = new Pen(SurfaceBrush(new Carrier(2), ratio), 2.6d * scale, lineCap: PenLineCap.Round);
        context.DrawLine(shaft, new Point(center.X, center.Y + (height * 0.32d)), new Point(center.X, center.Y - (height * 0.26d)));
        ScenePrimitives.DrawVerticalTriangle(
            context,
            new Point(center.X, center.Y - (height * 0.46d)),
            6.6d * scale,
            SurfaceBrush(new Carrier(2), ratio),
            stroke,
            true);
    }

    private static void DrawDensityGlyph(DrawingContext context, Point center, double ratio, double scale, Pen stroke)
    {
        var radius = 8d * scale;
        ScenePrimitives.DrawSquare(context, center, radius, SurfaceBrush(new Carrier(3), ratio), stroke, 3d * scale);
        var dots = 1 + (int)Math.Round(ratio * 6d, MidpointRounding.AwayFromZero);
        var columns = Math.Min(3, dots);
        var rows = (int)Math.Ceiling(dots / 3d);
        var dotRadius = Math.Max(0.8d, 1.15d * scale);
        for (var index = 0; index < dots; index++)
        {
            var column = index % columns;
            var row = index / columns;
            var dx = columns == 1 ? 0d : (column - ((columns - 1) * 0.5d)) * 4.1d * scale;
            var dy = rows == 1 ? 0d : (row - ((rows - 1) * 0.5d)) * 4.1d * scale;
            ScenePrimitives.DrawCircle(context, new Point(center.X + dx, center.Y + dy), dotRadius, new SolidColorBrush(Color.Parse("#f7f5ef")));
        }
    }

    private static Color Blend(Color left, Color right, double ratio)
    {
        var t = Math.Clamp(ratio, 0d, 1d);
        return Color.FromArgb(
            (byte)(left.A + ((right.A - left.A) * t)),
            (byte)(left.R + ((right.R - left.R) * t)),
            (byte)(left.G + ((right.G - left.G) * t)),
            (byte)(left.B + ((right.B - left.B) * t)));
    }
}
