using System;
using Avalonia;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted geometry for one orthogonal-output scene built on top of a fused directed segment.
/// </summary>
public readonly record struct OrthogonalOutputLayout(
    FrozenSegmentLayout SegmentLayout,
    OrthogonalOutputDemoState State,
    Point OutputTip,
    Point TensionTip,
    double TensionAmplitude)
{
    public Rect Plot => SegmentLayout.Plot;

    public double PixelsPerMeasure => SegmentLayout.PixelsPerMeasure;

    public Point OriginPoint => SegmentLayout.OriginPoint;

    public bool HasOutput => State.TopOutput != Measure.Zero;

    public bool HasTension => State.TopTension != Measure.Zero;

    public Measure OutputAt(double y)
    {
        if (PixelsPerMeasure <= 0d)
        {
            return Measure.Zero;
        }

        var world = (SegmentLayout.AxisY - y) / PixelsPerMeasure;
        return new Measure((long)Math.Round(world, MidpointRounding.AwayFromZero));
    }
}
