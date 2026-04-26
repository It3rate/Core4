namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks)
{
    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);

    public Relation ReadThroughBoundary(Carrier earlierCarrier, CarrierLocation boundary)
    {
        ArgumentOutOfRangeException.ThrowIfZero(boundary.Ticks);
        return new(
            new CarrierLocation(earlierCarrier, Ticks / boundary.Ticks),
            new CarrierLocation(Carrier, Ticks % boundary.Ticks));
    }

    public CarrierLocation NewTensionAfter(CarrierLocation accounted, Carrier reader) =>
        NewTensionAfter(reader, accounted);

    public Relation AccountedBy(Carrier reader, params CarrierLocation[] accounted)
    {
        if (!ReadFrom(reader).IsVisible)
        {
            var zero = new CarrierLocation(Carrier, 0);
            return new(zero, zero);
        }

        var tension = NewTensionAfter(reader, accounted);
        return new(new CarrierLocation(Carrier, Ticks - tension.Ticks), tension);
    }

    public CarrierLocation NewTensionAfter(Carrier reader, params CarrierLocation[] accounted)
    {
        var current = ReadFrom(reader);
        if (!current.IsVisible)
        {
            return new(Carrier, 0);
        }

        var coveredTicks = 0UL;
        foreach (var account in accounted)
        {
            var covered = account.ReadFrom(reader);
            if (!covered.IsVisible || covered.Direction != current.Direction)
            {
                continue;
            }

            var remaining = Ticks - coveredTicks;
            if (covered.Magnitude >= remaining)
            {
                return new(Carrier, 0);
            }

            coveredTicks += covered.Magnitude;
        }

        return new(Carrier, Ticks - coveredTicks);
    }
}
