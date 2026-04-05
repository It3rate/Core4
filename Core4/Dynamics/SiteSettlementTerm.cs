using Core4.Numerics;

namespace Core4.Dynamics;

/// <summary>
/// One explicitly ordered settlement term, exposed so runtime code can iterate local phases without knowing the
/// internal field names.
/// Example: <c>var first = step.Terms[0];</c>
/// </summary>
public readonly record struct SiteSettlementTerm(SettlementPhase Phase, Measure Order, Ratio Value);
