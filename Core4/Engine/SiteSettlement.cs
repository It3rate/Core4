namespace Core4.Engine;

/// <summary>
/// One local settlement pass. It exposes the anchor, incoming evidence, publication, feedback, and carried
/// remainder before producing the next site state.
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
    Site Next);
