namespace Gojo.Core;

public readonly record struct LayeredRead(
    CarrierLocation Earlier,
    CarrierLocation Later,
    LayerTransform Boundary)
{
    public static LayeredRead FromBoundary(
        Carrier earlierCarrier,
        Carrier laterCarrier,
        ulong ticks,
        LayerTransform boundary)
    {
        ArgumentOutOfRangeException.ThrowIfZero(boundary.Magnitude);
        return new(
            new CarrierLocation(earlierCarrier, ticks / boundary.Magnitude),
            new CarrierLocation(laterCarrier, ticks % boundary.Magnitude),
            boundary);
    }

    public bool HasLaterLayerTicks => Later.Ticks != 0;

    public override string ToString() =>
        HasLaterLayerTicks
            ? $"{Earlier.Ticks} {Later.Ticks}|{Boundary.Magnitude}"
            : Earlier.Ticks.ToString();
}
