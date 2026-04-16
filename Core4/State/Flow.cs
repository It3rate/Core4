namespace Core4.State;

/// <summary>
/// A live support-linked orthogonal influence. In the current engine this still carries one shared Ratio channel,
/// but the intended reading is dual-lane: foreign publication tends to travel toward orthogonal dominant, while
/// disagreement or responsive tension tends to travel back toward orthogonal recessive. Provenance says whether the
/// present pressure is mostly self-sourced, other-sourced, or balanced into oscillation.
/// Example: <c>var flow = new Flow(sensor, Ratio.Tension(4), Provenance.OtherDriven(4));</c>
/// </summary>
public readonly record struct Flow(ISupport Support, SupportProjectionRead ValueRead, Provenance Provenance)
{
    public Flow(ISupport support, ISupportProjection value, Provenance provenance)
        : this(support, value.ToSupportRead(), provenance)
    {
    }

    public Carrier Carrier => Support.PrimaryCarrier;

    // TODO(core): Delete this compatibility alias once callers consume ValueRead directly.
    public Ratio Value => ValueRead.CompatibilityRatio();

    public Measure Potential => ValueRead.Potential;

    public Measure Direction => Provenance.Direction;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public Flow RetuneRead(ISupportProjection value) => new(Support, value, Provenance);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Flow Retune(Ratio value) => RetuneRead(value);

    public Flow Rebalance(Provenance provenance) => new(Support, ValueRead, provenance);
}

