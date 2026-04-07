using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Renderer for one fused segment that is publishing on a single top-side orthogonal carrier.
/// </summary>
public static class OrthogonalOutputRenderer
{
    public static OrthogonalOutputLayout Arrange(Rect bounds, OrthogonalOutputDemoState state)
    {
        var plot = new Rect(
            bounds.X + 24d,
            bounds.Y + 18d,
            Math.Max(160d, bounds.Width - 48d),
            Math.Max(220d, bounds.Height - 36d));

        var axisY = plot.Y + (plot.Height * 0.87d);
        var segmentLayout = new FrozenSegmentLayout(
            plot,
            state.Segment,
            16d,
            plot.Left + (plot.Width * 0.5d),
            axisY,
            axisY - 20d,
            axisY + 20d,
            axisY - 46d);

        var outputHeight = Math.Max(0d, state.TopOutput.Raw * segmentLayout.PixelsPerMeasure);
        var tensionReach = state.TopTension == Measure.Zero
            ? 0d
            : Math.Max(segmentLayout.PixelsPerMeasure * 0.92d, Math.Abs(state.TopTension.Raw) * 1.4d);
        var outputTip = new Point(segmentLayout.OriginPoint.X, segmentLayout.AxisY - outputHeight);
        var tensionTip = new Point(segmentLayout.OriginPoint.X, segmentLayout.AxisY - Math.Max(outputHeight, tensionReach));
        var tensionAmplitude = state.TopTension == Measure.Zero
            ? 0d
            : Math.Max(3.8d, Math.Min(24d, Math.Abs(state.TopTension.Raw) * 1.2d));

        return new OrthogonalOutputLayout(
            segmentLayout,
            state,
            outputTip,
            tensionTip,
            tensionAmplitude);
    }

    public static void Render(DrawingContext context, Rect bounds, OrthogonalOutputDemoState state)
    {
        var layout = Arrange(bounds, state);
        Render(context, layout);
    }

    public static void Render(DrawingContext context, OrthogonalOutputLayout layout)
    {
        FrozenSegmentRenderer.DrawSegment(context, layout.SegmentLayout, includeReadout: false);
        DrawTension(context, layout);
        DrawOutput(context, layout);
        FrozenSegmentRenderer.DrawReadout(context, layout.SegmentLayout);
    }

    private static void DrawOutput(DrawingContext context, OrthogonalOutputLayout layout)
    {
        if (!layout.HasOutput)
        {
            return;
        }

        var arrowSize = 14d;
        var basePoint = new Point(layout.OutputTip.X, layout.OutputTip.Y + (arrowSize * 0.94d));
        context.DrawLine(ProbePalette.OrthogonalPen, layout.OriginPoint, basePoint);
        ScenePrimitives.DrawVerticalTriangle(
            context,
            layout.OutputTip,
            arrowSize,
            ProbePalette.Surface,
            new Pen(ProbePalette.Orthogonal, 3.8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round),
            pointsUp: true);
    }

    private static void DrawTension(DrawingContext context, OrthogonalOutputLayout layout)
    {
        if (!layout.HasTension)
        {
            return;
        }

        var pen = layout.State.TopTension.Raw < 0
            ? ProbePalette.TensionNegativePen
            : ProbePalette.TensionPositivePen;
        ScenePrimitives.DrawWave(context, layout.OriginPoint, layout.TensionTip, layout.TensionAmplitude, pen);
    }
}
