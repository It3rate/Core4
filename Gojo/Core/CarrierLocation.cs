namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks)
{
    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);

    public CarrierLocation NewTensionAfter(CarrierLocation accounted, Carrier reader)
    {
        var current = ReadFrom(reader);
        if (!current.IsVisible)
        {
            return new(Carrier, 0);
        }

        var covered = accounted.ReadFrom(reader);
        if (!covered.IsVisible || covered.Direction != current.Direction)
        {
            return this;
        }

        return new(Carrier, Ticks > covered.Magnitude ? Ticks - covered.Magnitude : 0);
    }
}
