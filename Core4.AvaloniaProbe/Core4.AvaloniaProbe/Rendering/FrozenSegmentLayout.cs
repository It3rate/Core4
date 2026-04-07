using System;
using Avalonia;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted geometry for a fused directed segment scene.
/// </summary>
public readonly record struct FrozenSegmentLayout(
    Rect Plot,
    FrozenSegmentDemoState State,
    double PixelsPerMeasure,
    double ZeroAnchorX,
    double AxisY,
    double DominantLaneY,
    double RecessiveLaneY,
    double SegmentOverlayY)
{
    public Point OriginPoint => new(XFor(State.Origin), AxisY);

    public Point DominantUnitPoint => new(XFor(State.DominantUnitLocation), AxisY);

    public Point RecessiveUnitPoint => new(XFor(State.RecessiveUnitLocation), AxisY);

    public Point DominantValuePoint => new(XFor(State.DominantValueLocation), DominantLaneY);

    public Point RecessiveValuePoint => new(XFor(State.RecessiveValueLocation), RecessiveLaneY);

    public double XFor(Measure measure) => ZeroAnchorX + (measure.Raw * PixelsPerMeasure);

    public Measure MeasureAt(double x)
    {
        if (PixelsPerMeasure <= 0d)
        {
            return Measure.Zero;
        }

        var world = (x - ZeroAnchorX) / PixelsPerMeasure;
        return new Measure((long)Math.Round(world, MidpointRounding.AwayFromZero));
    }
}
