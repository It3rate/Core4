using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

/// <summary>
/// One named orthogonal channel used by the numeric explorer presets.
/// </summary>
public readonly record struct NumericExplorerChannel(
    string Name,
    Carrier Carrier,
    Ratio Value,
    Measure Tension,
    Provenance Provenance,
    int SlotIndex = 0,
    int SlotCount = 1)
{
    public Flow ToFlow() => new(Carrier, Value.Normalize(), Provenance);
}
