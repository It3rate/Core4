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

    public Relation AccountedBy(Carrier reader, Relation accounted) =>
        AccountedBy(reader, accounted.Left, accounted.Right);

    public Relation AccountedBy(Carrier reader, Relation firstAccounted, params Relation[] remainingAccounted) =>
        AccountedBy(reader, Flatten(firstAccounted, remainingAccounted));

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

    public CarrierLocation NewTensionAfter(Carrier reader, Relation accounted) =>
        NewTensionAfter(reader, accounted.Left, accounted.Right);

    public CarrierLocation NewTensionAfter(Carrier reader, Relation firstAccounted, params Relation[] remainingAccounted) =>
        NewTensionAfter(reader, Flatten(firstAccounted, remainingAccounted));

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

    private static CarrierLocation[] Flatten(Relation first, Relation[] rest)
    {
        var locations = new CarrierLocation[2 + rest.Length * 2];
        locations[0] = first.Left;
        locations[1] = first.Right;

        var index = 2;
        foreach (var relation in rest)
        {
            locations[index++] = relation.Left;
            locations[index++] = relation.Right;
        }

        return locations;
    }
}
