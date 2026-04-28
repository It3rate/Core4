using Gojo.Core;

namespace Gojo.Reads;

public static class CarrierLocationReads
{
    public static Relation ReadThroughBoundary(
        this CarrierLocation location,
        Carrier earlierCarrier,
        CarrierLocation boundary)
    {
        ArgumentOutOfRangeException.ThrowIfZero(boundary.Ticks);
        return new(
            new CarrierLocation(earlierCarrier, location.Ticks / boundary.Ticks),
            new CarrierLocation(location.Carrier, location.Ticks % boundary.Ticks));
    }

    public static Relation ReadRefillThroughBoundary(
        this CarrierLocation location,
        Carrier earlierCarrier,
        CarrierLocation boundary)
    {
        ArgumentOutOfRangeException.ThrowIfZero(boundary.Ticks);
        if (location.Ticks == 0)
        {
            return new(
                new CarrierLocation(earlierCarrier, 0),
                new CarrierLocation(location.Carrier.Inverse(), 0));
        }

        return new(
            new CarrierLocation(earlierCarrier, ((location.Ticks - 1) / boundary.Ticks) + 1),
            new CarrierLocation(location.Carrier.Inverse(), (boundary.Ticks - (location.Ticks % boundary.Ticks)) % boundary.Ticks));
    }

    public static Relation ReadThroughSegment(
        this CarrierLocation location,
        Carrier reader,
        Relation segment,
        Carrier lapCarrier)
    {
        var boundaries = segment.BoundariesOn(reader);
        var span = boundaries.RightLocation.Ticks - boundaries.LeftLocation.Ticks;
        if (span == 0)
        {
            var zero = new CarrierLocation(lapCarrier, 0);
            return new(zero, zero);
        }

        var read = location.ReadFrom(boundaries.LeftLocation.Carrier);
        if (read.Direction <= 0 || read.Magnitude < boundaries.LeftLocation.Ticks)
        {
            var zero = new CarrierLocation(lapCarrier, 0);
            return new(zero, zero);
        }

        var offset = read.Magnitude - boundaries.LeftLocation.Ticks;
        return new(
            new CarrierLocation(lapCarrier, offset / span),
            new CarrierLocation(boundaries.LeftLocation.Carrier, boundaries.LeftLocation.Ticks + (offset % span)));
    }

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        CarrierLocation accounted,
        Carrier reader) =>
        location.NewTensionAfter(reader, accounted);

    public static Relation AccountedBy(
        this CarrierLocation location,
        Carrier reader,
        Relation accounted) =>
        location.AccountedBy(reader, accounted.CarrierLocations());

    public static Relation AccountedBy(
        this CarrierLocation location,
        Carrier reader,
        Relation firstAccounted,
        params Relation[] remainingAccounted) =>
        location.AccountedBy(reader, Flatten(firstAccounted, remainingAccounted));

    public static Relation AccountedBy(
        this CarrierLocation location,
        Carrier reader,
        params CarrierLocation[] accounted)
    {
        if (!location.ReadFrom(reader).IsVisible)
        {
            var zero = new CarrierLocation(location.Carrier, 0);
            return new(zero, zero);
        }

        var tension = location.NewTensionAfter(reader, accounted);
        return new(new CarrierLocation(location.Carrier, location.Ticks - tension.Ticks), tension);
    }

    public static Relation TransferThrough(
        this CarrierLocation location,
        Carrier reader,
        Relation route)
    {
        return location.RouteThrough(
            reader,
            route.LeftCarrierLocations(),
            sourceRead => sourceRead.LeftLocation,
            emitted => Receive(route.Right, emitted));
    }

    public static Relation TransferThrough(
        this CarrierLocation location,
        Carrier reader,
        Relation route,
        Carrier receiverReader,
        Relation receiverCapacity)
    {
        var transfer = location.TransferThrough(reader, route);
        return transfer.Right is CarrierLocation received
            ? new Relation(transfer.Left, received.AccountedBy(receiverReader, receiverCapacity))
            : transfer;
    }

    public static Relation ObserveThrough(
        this CarrierLocation location,
        Carrier reader,
        Relation route)
    {
        var observed = location.AccountedBy(reader, route.LeftCarrierLocations()).LeftLocation;
        return location.RouteThrough(
            reader,
            [],
            _ => observed,
            emitted => Receive(route.Right, emitted));
    }

    public static Relation GatherThrough(
        this CarrierLocation location,
        Carrier reader,
        Relation route)
    {
        return location.RouteThrough(
            reader,
            [],
            sourceRead => sourceRead.RightLocation,
            residual => new Relation(Receive(route.Left, residual), route.Right));
    }

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        Carrier reader,
        Relation accounted) =>
        location.NewTensionAfter(reader, accounted.CarrierLocations());

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        Carrier reader,
        Relation firstAccounted,
        params Relation[] remainingAccounted) =>
        location.NewTensionAfter(reader, Flatten(firstAccounted, remainingAccounted));

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        Carrier reader,
        params CarrierLocation[] accounted)
    {
        var current = location.ReadFrom(reader);
        if (!current.IsVisible)
        {
            return new(location.Carrier, 0);
        }

        var coveredTicks = 0UL;
        foreach (var account in accounted)
        {
            var covered = account.ReadFrom(reader);
            if (!covered.IsVisible || covered.Direction != current.Direction)
            {
                continue;
            }

            var remaining = location.Ticks - coveredTicks;
            if (covered.Magnitude >= remaining)
            {
                return new(location.Carrier, 0);
            }

            coveredTicks += covered.Magnitude;
        }

        return new(location.Carrier, location.Ticks - coveredTicks);
    }

    private static Relation RouteThrough(
        this CarrierLocation location,
        Carrier reader,
        CarrierLocation[] sourceCapacity,
        Func<Relation, CarrierLocation> emit,
        Func<CarrierLocation, IElement> receive)
    {
        var sourceRead = location.AccountedBy(reader, sourceCapacity);
        return new(sourceRead, receive(emit(sourceRead)));
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
