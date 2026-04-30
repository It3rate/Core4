namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, long Ticks) : IElement
{
    public CarrierLocation(Carrier carrier, ulong ticks)
        : this(carrier, checked((long)ticks))
    {
    }

    public CarrierLocation Dual() => new(Carrier, checked(-Ticks));

    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);

    IElement IElement.Dual() => Dual();
}
