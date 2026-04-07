using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Renderer for one top publication plus one bottom input attached to the same fused segment.
/// </summary>
public static class OrthogonalInputRenderer
{
    public static OrthogonalInputLayout Arrange(Rect bounds, OrthogonalInputDemoState state)
    {
        var plot = new Rect(
            bounds.X + 24d,
            bounds.Y + 18d,
            System.Math.Max(160d, bounds.Width - 48d),
            System.Math.Max(220d, bounds.Height - 36d));

        var axisY = plot.Y + (plot.Height * 0.53d);
        var segmentLayout = new FrozenSegmentLayout(
            plot,
            state.Segment,
            16d,
            plot.Left + (plot.Width * 0.5d),
            axisY,
            axisY - 18d,
            axisY + 18d,
            axisY - 42d);

        var top = OrthogonalBranchRenderer.Arrange(segmentLayout.OriginPoint, segmentLayout.PixelsPerMeasure, state.TopBranch, recessive: false);
        var bottom = OrthogonalBranchRenderer.Arrange(segmentLayout.OriginPoint, segmentLayout.PixelsPerMeasure, state.BottomBranch, recessive: true);
        return new OrthogonalInputLayout(segmentLayout, state, top, bottom);
    }

    public static void Render(DrawingContext context, Rect bounds, OrthogonalInputDemoState state)
    {
        var layout = Arrange(bounds, state);
        Render(context, layout);
    }

    public static void Render(DrawingContext context, OrthogonalInputLayout layout)
    {
        FrozenSegmentRenderer.DrawSegment(context, layout.SegmentLayout, includeReadout: false);
        OrthogonalBranchRenderer.Render(context, layout.TopBranch);
        OrthogonalBranchRenderer.Render(context, layout.BottomBranch);
        FrozenSegmentRenderer.DrawReadout(context, layout.SegmentLayout);
    }
}
