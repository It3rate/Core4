namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks) : IElement
{
    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);
}
