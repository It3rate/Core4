namespace Gojo.Core;

public readonly record struct Relation(IElement Left, IElement Right) : IElement
{
    public Relation Dual() => IsPoint(out var location, out var carrier)
        ? new(location.Dual(), carrier)
        : new(Left.Dual(), Right.Dual());

    public Relation Reverse() => new(Right, Left);

    IElement IElement.Dual() => Dual();

    internal bool Apply()
    {
        var left = CarrierSitesOf(Left);
        var right = CarrierSitesOf(Right);
        if (!TryReadOneCarrier(left, out var leftSites) ||
            !TryReadOneCarrier(right, out var rightSites))
        {
            return false;
        }

        if (leftSites.Carrier.SameIdentityAs(rightSites.Carrier))
        {
            return AccountLocal(leftSites.Carrier, leftSites.Max, rightSites.Min);
        }

        if (leftSites.Carrier.IsInverse != rightSites.Carrier.IsInverse &&
            rightSites.IsOrigin)
        {
            return leftSites.Carrier.BalanceOpposedValueWith(rightSites.Carrier) |
                RouteNegativeTension(leftSites, rightSites);
        }

        return rightSites.IsOrigin && RoutePositiveTension(leftSites, rightSites);
    }

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

        if (!TryReadInCarrier(reader, leftLocation, rightLocation, out var leftTicks, out var rightTicks))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(zero, zero);
        }

        var start = Math.Min(leftTicks, rightTicks);
        var end = Math.Max(leftTicks, rightTicks);
        return new(new CarrierLocation(reader, start), new CarrierLocation(reader, end));
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

        if (!TryReadInCarrier(reader, leftLocation, rightLocation, out var leftTicks, out var rightTicks))
        {
            return new CarrierLocation(reader, 0);
        }

        return new CarrierLocation(reader, rightTicks - leftTicks);
    }

    public Relation TraversalSitesOn(Carrier reader)
    {
        if (!TryGetCarrierLocations(out var leftLocation, out var rightLocation))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        if (!TryReadInCarrier(reader, leftLocation, rightLocation, out var leftTicks, out var rightTicks))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        var flow = FlowOn(reader);
        if (flow.Ticks == 0)
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        var zeroFlow = new CarrierLocation(flow.Carrier, 0);
        return new(
            new Relation(new CarrierLocation(flow.Carrier, leftTicks), zeroFlow),
            new Relation(new CarrierLocation(flow.Carrier, rightTicks), flow));
    }

    public CarrierLocation[] CarrierLocations()
    {
        return [.. LeftCarrierLocations(), .. RightCarrierLocations()];
    }

    public Carrier[] Carriers() => [.. CarriersOf(Left), .. CarriersOf(Right)];

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

    private bool IsPoint(out CarrierLocation location, out Carrier carrier)
    {
        if (Left is CarrierLocation leftLocation &&
            Right is Carrier rightCarrier &&
            leftLocation.Carrier.SameIdentityAs(rightCarrier))
        {
            location = leftLocation;
            carrier = rightCarrier;
            return true;
        }

        location = default;
        carrier = null!;
        return false;
    }

    private static bool TryReadInCarrier(
        Carrier reader,
        CarrierLocation left,
        CarrierLocation right,
        out long leftTicks,
        out long rightTicks)
    {
        if (!left.Carrier.SameIdentityAs(reader) || !right.Carrier.SameIdentityAs(reader))
        {
            leftTicks = 0;
            rightTicks = 0;
            return false;
        }

        leftTicks = left.Ticks;
        rightTicks = right.Ticks;
        return true;
    }

    private static CarrierLocation[] CarrierLocationsOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => relation.CarrierLocations(),
        _ => []
    };

    private static Carrier[] CarriersOf(IElement element) => element switch
    {
        Carrier carrier => [carrier],
        CarrierLocation location => [location.Carrier],
        Relation relation => relation.Carriers(),
        _ => []
    };

    private static Relation SlotPair(Carrier reader, ulong reverse, ulong forward) =>
        new(
            new CarrierLocation(reader, -checked((long)reverse)),
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

    private static bool AccountLocal(
        Carrier carrier,
        long source,
        long target)
    {
        if (source <= 0 || target < 0 || source < target)
        {
            return false;
        }

        var sourceTicks = checked((ulong)source);
        var targetTicks = checked((ulong)target);
        if (carrier.Value <= targetTicks)
        {
            return false;
        }

        var local = sourceTicks == targetTicks
            ? Math.Min(carrier.Value, sourceTicks)
            : checked(targetTicks + ((carrier.Value - targetTicks) % (sourceTicks - targetTicks)));
        return local != carrier.Value &&
            carrier.LeaveTension(local, checked((long)(carrier.Value - local)));
    }

    private static bool RoutePositiveTension(CarrierSites source, CarrierSites receiver)
    {
        var unit = source.Width == 0 ? source.Max : source.Width;
        if (unit <= 0)
        {
            return false;
        }

        var target = source.Width == 0 ? 0 : source.Min;
        var changed = AccountLocal(source.Carrier, source.Max, target);
        if (source.Carrier.Tension <= 0)
        {
            return changed;
        }

        var tension = checked((ulong)source.Carrier.Tension);
        var unitTicks = checked((ulong)unit);
        var received = tension / unitTicks;
        if (received == 0)
        {
            return changed;
        }

        changed |= source.Carrier.KeepTensionRemainder(checked((long)(tension % unitTicks)));
        receiver.Carrier.Receive(checked((long)received));
        return true;
    }

    private static bool RouteNegativeTension(CarrierSites source, CarrierSites receiver)
    {
        var unit = source.Width == 0 ? source.Max : source.Width;
        if (unit <= 0)
        {
            return false;
        }

        if (source.Carrier.Tension >= 0)
        {
            return false;
        }

        var tension = checked((ulong)-source.Carrier.Tension);
        var unitTicks = checked((ulong)unit);
        var received = tension / unitTicks;
        if (received == 0)
        {
            return false;
        }

        var changed = source.Carrier.KeepTensionRemainder(-checked((long)(tension % unitTicks)));
        receiver.Carrier.Receive(checked((long)received));
        return true;
    }

    private static CarrierLocation[] CarrierSitesOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => [.. CarrierSitesOf(relation.Left), .. CarrierSitesOf(relation.Right)],
        _ => []
    };

    private static bool TryReadOneCarrier(
        CarrierLocation[] sites,
        out CarrierSites carrierSites)
    {
        if (sites.Length == 0)
        {
            carrierSites = default;
            return false;
        }

        var carrier = sites[0].Carrier;
        if (sites.Any(site => !site.Carrier.SameIdentityAs(carrier)))
        {
            carrierSites = default;
            return false;
        }

        carrierSites = new(
            carrier,
            sites.Min(site => site.Ticks),
            sites.Max(site => site.Ticks));
        return true;
    }

    private readonly record struct CarrierSites(Carrier Carrier, long Min, long Max)
    {
        public long Width => Max - Min;

        public bool IsOrigin => Width == 0 && Min == 0;
    }
}
