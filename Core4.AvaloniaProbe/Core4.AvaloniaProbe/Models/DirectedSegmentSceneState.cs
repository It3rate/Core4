using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// A tiny Core4-backed state bundle for the Avalonia probe. It keeps the directed segment entirely in Core4
/// terms so the scene can render real basis/value positions instead of parallel UI-only numbers.
/// </summary>
public sealed record DirectedSegmentSceneState(
    Measure Origin,
    Measure DominantSpan,
    Measure RecessiveSpan,
    Measure DominantValue,
    Measure RecessiveValue)
{
    public static DirectedSegmentSceneState Default => new(
        Origin: 0,
        DominantSpan: 8,
        RecessiveSpan: 8,
        DominantValue: 6,
        RecessiveValue: 4);

    public Carrier Carrier => new(1);

    public Basis DominantBasis => Basis.Freeze(Carrier, Origin, Origin + DominantSpan);

    public Basis RecessiveBasis => Basis.Freeze(Carrier, Origin, Origin - RecessiveSpan);

    public Basis SegmentBasis => Basis.Fuse(RecessiveBasis, DominantBasis);

    public Measure DominantAnchor => Origin + DominantSpan;

    public Measure RecessiveAnchor => Origin - RecessiveSpan;

    public Measure DominantPosition => Origin + DominantValue;

    public Measure RecessivePosition => Origin - RecessiveValue;

    public Measure Length => DominantValue + RecessiveValue;

    public Ratio DominantReading => DominantBasis.Project(DominantPosition).Normalize();

    public Ratio RecessiveReading => RecessiveBasis.Project(RecessivePosition).Normalize();

    public Ratio Reading => RecessiveReading.Add(DominantReading).Normalize();

    public DirectedSegmentSceneState WithTotalLength(Measure totalLength) =>
        this with { DominantValue = new Measure(totalLength.Raw - RecessiveValue.Raw) };
}
