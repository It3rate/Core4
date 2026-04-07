namespace Core4.Dynamics;

/// <summary>
/// Explicit numbered phases for one local settlement pass. Values are flags so runtime code can inspect individual
/// phases or grouped views without relying on field names.
/// TODO: Replace this fixed enum with a Core4-native numeric phase representation once settlement topology is encoded
/// directly in the core.
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
