using Core4.Numerics;
namespace Core4.Structure;

/// <summary>
/// A live support-linked orthogonal influence. In the current engine this still carries one shared Ratio channel,
/// but the intended reading is dual-lane: foreign publication tends to travel toward orthogonal dominant, while
/// disagreement or responsive tension tends to travel back toward orthogonal recessive. Provenance says whether the
/// present pressure is mostly self-sourced, other-sourced, or balanced into oscillation.
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
