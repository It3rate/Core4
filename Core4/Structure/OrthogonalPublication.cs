using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A read-only foreign publication visible across the orthogonal frontier. Unlike tension flow, this payload is not
/// locally consumed or mutated by settlement; it remains an inspectable publication from some source support,
/// carrying both the published value and a source-site reference.
/// Example: <c>var publication = OrthogonalPublication.Publish(lightCarrier, sourceSite);</c>
/// </summary>
public sealed record OrthogonalPublication(ISupport Support, Ratio Value, Site Site, Provenance Provenance)
{
    public Carrier Carrier => Support.PrimaryCarrier;

    public Measure Potential => Value.Potential != Measure.Zero
        ? Value.Potential
        : Site.Potential != Measure.Zero
            ? Site.Potential
            : Measure.One;

    public Measure Direction => Provenance.Direction;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public static OrthogonalPublication Publish(ISupport support, Site site) => Publish(support, site, site.State);

    public static OrthogonalPublication Publish(ISupport support, Site site, Ratio value)
    {
        var strength = value.Potential != Measure.Zero
            ? value.Potential
            : site.Potential != Measure.Zero
                ? site.Potential
                : Measure.One;
        return new OrthogonalPublication(support, value.Normalize(), site, Provenance.SelfDriven(strength));
    }

    public static OrthogonalPublication Receive(ISupport support, Site site, Ratio value)
    {
        var strength = value.Potential != Measure.Zero
            ? value.Potential
            : site.Potential != Measure.Zero
                ? site.Potential
                : Measure.One;
        return new OrthogonalPublication(support, value.Normalize(), site, Provenance.OtherDriven(strength));
    }

    public OrthogonalPublication Retune(Site site) => new(Support, Value, site, Provenance);

    public OrthogonalPublication Revalue(Ratio value) => new(Support, value.Normalize(), Site, Provenance);

    public OrthogonalPublication Rebalance(Provenance provenance) => new(Support, Value, Site, provenance);
}
