using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

/// <summary>
/// Core4-backed primitive fused forms built from one aligned pair and one orthogonal pair.
/// Example: the same signed horizontal segment can feed area, anti-area, and the two complex rays.
/// </summary>
public sealed record PrimitiveBasisSnapshot(
    NumericExplorerPreset Preset,
    Basis DominantBasis,
    Basis RecessiveBasis,
    Basis TopBasis,
    Basis BottomBasis,
    Basis SelfBasis,
    Basis SegmentBasis,
    Basis AreaBasis,
    Basis AntiAreaBasis,
    Basis AntiComplexBasis,
    Basis ComplexBasis,
    Ratio DominantValue,
    Ratio RecessiveValue,
    Ratio TopValue,
    Ratio BottomValue,
    Ratio SelfInteraction,
    Ratio DirectedSegment,
    Ratio Area,
    Ratio AntiArea,
    Ratio AntiComplex,
    Ratio Complex)
{
    public long DominantTicks => Preset.DominantValueOffset.Raw;

    public long RecessiveTicks => Preset.RecessiveValueOffset.Raw;

    public long TopTicks => Math.Abs(DominantTicks);

    public long BottomTicks => Math.Abs(RecessiveTicks);

    public double DominantUnits => DominantValue.Normalize().Approximate;

    public double RecessiveUnits => RecessiveValue.Normalize().Approximate;

    public double DominantMagnitudeUnits => Math.Abs(DominantUnits);

    public double RecessiveMagnitudeUnits => Math.Abs(RecessiveUnits);

    public double TopUnits => Math.Abs(TopValue.Normalize().Approximate);

    public double BottomUnits => Math.Abs(BottomValue.Normalize().Approximate);

    public double SelfUnits => Math.Abs(SelfInteraction.Normalize().Approximate);

    public double SegmentUnits => DirectedSegment.Normalize().Approximate;

    public double SegmentMagnitude => Math.Abs(SegmentUnits);

    public bool SegmentIsNegative => SegmentUnits < 0d;

    public bool DominantIsTension => DominantUnits < 0d;

    public bool RecessiveIsTension => RecessiveUnits < 0d;

    public bool SelfIsTension => DominantIsTension || RecessiveIsTension;

    public bool AreaIsTension => DominantIsTension || RecessiveIsTension;

    public bool AntiAreaIsTension => DominantIsTension || RecessiveIsTension;

    public bool ComplexIsTension => DominantIsTension || RecessiveIsTension;

    public bool AntiComplexIsTension => DominantIsTension || RecessiveIsTension;

    public string SegmentLabel => SegmentUnits.ToString("0.###");

    public string SelfLabel => SelfUnits.ToString("0.###");

    public string AreaLabel => Area.Normalize().Approximate.ToString("0.###");

    public string AntiAreaLabel => AntiArea.Normalize().Approximate.ToString("0.###");

    public string ComplexLabel => Complex.Normalize().Approximate.ToString("0.###");

    public string AntiComplexLabel => AntiComplex.Normalize().Approximate.ToString("0.###");

    public string ComplexReading => $"{RecessiveValue.Normalize()}r + {DominantValue.Normalize()}d";

    public string ApproximateReading => $"({RecessiveValue.Normalize().Approximate:0.0}i + {DominantValue.Normalize().Approximate:0.0})";
}
