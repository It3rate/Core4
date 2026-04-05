namespace Core4.Dynamics;

/// <summary>
/// Explicit numbered phases for one local settlement pass. Values are flags so runtime code can inspect individual
/// phases or grouped views without relying on field names.
/// </summary>
[Flags]
public enum SettlementPhase : long
{
    Anchor = 1,
    Incoming = 2,
    Provisional = 4,
    Published = 8,
    Feedback = 16,
    Settled = 32,
    Remainder = 64
}
