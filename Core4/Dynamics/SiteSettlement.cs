using System.Collections.Immutable;
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
    public ImmutableArray<SiteSettlementTerm> Terms =>
    [
        new(SettlementPhase.Anchor, 1, Anchor),
        new(SettlementPhase.Incoming, 2, Incoming),
        new(SettlementPhase.Provisional, 3, Provisional),
        new(SettlementPhase.Published, 4, Published),
        new(SettlementPhase.Feedback, 5, Feedback),
        new(SettlementPhase.Settled, 6, Settled),
        new(SettlementPhase.Remainder, 7, Remainder)
    ];

    public Ratio Select(SettlementPhase phase)
    {
        var hasAny = false;
        var total = Ratio.Zero(Measure.Zero);

        foreach (var term in Terms)
        {
            if ((term.Phase & phase) == 0)
            {
                continue;
            }

            total = hasAny ? total.Add(term.Value) : term.Value.Normalize();
            hasAny = true;
        }

        return hasAny ? total.Normalize() : Ratio.Zero(Measure.Zero);
    }
}
