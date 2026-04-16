using Core4.Numerics;

namespace Core4.State;

/// <summary>
/// A read-only feedback publication travelling back across the orthogonal frontier toward the site that originally
/// published on a carrier. It carries the requester's desired next value together with the target's current
/// published value so the receiver can inspect both value and resolution preference without collapsing them into
/// anonymous local tension.
/// Example: <c>var feedback = OrthogonalFeedback.Request(lightCarrier, listenerSite, sourceSite, published, desired);</c>
/// </summary>
public sealed record OrthogonalFeedback(
    ISupport Support,
    SupportProjectionRead DesiredRead,
    SupportProjectionRead PublishedRead,
    Site Source,
    Site Target,
    Provenance Provenance)
{
    public OrthogonalFeedback(
        ISupport support,
        ISupportProjection desired,
        ISupportProjection published,
        Site source,
        Site target,
        Provenance provenance)
        : this(
            support,
            desired.ToSupportRead(),
            published.ToSupportRead(),
            source,
            target,
            provenance)
    {
    }

    public Carrier Carrier => Support.PrimaryCarrier;

    // TODO(core): Delete this compatibility alias once callers consume DesiredRead directly.
    public Ratio Desired => DesiredRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume PublishedRead directly.
    public Ratio Published => PublishedRead.CompatibilityRatio();

    public Measure Potential => DesiredRead.Potential != Measure.Zero
        ? DesiredRead.Potential
        : PublishedRead.Potential != Measure.Zero
            ? PublishedRead.Potential
            : Source.Potential != Measure.Zero
                ? Source.Potential
                : Measure.One;

    public Measure Direction => Provenance.Direction;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public ISupportProjection ValueDeltaRead => Desired.IsResolved && Published.IsResolved
        ? new SupportProjectionRead(Desired.Subtract(Published).Normalize())
        : Ratio.Tension(Potential);

    // TODO(core): Delete this compatibility alias once callers consume ValueDeltaRead directly.
    public Ratio ValueDelta => ValueDeltaRead.CompatibilityRatio();

    public ISupportProjection DesiredResolutionRead => new SupportProjectionRead(new Ratio(Desired.Dominant, Measure.One));

    // TODO(core): Delete this compatibility alias once callers consume DesiredResolutionRead directly.
    public Ratio DesiredResolution => DesiredResolutionRead.CompatibilityRatio();

    public ISupportProjection PublishedResolutionRead => new SupportProjectionRead(new Ratio(Published.Dominant, Measure.One));

    // TODO(core): Delete this compatibility alias once callers consume PublishedResolutionRead directly.
    public Ratio PublishedResolution => PublishedResolutionRead.CompatibilityRatio();

    public ISupportProjection ResolutionDeltaRead => new SupportProjectionRead(
        DesiredResolution.Subtract(PublishedResolution).Normalize());

    // TODO(core): Delete this compatibility alias once callers consume ResolutionDeltaRead directly.
    public Ratio ResolutionDelta => ResolutionDeltaRead.CompatibilityRatio();

    public static OrthogonalFeedback Request(
        ISupport support,
        Site source,
        Site target,
        ISupportProjection published,
        ISupportProjection desired)
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
            desired,
            published,
            source,
            target,
            Provenance.OtherDriven(strength));
    }

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public static OrthogonalFeedback Request(
        ISupport support,
        Site source,
        Site target,
        Ratio published,
        Ratio desired) => Request(support, source, target, (ISupportProjection)published, desired);
}

