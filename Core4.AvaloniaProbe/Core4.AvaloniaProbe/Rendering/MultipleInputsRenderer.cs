using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Renderer for one output with several bottom-side input branches attached to the same fused segment.
/// </summary>
public static class MultipleInputsRenderer
{
    public static MultipleInputsLayout Arrange(Rect bounds, MultipleInputsDemoState state)
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
        var bottoms = ImmutableArray.CreateBuilder<OrthogonalBranchLayout>(state.BottomBranches.Length);
        for (var index = 0; index < state.BottomBranches.Length; index++)
        {
            bottoms.Add(
                OrthogonalBranchRenderer.Arrange(
                    segmentLayout.OriginPoint,
                    segmentLayout.PixelsPerMeasure,
                    state.BottomBranches[index],
                    recessive: true,
                    slotIndex: index,
                    slotCount: state.BottomBranches.Length));
        }

        return new MultipleInputsLayout(segmentLayout, state, top, bottoms.ToImmutable());
    }

    public static void Render(DrawingContext context, Rect bounds, MultipleInputsDemoState state)
    {
        var layout = Arrange(bounds, state);
        Render(context, layout);
    }

    public static void Render(DrawingContext context, MultipleInputsLayout layout)
    {
        FrozenSegmentRenderer.DrawSegment(context, layout.SegmentLayout, includeReadout: false);
        OrthogonalBranchRenderer.Render(context, layout.TopBranch);

        foreach (var branch in layout.BottomBranches)
        {
            OrthogonalBranchRenderer.Render(context, branch);
        }

        FrozenSegmentRenderer.DrawReadout(context, layout.SegmentLayout);
    }
}
