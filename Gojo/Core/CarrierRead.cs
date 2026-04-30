namespace Gojo.Core;

public readonly record struct CarrierRead(
    Carrier Reader,
    Carrier LocationCarrier,
    ulong Magnitude,
    long Direction)
{
    public static CarrierRead From(CarrierLocation location, Carrier reader) =>
        new(
            reader,
            location.Carrier,
            MagnitudeOf(location.Ticks),
            reader.ReadSignOf(location.Carrier) * LocationSign(location.Ticks));

    public bool IsVisible => Direction != 0 && Magnitude != 0;

    public long ProjectedTicks => Direction == 0 ? 0 : checked((long)Magnitude) * Math.Sign(Direction);

    private static ulong MagnitudeOf(long ticks) =>
        ticks >= 0 ? checked((ulong)ticks) : checked((ulong)-ticks);

    private static long LocationSign(long ticks) => ticks < 0 ? -1 : 1;
}
