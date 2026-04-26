namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks) : IElement
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
        AccountedBy(reader, accounted.CarrierLocations());

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

    public Relation TransferThrough(Carrier reader, Relation route)
    {
        var sourceRead = AccountedBy(reader, route.LeftCarrierLocations());
        return new(sourceRead, Receive(route.Right, sourceRead.LeftLocation));
    }

    public Relation[] TransferThroughRoutes(Carrier reader, params Relation[] routes)
    {
        var remaining = this;
        var transfers = new List<Relation>();
        foreach (var route in routes)
        {
            if (!remaining.ReadFrom(reader).IsVisible)
            {
                break;
            }

            var transfer = remaining.TransferThrough(reader, route);
            transfers.Add(transfer);
            remaining = transfer.Left is Relation sourceRead
                ? sourceRead.RightLocation
                : new CarrierLocation(Carrier, 0);
        }

        return [.. transfers];
    }

    public Relation ObserveThrough(Carrier reader, Relation route)
    {
        var sourceRead = AccountedBy(reader);
        var observed = AccountedBy(reader, route.LeftCarrierLocations()).LeftLocation;
        return new(sourceRead, Receive(route.Right, observed));
    }

    public Relation GatherThrough(Carrier reader, Relation route)
    {
        var sourceRead = AccountedBy(reader);
        return new(sourceRead, new Relation(Receive(route.Left, sourceRead.RightLocation), route.Right));
    }

    public CarrierLocation NewTensionAfter(Carrier reader, Relation accounted) =>
        NewTensionAfter(reader, accounted.CarrierLocations());

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

    private static IElement Receive(IElement target, CarrierLocation accounted) => target switch
    {
        CarrierLocation location => new CarrierLocation(location.Carrier, checked(location.Ticks + accounted.Ticks)),
        Relation relation => new Relation(Receive(relation.Left, accounted), Receive(relation.Right, accounted)),
        _ => target
    };

    private static CarrierLocation[] Flatten(Relation first, Relation[] rest)
    {
        var locations = new List<CarrierLocation>(first.CarrierLocations());
        foreach (var relation in rest)
        {
            locations.AddRange(relation.CarrierLocations());
        }

        return [.. locations];
    }
}
