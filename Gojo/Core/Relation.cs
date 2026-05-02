namespace Gojo.Core;

public sealed class Relation : IElement, IEquatable<Relation>
{
    private static ulong idCounter = 1;

    public Relation(IElement left, IElement right)
    {
        Id = NextId();
        Left = left;
        Right = right;
    }

    public ulong Id { get; }

    public IElement Left { get; private set; }

    public IElement Right { get; private set; }

    public Relation Dual() => IsPoint(out var location, out var carrier)
        ? new(location.Dual(), carrier)
        : new(Left.Dual(), Right.Dual());

    public Relation Reverse() => new(Right, Left);

    IElement IElement.Dual() => Dual();

    public bool SameIdentityAs(Relation other) => ReferenceEquals(this, other);

    public bool Perturb(IElement perturbation)
    {
        if (perturbation is CarrierLocation delta)
        {
            return TryTransformLocations(location =>
                location.Carrier.SameIdentityAs(delta.Carrier)
                    ? LocationTransform.To(new CarrierLocation(location.Carrier, checked(location.Ticks + delta.Ticks)))
                    : LocationTransform.To(location));
        }

        if (perturbation is Relation relation &&
            TryReadBasis(relation.Left, out var source) &&
            TryReadBasis(relation.Right, out var target) &&
            source.Ticks != 0 &&
            target.Ticks != 0)
        {
            return TryTransformLocations(location =>
                TryScale(location, source.Ticks, target.Ticks, out var scaled)
                    ? LocationTransform.To(scaled)
                    : LocationTransform.Blocked);
        }

        return false;
    }

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

    public Relation Project(CarrierLocation location, Carrier reader)
    {
        return TryProject(location, reader, out var projected)
            ? projected
            : ProjectionResult(new CarrierLocation(reader, 0), location);
    }

    public bool TryProject(CarrierLocation location, Carrier reader, out Relation projected)
    {
        if (!TryReadFrame(Left, out var leftFrame) ||
            !TryReadFrame(Right, out var rightFrame))
        {
            projected = ProjectionResult(new CarrierLocation(reader, 0), location);
            return false;
        }

        if (FrameAccepts(leftFrame, location.Carrier) &&
            FrameAccepts(rightFrame, reader))
        {
            projected = ProjectBetween(
                location,
                reader,
                ResolveFrameCarrier(leftFrame, location.Carrier),
                ResolveFrameCarrier(rightFrame, reader));
            return true;
        }

        if (FrameAccepts(rightFrame, location.Carrier) &&
            FrameAccepts(leftFrame, reader))
        {
            projected = ProjectBetween(
                location,
                reader,
                ResolveFrameCarrier(rightFrame, location.Carrier),
                ResolveFrameCarrier(leftFrame, reader));
            return true;
        }

        projected = ProjectionResult(new CarrierLocation(reader, 0), location);
        return false;
    }

    public bool TrySetFrom(IElement input)
    {
        var imports = new List<SlotImport>();
        if (!TryCollectSlotImports(Left, Right, input, imports))
        {
            return false;
        }

        foreach (var import in imports)
        {
            import.Apply();
        }

        return imports.Count != 0;
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

    public bool Equals(Relation? other) =>
        other is not null &&
        Equals(Left, other.Left) &&
        Equals(Right, other.Right);

    public override bool Equals(object? obj) => obj is Relation other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Left, Right);

    public override string ToString() => $"{Left} -> {Right}";

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

    private static bool TryReadBasis(IElement element, out CarrierLocation basis)
    {
        if (TryReadFrame(element, out var frame))
        {
            basis = new CarrierLocation(frame.Carrier, frame.Extent);
            return true;
        }

        basis = default;
        return false;
    }

    private static bool TryReadFrame(IElement element, out CarrierFrame frame)
    {
        if (element is CarrierLocation location)
        {
            frame = new CarrierFrame(location.Carrier, 0, location.Ticks);
            return true;
        }

        if (element is not Relation relation)
        {
            frame = default;
            return false;
        }

        if (relation.IsPoint(out var pointLocation, out _))
        {
            frame = new CarrierFrame(pointLocation.Carrier, 0, pointLocation.Ticks);
            return true;
        }

        if (relation.TryGetCarrierLocations(out var left, out var right) &&
            left.Carrier.SameIdentityAs(right.Carrier))
        {
            frame = new CarrierFrame(left.Carrier, left.Ticks, right.Ticks);
            return true;
        }

        if (relation.Left is Relation leftPoint &&
            relation.Right is Relation rightPoint &&
            leftPoint.IsPoint(out var leftPointLocation, out _) &&
            rightPoint.IsPoint(out var rightPointLocation, out _) &&
            leftPointLocation.Carrier.SameIdentityAs(rightPointLocation.Carrier))
        {
            frame = new CarrierFrame(
                leftPointLocation.Carrier,
                leftPointLocation.Ticks,
                rightPointLocation.Ticks);
            return true;
        }

        frame = default;
        return false;
    }

    private static bool TryCollectSlotImports(
        IElement sourceSurface,
        IElement targetSurface,
        IElement input,
        List<SlotImport> imports)
    {
        if (input is Relation inputGroup &&
            sourceSurface is Relation sourceGroup &&
            targetSurface is Relation targetGroup)
        {
            return TryCollectSlotImports(sourceGroup.Left, targetGroup.Left, inputGroup.Left, imports) &&
                TryCollectSlotImports(sourceGroup.Right, targetGroup.Right, inputGroup.Right, imports);
        }

        if (input is not CarrierLocation inputLocation ||
            !TryReadFrame(sourceSurface, out var sourceFrame) ||
            !TryReadFrame(targetSurface, out var targetFrame) ||
            targetFrame.Carrier.IsUnknown ||
            !FrameAccepts(sourceFrame, inputLocation.Carrier))
        {
            return false;
        }

        if (!new Relation(sourceSurface, targetSurface).TryProject(inputLocation, targetFrame.Carrier, out var projected) ||
            !projected.TryGetCarrierLocations(out var target, out var residue) ||
            !target.Carrier.SameIdentityAs(targetFrame.Carrier))
        {
            return false;
        }

        imports.Add(new SlotImport(target.Carrier, target.Ticks, residue));
        return true;
    }

    private static bool FrameAccepts(CarrierFrame frame, Carrier carrier) =>
        frame.Carrier.IsUnknown || carrier.SameIdentityAs(frame.Carrier);

    private static CarrierFrame ResolveFrameCarrier(CarrierFrame frame, Carrier carrier) =>
        frame.Carrier.IsUnknown
            ? new CarrierFrame(carrier, frame.Start, frame.End)
            : frame;

    private bool TryTransformLocations(Func<CarrierLocation, LocationTransform> transform)
    {
        var updates = new List<LocationUpdate>();
        if (!TryCollectLocationUpdates(transform, new HashSet<ulong>(), updates))
        {
            return false;
        }

        foreach (var update in updates)
        {
            if (update.Current.Carrier.SameIdentityAs(update.Next.Carrier))
            {
                update.Current.Tick.Set(update.Next.Ticks);
                continue;
            }

            if (update.IsLeft)
            {
                update.Relation.Left = update.Next;
            }
            else
            {
                update.Relation.Right = update.Next;
            }
        }

        return updates.Count != 0;
    }

    private bool TryCollectLocationUpdates(
        Func<CarrierLocation, LocationTransform> transform,
        HashSet<ulong> visited,
        List<LocationUpdate> updates)
    {
        if (!visited.Add(Id))
        {
            return true;
        }

        return TryCollect(Left, isLeft: true) &&
            TryCollect(Right, isLeft: false);

        bool TryCollect(IElement element, bool isLeft)
        {
            if (element is CarrierLocation location)
            {
                var transformed = transform(location);
                if (!transformed.CanTransform)
                {
                    return false;
                }

                if (!transformed.Location.Equals(location))
                {
                    updates.Add(new LocationUpdate(this, isLeft, location, transformed.Location));
                }

                return true;
            }

            return element is not Relation relation ||
                relation.TryCollectLocationUpdates(transform, visited, updates);
        }
    }

    private static Relation ProjectBetween(
        CarrierLocation location,
        Carrier reader,
        CarrierFrame source,
        CarrierFrame target)
    {
        var sourceBasis = source.Extent;
        var targetBasis = target.Extent;
        if (sourceBasis == 0 || targetBasis == 0)
        {
            return ProjectionResult(new CarrierLocation(reader, 0), location);
        }

        var relativeTicks = checked(location.Ticks - source.Start);
        if (relativeTicks == 0)
        {
            return ProjectionResult(new CarrierLocation(reader, target.Start), new CarrierLocation(location.Carrier, 0));
        }

        var sourceMagnitude = MagnitudeOf(sourceBasis);
        var targetMagnitude = MagnitudeOf(targetBasis);
        var locationMagnitude = MagnitudeOf(relativeTicks);
        var scaled = checked(locationMagnitude * targetMagnitude);
        var projectedMagnitude = scaled / sourceMagnitude;
        var residueMagnitude = scaled % sourceMagnitude;
        var sign = Math.Sign(relativeTicks) * Math.Sign(sourceBasis) * Math.Sign(targetBasis);

        return ProjectionResult(
            new CarrierLocation(reader, checked(target.Start + SignedTicks(projectedMagnitude, sign))),
            new CarrierLocation(location.Carrier, SignedTicks(residueMagnitude, Math.Sign(relativeTicks))));
    }

    private static Relation ProjectionResult(CarrierLocation projected, CarrierLocation residue) =>
        new(projected, residue);

    private static ulong MagnitudeOf(long ticks) =>
        ticks >= 0 ? checked((ulong)ticks) : checked((ulong)-ticks);

    private static long SignedTicks(ulong ticks, int sign) =>
        sign < 0 ? -checked((long)ticks) : checked((long)ticks);

    private static bool TryScale(
        CarrierLocation location,
        long sourceBasis,
        long targetBasis,
        out CarrierLocation scaled)
    {
        var sourceMagnitude = MagnitudeOf(sourceBasis);
        var targetMagnitude = MagnitudeOf(targetBasis);
        var locationMagnitude = MagnitudeOf(location.Ticks);
        var scaledMagnitude = checked(locationMagnitude * targetMagnitude);
        if (scaledMagnitude % sourceMagnitude != 0)
        {
            scaled = default;
            return false;
        }

        var sign = Math.Sign(location.Ticks) * Math.Sign(sourceBasis) * Math.Sign(targetBasis);
        scaled = new CarrierLocation(location.Carrier, SignedTicks(scaledMagnitude / sourceMagnitude, sign));
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

    private readonly record struct CarrierFrame(Carrier Carrier, long Start, long End)
    {
        public long Extent => End - Start;
    }

    private readonly record struct SlotImport(Carrier Target, long TargetTicks, CarrierLocation Residue)
    {
        public void Apply()
        {
            Target.Set(TargetTicks);
            Residue.Carrier.HoldResidueTension(Residue.Ticks);
        }
    }

    private readonly record struct LocationUpdate(
        Relation Relation,
        bool IsLeft,
        CarrierLocation Current,
        CarrierLocation Next);

    private readonly record struct LocationTransform(bool CanTransform, CarrierLocation Location)
    {
        public static LocationTransform Blocked => new(false, default);

        public static LocationTransform To(CarrierLocation location) => new(true, location);
    }

    private static ulong NextId() => idCounter++;
}
