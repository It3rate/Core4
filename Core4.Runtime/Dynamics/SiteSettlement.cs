using System.Collections.Immutable;
using Core4.Aliases;

namespace Core4.Dynamics;

/// <summary>
/// One local settlement pass. TODO: read phase values dynamically and traverse them through a general external
/// inspector so multiple perspectives can inspect the same structure at once without storing cursor state here.
/// It exposes the anchor, incoming evidence, publication, feedback, and carried remainder before producing the next
/// site state, and it also yields explicitly numbered phases for inspection.
/// Example: <c>var step = SiteDynamics.Settle(site); var next = step.Next;</c>
/// </summary>
public readonly record struct SiteSettlement(
    Site Source,
    SupportProjectionRead AnchorRead,
    SupportProjectionRead IncomingRead,
    SupportProjectionRead ProvisionalRead,
    SupportProjectionRead PublishedRead,
    SupportProjectionRead FeedbackRead,
    SupportProjectionRead SettledRead,
    SupportProjectionRead RemainderRead,
    Site Next)
{
    public SiteSettlement(
        Site source,
        ISupportProjection anchor,
        ISupportProjection incoming,
        ISupportProjection provisional,
        ISupportProjection published,
        ISupportProjection feedback,
        ISupportProjection settled,
        ISupportProjection remainder,
        Site next)
        : this(
            source,
            anchor.ToSupportRead(),
            incoming.ToSupportRead(),
            provisional.ToSupportRead(),
            published.ToSupportRead(),
            feedback.ToSupportRead(),
            settled.ToSupportRead(),
            remainder.ToSupportRead(),
            next)
    {
    }

    public enum SettlementPhase : int
    {
        None = 0,
        Anchor = 1,
        Incoming = 2,
        Provisional = 3,
        Published = 4,
        Feedback = 5,
        Settled = 6,
        Remainder = 7
    }

    // TODO(core): Delete this compatibility alias once callers consume AnchorRead directly.
    public Ratio Anchor => AnchorRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume IncomingRead directly.
    public Ratio Incoming => IncomingRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume ProvisionalRead directly.
    public Ratio Provisional => ProvisionalRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume PublishedRead directly.
    public Ratio Published => PublishedRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume FeedbackRead directly.
    public Ratio Feedback => FeedbackRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume SettledRead directly.
    public Ratio Settled => SettledRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume RemainderRead directly.
    public Ratio Remainder => RemainderRead.CompatibilityRatio();

    /// <summary>
    /// Enum-backed positive unit used to inspect settlement ordering through ordinary Core4 positions and relations.
    /// </summary>
    public EnumerableUnit<SettlementPhase> Phases { get; } = new(new Carrier(-4107), nameof(SettlementPhase));

    public ImmutableArray<ISupportProjection> ValueReads =>
    [
        Ratio.Zero(Measure.Zero),
        AnchorRead,
        IncomingRead,
        ProvisionalRead,
        PublishedRead,
        FeedbackRead,
        SettledRead,
        RemainderRead
    ];

    // TODO(core): Delete this compatibility alias once callers consume ValueReads directly.
    public ImmutableArray<Ratio> Values => [.. ValueReads.Select(value => value.CompatibilityRatio())];

    public ISupportProjection CurrentRead => AtRead(Phases.CurrentIndex);

    // TODO(core): Delete this compatibility alias once callers consume CurrentRead directly.
    public Ratio Current => CurrentRead.CompatibilityRatio();

    public ISupportProjection AtRead(SettlementPhase phase) => AtRead(Phases.OrderOf(phase));

    // TODO(core): Delete this compatibility alias once callers consume AtRead directly.
    public Ratio At(SettlementPhase phase) => AtRead(phase).CompatibilityRatio();

    public ISupportProjection AtRead(Measure index)
    {
        var values = ValueReads;

        if (index.Raw >= 0L && index.Raw < values.Length)
        {
            return values[(int)index.Raw].ToSupportRead();
        }

        return OverflowRead(index);
    }

    // TODO(core): Delete this compatibility alias once callers consume AtRead directly.
    public Ratio At(Measure index) => AtRead(index).CompatibilityRatio();

    private static ISupportProjection OverflowRead(Measure index) => index.Raw < 0L
        ? Ratio.Tension(index.Absolute)
        : Ratio.Tension(new Measure(index.Raw - (long)SettlementPhase.Remainder));
}
