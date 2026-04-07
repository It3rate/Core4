using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Renderer for one fused directed segment built from an opposed pair.
/// </summary>
public static class FrozenSegmentRenderer
{
    public static FrozenSegmentLayout Arrange(Rect bounds, FrozenSegmentDemoState state)
    {
        var plot = new Rect(
            bounds.X + 24d,
            bounds.Y + 18d,
            Math.Max(160d, bounds.Width - 48d),
            Math.Max(220d, bounds.Height - 36d));

        var axisY = plot.Y + (plot.Height * 0.56d);
        return new FrozenSegmentLayout(
            plot,
            state,
            16d,
            plot.Left + (plot.Width * 0.5d),
            axisY,
            axisY - 14d,
            axisY + 14d,
            axisY - 32d);
    }

    public static void Render(DrawingContext context, Rect bounds, FrozenSegmentDemoState state)
    {
        var layout = Arrange(bounds, state);
        Render(context, layout);
    }

    public static void Render(DrawingContext context, FrozenSegmentLayout layout)
    {
        context.FillRectangle(ProbePalette.Surface, layout.Plot);
        context.DrawLine(ProbePalette.AxisPen, new Point(layout.Plot.Left, layout.AxisY), new Point(layout.Plot.Right, layout.AxisY));
        DrawTicks(context, layout);
        DrawDirectedOverlay(context, layout);
        DrawRecessiveUnit(context, layout);
        DrawDominantUnit(context, layout);
        DrawDominantValue(context, layout);
        DrawRecessiveValue(context, layout);
        ScenePrimitives.DrawSquare(context, layout.OriginPoint, 5.2d, ProbePalette.Surface, new Pen(ProbePalette.Origin, 3.4d, lineCap: PenLineCap.Round), 3.1d);
        DrawReadout(context, layout);
    }

    private static void DrawDirectedOverlay(DrawingContext context, FrozenSegmentLayout layout)
    {
        var start = new Point(layout.RecessiveValuePoint.X, layout.SegmentOverlayY);
        var end = new Point(layout.DominantValuePoint.X, layout.SegmentOverlayY);
        var arrowSize = 18d;
        var directionRight = end.X >= start.X;
        var lineEnd = directionRight
            ? new Point(end.X - (arrowSize * 0.94d), end.Y)
            : new Point(end.X + (arrowSize * 0.94d), end.Y);
        context.DrawLine(new Pen(ProbePalette.Overlay, 4.6d, lineCap: PenLineCap.Round), start, lineEnd);
        ScenePrimitives.DrawCircle(context, start, 7.2d, ProbePalette.Overlay);
        ScenePrimitives.DrawTriangle(context, end, arrowSize, ProbePalette.Overlay, pointsLeft: !directionRight);
    }

    private static void DrawDominantUnit(DrawingContext context, FrozenSegmentLayout layout)
    {
        var arrowSize = 14d;
        var basePoint = new Point(layout.DominantUnitPoint.X - (arrowSize * 0.94d), layout.AxisY);
        context.DrawLine(ProbePalette.DominantUnitPen, layout.OriginPoint, basePoint);
        ScenePrimitives.DrawTriangle(
            context,
            layout.DominantUnitPoint,
            arrowSize,
            ProbePalette.Surface,
            new Pen(ProbePalette.Dominant, 3.8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
    }

    private static void DrawRecessiveUnit(DrawingContext context, FrozenSegmentLayout layout)
    {
        context.DrawLine(new Pen(ProbePalette.DominantSoft, 4.6d, lineCap: PenLineCap.Round), layout.OriginPoint, layout.RecessiveUnitPoint);
        ScenePrimitives.DrawCircle(context, layout.RecessiveUnitPoint, 5.5d, Brushes.White, new Pen(ProbePalette.DominantSoft, 3d, lineCap: PenLineCap.Round));
    }

    private static void DrawDominantValue(DrawingContext context, FrozenSegmentLayout layout)
    {
        var arrowSize = 14.8d;
        var pointsLeft = layout.State.DominantValueOffset.Raw < 0;
        var basePoint = pointsLeft
            ? new Point(layout.DominantValuePoint.X + (arrowSize * 0.94d), layout.DominantLaneY)
            : new Point(layout.DominantValuePoint.X - (arrowSize * 0.94d), layout.DominantLaneY);
        context.DrawLine(ProbePalette.DominantValuePen, new Point(layout.OriginPoint.X, layout.DominantLaneY), basePoint);
        ScenePrimitives.DrawTriangle(context, layout.DominantValuePoint, arrowSize, ProbePalette.Dominant, pointsLeft: pointsLeft);
    }

    private static void DrawRecessiveValue(DrawingContext context, FrozenSegmentLayout layout)
    {
        context.DrawLine(new Pen(ProbePalette.DominantSoft, 3.8d, lineCap: PenLineCap.Round), new Point(layout.OriginPoint.X, layout.RecessiveLaneY), layout.RecessiveValuePoint);
        ScenePrimitives.DrawCircle(context, layout.RecessiveValuePoint, 5.1d, ProbePalette.DominantSoft);
    }

    private static void DrawTicks(DrawingContext context, FrozenSegmentLayout layout)
    {
        var leftMinimum = (long)Math.Floor((layout.OriginPoint.X - layout.Plot.Left) / layout.PixelsPerMeasure) + 1L;
        var rightMaximum = (long)Math.Floor((layout.Plot.Right - layout.OriginPoint.X) / layout.PixelsPerMeasure);

        for (var index = 1L; index <= rightMaximum; index++)
        {
            var x = layout.OriginPoint.X + (index * layout.PixelsPerMeasure);
            var isMajor = index % Math.Max(1L, layout.State.Span.Raw) == 0L;
            DrawTick(context, x, layout.AxisY, isMajor, ProbePalette.Dominant, false, isMajor ? (index / layout.State.Span.Raw).ToString() : null);
        }

        for (var index = 1L; index <= leftMinimum; index++)
        {
            var x = layout.OriginPoint.X - (index * layout.PixelsPerMeasure);
            var isMajor = index % Math.Max(1L, layout.State.Span.Raw) == 0L;
            DrawTick(context, x, layout.AxisY, isMajor, ProbePalette.DominantSoft, true, isMajor ? (index / layout.State.Span.Raw).ToString() : null);
        }
    }

    private static void DrawTick(
        DrawingContext context,
        double x,
        double axisY,
        bool isMajor,
        IBrush labelBrush,
        bool topLabel,
        string? label)
    {
        var tickPen = isMajor ? ProbePalette.MajorTickPen : ProbePalette.TickPen;
        var height = isMajor ? 4.2d : 3d;
        context.DrawLine(tickPen, new Point(x, axisY - height), new Point(x, axisY + height));

        if (label is null)
        {
            return;
        }

        ScenePrimitives.DrawCenteredLabel(
            context,
            label,
            new Point(x - 4d, topLabel ? axisY - 20d : axisY + 8d),
            9d,
            labelBrush,
            fontFamily: ProbePalette.MonoFont);
    }

    private static void DrawReadout(DrawingContext context, FrozenSegmentLayout layout)
    {
        var centerX = layout.Plot.Left + (layout.Plot.Width * 0.22d);
        var baseline = new Point(centerX, layout.Plot.Top + 1d);
        var left = CreateText($"{layout.State.RecessiveReading}r +", 17d, ProbePalette.DominantSoft);
        var right = CreateText($" {layout.State.DominantReading}d", 17d, ProbePalette.Dominant);
        var totalWidth = left.Width + right.Width;
        var leftPoint = new Point(baseline.X - (totalWidth * 0.5d), baseline.Y);
        context.DrawText(left, leftPoint);
        context.DrawText(right, new Point(leftPoint.X + left.Width, baseline.Y));

        ScenePrimitives.DrawCenteredLabel(
            context,
            layout.State.ApproximateReading,
            new Point(centerX - 8d, layout.Plot.Top + 22d),
            12d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
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
