using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// The six primitive grade-two forms built from one aligned directed pair and one orthogonal extension.
/// The only live controls are the two aligned endpoints on the directed segment.
/// </summary>
public readonly record struct PrimitiveBasesDemoState(Measure DominantTicks, Measure RecessiveTicks)
{
    private static readonly Measure UnitSpan = new(8);
    private static readonly Carrier Horizontal = new(11);
    private static readonly Carrier Vertical = new(12);
    private static readonly Measure Origin = Measure.Zero;

    public static PrimitiveBasesDemoState Default => new(new Measure(24), new Measure(16));

    public Basis DominantBasis => Basis.Freeze(Horizontal, Origin, UnitSpan);

    public Basis RecessiveBasis => Basis.Freeze(Horizontal, Origin, -UnitSpan);

    public Basis TopBasis => Basis.Freeze(Vertical, Origin, UnitSpan);

    public Basis BottomBasis => Basis.Freeze(Vertical, Origin, -UnitSpan);

    public Ratio DominantValue => DominantBasis.Project(DominantTicks);

    public Ratio RecessiveValue => RecessiveBasis.Project(-RecessiveTicks);

    public Ratio TopValue => DominantValue;

    public Ratio BottomValue => RecessiveValue;

    public Ratio SelfInteraction => DominantValue.Multiply(RecessiveValue).Normalize();

    public Ratio DirectedSegment => DominantValue.Subtract(RecessiveValue).Normalize();

    public Ratio Area => DominantValue.Multiply(TopValue).Normalize();

    public Ratio AntiArea => RecessiveValue.Multiply(BottomValue).Normalize();

    public Ratio AntiComplex => RecessiveValue.Multiply(TopValue).Normalize();

    public Ratio Complex => DominantValue.Multiply(BottomValue).Normalize();

    public double DominantUnits => DominantValue.Normalize().Approximate;

    public double RecessiveUnits => RecessiveValue.Normalize().Approximate;

    public double SelfUnits => SelfInteraction.Normalize().Approximate;

    public double SegmentUnits => DirectedSegment.Normalize().Approximate;

    public bool DominantIsTension => DominantUnits < 0d;

    public bool RecessiveIsTension => RecessiveUnits < 0d;

    public bool SelfIsTension => DominantIsTension || RecessiveIsTension;

    public bool AreaIsTension => DominantIsTension || RecessiveIsTension;

    public bool AntiAreaIsTension => DominantIsTension || RecessiveIsTension;

    public bool ComplexIsTension => DominantIsTension || RecessiveIsTension;

    public bool AntiComplexIsTension => DominantIsTension || RecessiveIsTension;

    public string ComplexReading => $"{RecessiveValue.Normalize()}r + {DominantValue.Normalize()}d";

    public string ApproximateReading => $"({RecessiveValue.Normalize().Approximate:0.0}i + {DominantValue.Normalize().Approximate:0.0})";

    public PrimitiveBasesDemoState WithDominantTicks(Measure ticks) => this with { DominantTicks = ticks };

    public PrimitiveBasesDemoState WithRecessiveTicks(Measure ticks) => this with { RecessiveTicks = ticks };
}
