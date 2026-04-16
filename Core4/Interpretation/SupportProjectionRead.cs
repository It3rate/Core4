
namespace Core4.Interpretation;

/// <summary>
/// One support-derived read carried as its own structural object rather than as a naked ratio. The current payload is
/// still ratio-shaped for compatibility, but support-first outputs should prefer this wrapper over exposing raw ratio
/// directly.
/// </summary>
public readonly record struct SupportProjectionRead(Ratio Value) : ISupportProjection
{
    public Measure Potential => Value.Potential;

    public Ratio AsRatio() => Value;
}

