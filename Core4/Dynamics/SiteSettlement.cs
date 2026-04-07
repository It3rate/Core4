using System.Collections.Immutable;
using Core4.Aliases;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Dynamics;

/// <summary>
/// One local settlement pass. TODO: promote this into a runtime-iterable trace once the solver layer lands.
/// It exposes the anchor, incoming evidence, publication, feedback, and carried remainder before producing the next
/// site state, and it also yields explicitly numbered phases for inspection.
/// Example: <c>var step = site.Settle(); var next = step.Next;</c>
/// </summary>
public readonly record struct SiteSettlement(
    Site Source,
    Ratio Anchor,
    Ratio Incoming,
    Ratio Provisional,
    Ratio Published,
    Ratio Feedback,
    Ratio Settled,
    Ratio Remainder,
    Site Next)
{
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

    /// <summary>
    /// Enum-backed positive unit used to inspect settlement ordering through ordinary Core4 positions and relations.
    /// TODO: Replace the fixed conceptual carrier id with a broader Core4-native registry for named enumerable units.
    /// </summary>
    public EnumerableUnit<SettlementPhase> Phases { get; } = new(new Carrier(-4107), nameof(SettlementPhase));

    public ImmutableArray<Ratio> Values =>
    [
        Ratio.Zero(Measure.Zero),
        Anchor,
        Incoming,
        Provisional,
        Published,
        Feedback,
        Settled,
        Remainder
    ];

    public Ratio Current => At(Phases.CurrentIndex);

    public Ratio At(SettlementPhase phase) => At(Phases.OrderOf(phase));

    public Ratio At(Measure index)
    {
        var values = Values;

        if (index.Raw >= 0L && index.Raw < values.Length)
        {
            return values[(int)index.Raw].Normalize();
        }

        return Overflow(index);
    }

    private static Ratio Overflow(Measure index) => index.Raw < 0L
        ? Ratio.Tension(index.Absolute)
        : Ratio.Tension(new Measure(index.Raw - (long)SettlementPhase.Remainder));
}
