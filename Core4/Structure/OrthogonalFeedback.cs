using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A read-only feedback publication travelling back across the orthogonal frontier toward the site that originally
/// published on a carrier. It carries the requester's desired next value together with the target's current
/// published value so the receiver can inspect both value and resolution preference without collapsing them into
/// anonymous local tension.
/// Example: <c>var feedback = OrthogonalFeedback.Request(lightCarrier, listenerSite, sourceSite, published, desired);</c>
/// </summary>
public sealed record OrthogonalFeedback(
    ISupport Support,
    Ratio Desired,
    Ratio Published,
    Site Source,
    Site Target,
    Provenance Provenance)
{
    public Carrier Carrier => Support.PrimaryCarrier;

    public Measure Potential => Desired.Potential != Measure.Zero
        ? Desired.Potential
        : Published.Potential != Measure.Zero
            ? Published.Potential
            : Source.Potential != Measure.Zero
                ? Source.Potential
                : Measure.One;

    public Measure Direction => Provenance.Direction;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public Ratio ValueDelta => Desired.IsResolved && Published.IsResolved
        ? Desired.Subtract(Published).Normalize()
        : Ratio.Tension(Potential);

    public Ratio DesiredResolution => new(Desired.Dominant, Measure.One);

    public Ratio PublishedResolution => new(Published.Dominant, Measure.One);

    public Ratio ResolutionDelta => DesiredResolution.Subtract(PublishedResolution).Normalize();

    public static OrthogonalFeedback Request(
        ISupport support,
        Site source,
        Site target,
        Ratio published,
        Ratio desired)
    {
        var strength = desired.Potential != Measure.Zero
            ? desired.Potential
            : published.Potential != Measure.Zero
                ? published.Potential
                : source.Potential != Measure.Zero
                    ? source.Potential
                    : Measure.One;
        return new OrthogonalFeedback(
            support,
            desired.Normalize(),
            published.Normalize(),
            source,
            target,
            Provenance.OtherDriven(strength));
    }
}
