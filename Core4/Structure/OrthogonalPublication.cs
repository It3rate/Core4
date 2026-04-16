using Core4.Numerics;

namespace Core4.State;

/// <summary>
/// A read-only foreign publication visible across the orthogonal frontier. Unlike tension flow, this payload is not
/// locally consumed or mutated by settlement; it remains an inspectable publication from some source support,
/// carrying both the published value and a source-site reference.
/// Example: <c>var publication = OrthogonalPublication.Publish(lightCarrier, sourceSite);</c>
/// </summary>
public sealed record OrthogonalPublication(ISupport Support, SupportProjectionRead ValueRead, Site Site, Provenance Provenance)
{
    public OrthogonalPublication(ISupport support, ISupportProjection value, Site site, Provenance provenance)
        : this(support, value.ToSupportRead(), site, provenance)
    {
    }

    public Carrier Carrier => Support.PrimaryCarrier;

    // TODO(core): Delete this compatibility alias once callers consume ValueRead directly.
    public Ratio Value => ValueRead.CompatibilityRatio();

    public Measure Potential => ValueRead.Potential != Measure.Zero
        ? ValueRead.Potential
        : Site.Potential != Measure.Zero
            ? Site.Potential
            : Measure.One;

    public Measure Direction => Provenance.Direction;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public static OrthogonalPublication Publish(ISupport support, Site site) => Publish(support, site, site.StateRead);

    public static OrthogonalPublication Publish(ISupport support, Site site, ISupportProjection value)
    {
        var strength = value.Potential != Measure.Zero
            ? value.Potential
            : site.Potential != Measure.Zero
                ? site.Potential
                : Measure.One;
        return new OrthogonalPublication(support, value, site, Provenance.SelfDriven(strength));
    }

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public static OrthogonalPublication Publish(ISupport support, Site site, Ratio value) => Publish(support, site, (ISupportProjection)value);

    public static OrthogonalPublication Receive(ISupport support, Site site, ISupportProjection value)
    {
        var strength = value.Potential != Measure.Zero
            ? value.Potential
            : site.Potential != Measure.Zero
                ? site.Potential
                : Measure.One;
        return new OrthogonalPublication(support, value, site, Provenance.OtherDriven(strength));
    }

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public static OrthogonalPublication Receive(ISupport support, Site site, Ratio value) => Receive(support, site, (ISupportProjection)value);

    public OrthogonalPublication Retune(Site site) => new(Support, ValueRead, site, Provenance);

    public OrthogonalPublication RevalueRead(ISupportProjection value) => new(Support, value, Site, Provenance);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public OrthogonalPublication Revalue(Ratio value) => RevalueRead(value);

    public OrthogonalPublication Rebalance(Provenance provenance) => new(Support, ValueRead, Site, provenance);
}

