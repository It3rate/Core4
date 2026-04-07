using System;
using Avalonia;
using Avalonia.Media;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// A first reusable Core4 renderer: it accepts one dominant-only frozen basis plus a live site and interprets
/// them into the dominant-only half-line view.
/// </summary>
public static class DominantOnlyRenderer
{
    public static DominantOnlyLayout Arrange(Rect bounds, Basis basis, Site valueSite)
    {
        var relation = basis.Relations[0];
        var reading = basis.Project(valueSite);
        var plot = new Rect(
            bounds.X + 24d,
            bounds.Y + 18d,
            Math.Max(160d, bounds.Width - 48d),
            Math.Max(220d, bounds.Height - 36d));

        return new DominantOnlyLayout(
            plot,
            relation,
            valueSite,
            reading,
            16d,
            plot.Left + (plot.Width * 0.46d),
            plot.Y + (plot.Height * 0.52d),
            plot.Y + (plot.Height * 0.475d));
    }

    public static void Render(DrawingContext context, Rect bounds, Basis basis, Site valueSite)
    {
        var layout = Arrange(bounds, basis, valueSite);
        Render(context, layout);
    }

    public static void Render(DrawingContext context, DominantOnlyLayout layout)
    {
        context.FillRectangle(ProbePalette.Surface, layout.Plot);

        if (layout.IsHiddenInverse)
        {
            context.DrawRectangle(ProbePalette.HiddenZone, ProbePalette.HiddenZonePen, layout.HiddenZone, 18d, 18d);
        }

        context.DrawLine(ProbePalette.AxisPen, new Point(layout.Plot.Left, layout.AxisY), new Point(layout.Plot.Right, layout.AxisY));
        DrawTicks(context, layout);

        var unitArrowSize = 14d;
        var valueArrowSize = 14.8d;
        var unitBase = new Point(layout.UnitPoint.X - (unitArrowSize * 0.78d), layout.UnitPoint.Y);
        context.DrawLine(ProbePalette.DominantUnitPen, layout.OriginPoint, unitBase);
        ScenePrimitives.DrawTriangle(context, layout.UnitPoint, unitArrowSize, ProbePalette.Surface, new Pen(ProbePalette.Dominant, 3.8d, lineCap: PenLineCap.Round));

        var valuePen = layout.IsHiddenInverse ? ProbePalette.HiddenValuePen : ProbePalette.DominantValuePen;
        var valueBase = layout.IsHiddenInverse
            ? new Point(layout.ValuePoint.X + (valueArrowSize * 0.78d), layout.ValuePoint.Y)
            : new Point(layout.ValuePoint.X - (valueArrowSize * 0.78d), layout.ValuePoint.Y);
        context.DrawLine(valuePen, layout.ValueLaneOrigin, valueBase);

        if (layout.IsHiddenInverse)
        {
            ScenePrimitives.DrawTriangle(context, layout.ValuePoint, valueArrowSize, ProbePalette.Hidden, pointsLeft: true);
        }
        else
        {
            ScenePrimitives.DrawTriangle(context, layout.ValuePoint, valueArrowSize, ProbePalette.Dominant);
        }

        ScenePrimitives.DrawSquare(context, layout.OriginPoint, 5.2d, ProbePalette.Surface, new Pen(ProbePalette.Origin, 3.4d, lineCap: PenLineCap.Round), 3.1d);
        DrawReadout(context, layout);
    }

    private static void DrawTicks(DrawingContext context, DominantOnlyLayout layout)
    {
        var stride = Math.Max(1L, layout.UnitSpan.Raw);
        var relativeMinimum = (long)Math.Floor((layout.Plot.Left - layout.OriginPoint.X) / layout.PixelsPerMeasure) - 1L;
        var relativeMaximum = (long)Math.Ceiling((layout.Plot.Right - layout.OriginPoint.X) / layout.PixelsPerMeasure) + 1L;

        for (var relative = relativeMinimum; relative <= relativeMaximum; relative++)
        {
            var isOrigin = relative == 0;
            var isMajor = !isOrigin && relative % stride == 0;
            var x = layout.OriginPoint.X + (relative * layout.PixelsPerMeasure);

            if (x < layout.Plot.Left - 4d || x > layout.Plot.Right + 4d)
            {
                continue;
            }

            var tickPen = isMajor ? ProbePalette.MajorTickPen : ProbePalette.TickPen;
            var height = isMajor ? 4.2d : 3d;

            context.DrawLine(tickPen, new Point(x, layout.AxisY - height), new Point(x, layout.AxisY + height));

            if (isMajor)
            {
                ScenePrimitives.DrawCenteredLabel(
                    context,
                    (relative / stride).ToString(),
                    new Point(x - 4d, layout.AxisY + 8d),
                    9d,
                    ProbePalette.DominantSoft,
                    fontFamily: ProbePalette.MonoFont);
            }
        }
    }

    private static void DrawReadout(DrawingContext context, DominantOnlyLayout layout)
    {
        var dominantRatio = layout.IsHiddenInverse
            ? Ratio.Tension((layout.OriginLocation - layout.ValueLocation).Absolute)
            : layout.Reading;

        var dominantApprox = layout.UnitSpan.Raw == 0
            ? 0d
            : (double)(layout.ValueLocation.Raw - layout.OriginLocation.Raw) / layout.UnitSpan.Raw;

        var centerX = layout.Plot.Left + (layout.Plot.Width * 0.22d);
        var baseline = new Point(centerX, layout.Plot.Top + 1d);
        DrawSplitReading(context, baseline, dominantRatio);
        ScenePrimitives.DrawCenteredLabel(
            context,
            $"(0.0i + {dominantApprox:F1})",
            new Point(centerX - 8d, layout.Plot.Top + 22d),
            12d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawSplitReading(DrawingContext context, Point center, Ratio dominantRatio)
    {
        var left = CreateText("0/0r +", 17d, ProbePalette.DominantSoft);
        var right = CreateText($" {dominantRatio}d", 17d, ProbePalette.Dominant);
        var totalWidth = left.Width + right.Width;
        var leftPoint = new Point(center.X - (totalWidth * 0.5d), center.Y);
        var rightPoint = new Point(leftPoint.X + left.Width, center.Y);
        context.DrawText(left, leftPoint);
        context.DrawText(right, rightPoint);
    }

    private static FormattedText CreateText(string text, double size, IBrush brush) =>
        new(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, FontWeight.SemiBold),
            size,
            brush);
}
