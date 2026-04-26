namespace Gojo.Core;

public readonly record struct CarrierRead(
    Carrier Reader,
    Carrier LocationCarrier,
    ulong Magnitude,
    long Direction)
{
    public static CarrierRead From(CarrierLocation location, Carrier reader) =>
        new(reader, location.Carrier, location.Ticks, reader.ReadSignOf(location.Carrier));

    public bool IsVisible => Direction != 0 && Magnitude != 0;

    public long ProjectedTicks => Direction == 0 ? 0 : checked((long)Magnitude) * Math.Sign(Direction);
}
