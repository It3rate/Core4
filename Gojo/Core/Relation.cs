namespace Gojo.Core;

public readonly record struct Relation(IElement Left, IElement Right) : IElement
{
    public static CarrierLocation[] BoundaryPointsOn(Carrier reader, params Relation[] relations)
    {
        return relations
            .Select(relation => relation.BoundariesOn(reader))
            .Where(boundary => boundary.LeftLocation.Ticks != boundary.RightLocation.Ticks)
            .SelectMany(boundary => new[] { boundary.LeftLocation, boundary.RightLocation })
            .Distinct()
            .OrderBy(location => location.Carrier.Lineage)
            .ThenBy(location => location.Carrier.Id)
            .ThenBy(location => location.Carrier.Flow)
            .ThenBy(location => location.Ticks)
            .ToArray();
    }

    public static Relation[] CoveredSpansOn(Carrier reader, params Relation[] relations)
    {
        var boundaries = relations
            .Select(relation => relation.BoundariesOn(reader))
            .Where(boundary => boundary.LeftLocation.Ticks != boundary.RightLocation.Ticks)
            .ToArray();

        if (boundaries.Length == 0)
        {
            return [];
        }

        var spans = new List<Relation>();
        foreach (var group in boundaries.GroupBy(boundary => boundary.LeftLocation.Carrier))
        {
            var groupedBoundaries = group.ToArray();
            var points = groupedBoundaries
                .SelectMany(boundary => new[] { boundary.LeftLocation, boundary.RightLocation })
                .Distinct()
                .OrderBy(location => location.Ticks)
                .ToArray();

            for (var i = 0; i < points.Length - 1; i++)
            {
                var start = points[i];
                var end = points[i + 1];
                if (IsCovered(start.Ticks, end.Ticks, groupedBoundaries))
                {
                    spans.Add(new(start, end));
                }
            }
        }

        return [.. spans];
    }

    public static Relation[] SupportStatesOn(Carrier reader, params Relation[] relations) =>
        CoveredSpansOn(reader, relations)
            .Select(span => new Relation(span, SupportStateOn(span, reader, relations)))
            .ToArray();

    public static Relation[] SupportOn(Carrier reader, params Relation[] relations) =>
        ProjectStatesOn(reader, state => state.SupportOn(reader), relations);

    public static Relation[] TensionOn(Carrier reader, params Relation[] relations) =>
        ProjectStatesOn(reader, state => state.TensionOn(reader), relations);

    public static Relation[] ExclusiveOn(Carrier reader, params Relation[] relations) =>
        ProjectStatesOn(reader, state => state.ExclusiveOn(reader), relations);

    public static Relation[] AndOn(Carrier reader, params Relation[] relations) =>
        TensionOn(reader, relations);

    public static Relation[] XorOn(Carrier reader, params Relation[] relations) =>
        ExclusiveOn(reader, relations);

    public static Relation[] OrOn(Carrier reader, params Relation[] relations) =>
        SupportOn(reader, relations);

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

    public CarrierLocation[] CarrierLocations()
    {
        var locations = new List<CarrierLocation>(2);
        if (Left is CarrierLocation left)
        {
            locations.Add(left);
        }

        if (Right is CarrierLocation right)
        {
            locations.Add(right);
        }

        return [.. locations];
    }

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

    private static bool IsCovered(ulong start, ulong end, Relation[] boundaries) =>
        boundaries.Any(boundary => boundary.LeftLocation.Ticks <= start && boundary.RightLocation.Ticks >= end);

    private static Relation[] ProjectStatesOn(
        Carrier reader,
        Func<Relation, Relation> projection,
        Relation[] relations) =>
        SupportStatesOn(reader, relations)
            .Select(state => new Relation(state.Left, projection(state)))
            .Where(state => state.Right is Relation projected && projected.HasStateParticipationOn(reader))
            .ToArray();

    private static Relation SupportStateOn(Relation span, Carrier reader, Relation[] relations)
    {
        var reverse = 0UL;
        var forward = 0UL;
        foreach (var relation in relations)
        {
            var boundaries = relation.BoundariesOn(reader);
            if (boundaries.LeftLocation.Carrier != span.LeftLocation.Carrier ||
                !IsCovered(span.LeftLocation.Ticks, span.RightLocation.Ticks, [boundaries]))
            {
                continue;
            }

            var flow = relation.FlowOn(reader).ReadFrom(reader);
            if (flow.Direction > 0)
            {
                forward++;
            }
            else if (flow.Direction < 0)
            {
                reverse++;
            }
        }

        return new(
            new CarrierLocation(reader.Inverse(), reverse),
            new CarrierLocation(reader, forward));
    }

    private static Relation SlotPair(Carrier reader, ulong reverse, ulong forward) =>
        new(
            new CarrierLocation(reader.Inverse(), reverse),
            new CarrierLocation(reader, forward));

    private bool HasStateParticipationOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        return reverse != 0 || forward != 0;
    }

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
