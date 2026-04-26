namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks)
{
    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);
}
