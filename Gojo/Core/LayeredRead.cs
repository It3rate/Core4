namespace Gojo.Core;

public readonly record struct LayeredRead(
    CarrierLocation Earlier,
    CarrierLocation Later,
    CarrierLocation Boundary)
{
    public static LayeredRead FromBoundary(
        Carrier earlierCarrier,
        Carrier laterCarrier,
        ulong ticks,
        CarrierLocation boundary)
    {
        ArgumentOutOfRangeException.ThrowIfZero(boundary.Ticks);
        return new(
            new CarrierLocation(earlierCarrier, ticks / boundary.Ticks),
            new CarrierLocation(laterCarrier, ticks % boundary.Ticks),
            boundary);
    }

    public bool HasLaterLayerTicks => Later.Ticks != 0;

    public override string ToString() =>
        HasLaterLayerTicks
            ? $"{Earlier.Ticks} {Later.Ticks}|{Boundary.Ticks}"
            : Earlier.Ticks.ToString();
}
