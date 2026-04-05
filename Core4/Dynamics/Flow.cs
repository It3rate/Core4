using Core4.Numerics;
using Core4.Structure;

namespace Core4.Dynamics;

/// <summary>
/// A live support-linked influence. The value says what is moving, while provenance says whether that pressure is
/// mostly self-sourced, other-sourced, or balanced into oscillation.
/// Example: <c>var flow = new Flow(sensor, Ratio.Tension(4), Provenance.OtherDriven(4));</c>
/// </summary>
public readonly record struct Flow(ISupport Support, Ratio Value, Provenance Provenance)
{
    public Carrier Carrier => Support.PrimaryCarrier;

    public Measure Potential => Value.Potential;

    public Measure Direction => Provenance.Direction;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public Flow Retune(Ratio value) => new(Support, value.Normalize(), Provenance);

    public Flow Rebalance(Provenance provenance) => new(Support, Value.Normalize(), provenance);
}
