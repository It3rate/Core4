namespace Gojo.Core;

public readonly record struct Relation(IElement Left, IElement Right) : IElement
{
    public Relation SupportOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        return SlotPair(reader, reverse, forward);
    }

    public Relation TensionOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        var tension = Math.Min(reverse, forward);
        return SlotPair(reader, tension, tension);
    }

    public Relation ExclusiveOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        return forward >= reverse
            ? SlotPair(reader, 0, forward - reverse)
            : SlotPair(reader, reverse - forward, 0);
    }

    public Relation BoundariesOn(Carrier reader)
    {
        if (!TryGetCarrierLocations(out var leftLocation, out var rightLocation))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(zero, zero);
        }

        var left = leftLocation.ReadFrom(reader);
        var right = rightLocation.ReadFrom(reader);
        if (left.Direction == 0 || right.Direction == 0 || left.Direction != right.Direction)
        {
            var zero = new CarrierLocation(reader, 0);
            return new(zero, zero);
        }

        var boundaryCarrier = left.Direction > 0 ? reader : reader.Inverse();
        var start = Math.Min(left.Magnitude, right.Magnitude);
        var end = Math.Max(left.Magnitude, right.Magnitude);
        return new(new CarrierLocation(boundaryCarrier, start), new CarrierLocation(boundaryCarrier, end));
    }

    public CarrierLocation SpanOn(Carrier reader)
    {
        var boundaries = BoundariesOn(reader);
        return new CarrierLocation(
            boundaries.LeftLocation.Carrier,
            boundaries.RightLocation.Ticks - boundaries.LeftLocation.Ticks);
    }

    public CarrierLocation FlowOn(Carrier reader)
    {
        if (!TryGetCarrierLocations(out var leftLocation, out var rightLocation))
        {
            return new CarrierLocation(reader, 0);
        }

        var left = leftLocation.ReadFrom(reader);
        var right = rightLocation.ReadFrom(reader);
        if (left.Direction == 0 || right.Direction == 0 || left.Direction != right.Direction)
        {
            return new CarrierLocation(reader, 0);
        }

        var delta = right.ProjectedTicks - left.ProjectedTicks;
        return delta >= 0
            ? new CarrierLocation(reader, checked((ulong)delta))
            : new CarrierLocation(reader.Inverse(), checked((ulong)-delta));
    }

    public Relation TraversalSitesOn(Carrier reader)
    {
        if (!TryGetCarrierLocations(out var leftLocation, out var rightLocation))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        var left = leftLocation.ReadFrom(reader);
        var right = rightLocation.ReadFrom(reader);
        var flow = FlowOn(reader);
        if (left.Direction == 0 || right.Direction == 0 || left.Direction != right.Direction || flow.Ticks == 0)
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        var zeroFlow = new CarrierLocation(flow.Carrier, 0);
        return new(
            new Relation(new CarrierLocation(flow.Carrier, left.Magnitude), zeroFlow),
            new Relation(new CarrierLocation(flow.Carrier, right.Magnitude), flow));
    }

    public CarrierLocation[] CarrierLocations()
    {
        return [.. LeftCarrierLocations(), .. RightCarrierLocations()];
    }

    public CarrierLocation[] LeftCarrierLocations() => CarrierLocationsOf(Left);

    public CarrierLocation[] RightCarrierLocations() => CarrierLocationsOf(Right);

    public bool TryGetCarrierLocations(out CarrierLocation left, out CarrierLocation right)
    {
        if (Left is CarrierLocation leftLocation && Right is CarrierLocation rightLocation)
        {
            left = leftLocation;
            right = rightLocation;
            return true;
        }

        left = default;
        right = default;
        return false;
    }

    public CarrierLocation LeftLocation => Left is CarrierLocation location
        ? location
        : throw new InvalidOperationException("The left relation slot is not a carrier location.");

    public CarrierLocation RightLocation => Right is CarrierLocation location
        ? location
        : throw new InvalidOperationException("The right relation slot is not a carrier location.");

    private static CarrierLocation[] CarrierLocationsOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => relation.CarrierLocations(),
        _ => []
    };

    private static Relation SlotPair(Carrier reader, ulong reverse, ulong forward) =>
        new(
            new CarrierLocation(reader.Inverse(), reverse),
            new CarrierLocation(reader, forward));

    private (ulong Reverse, ulong Forward) ReadStatePair(Carrier reader)
    {
        if (TryGetCarrierLocations(out var left, out var right))
        {
            return ReadSlots(reader, left, right);
        }

        return Right is Relation state ? state.ReadStatePair(reader) : (0, 0);
    }

    private static (ulong Reverse, ulong Forward) ReadSlots(
        Carrier reader,
        CarrierLocation left,
        CarrierLocation right)
    {
        var reverse = 0UL;
        var forward = 0UL;
        AddSlot(left);
        AddSlot(right);
        return (reverse, forward);

        void AddSlot(CarrierLocation slot)
        {
            var read = slot.ReadFrom(reader);
            if (read.Direction > 0)
            {
                forward += read.Magnitude;
            }
            else if (read.Direction < 0)
            {
                reverse += read.Magnitude;
            }
        }
    }
}
