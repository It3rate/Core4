using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// One orthogonal publication or input expressed directly as a live Core4 flow plus a branch-local tension ratio.
/// </summary>
public readonly record struct OrthogonalBranchModel(Flow Signal, Ratio Tension)
{
    public Measure SignalMagnitude => Signal.Value.Potential;

    public Measure TensionValue => Tension.Recessive;

    public Measure TensionMagnitude => Tension.Potential;

    public bool HasSignal => SignalMagnitude != Measure.Zero;

    public bool HasTension => TensionValue != Measure.Zero;

    public bool IsIncoming => Signal.Provenance.IsOther;

    public bool IsOutgoing => !IsIncoming;
}
