namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks) : IElement
{
    public CarrierLocation Dual() => new(Carrier.Dual(), Ticks);

    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);

    IElement IElement.Dual() => Dual();
}
