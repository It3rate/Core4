using Gojo.Core;

namespace Gojo.Reads;

public static class StructuralTransformReads
{
    public static Relation AddAndSettle(this Relation structure, CarrierLocation incoming)
    {
        var positions = ReadPositionState(structure.Left);
        var relationSurface = structure.Right;
        var relations = RelationsOf(relationSurface);

        positions.Add(new Relation(Point(incoming.Carrier, 0), incoming), incoming);
        relationSurface = ApplyRelationsUntilQuiet(positions, relationSurface, relations);

        return new(BuildPositionSurface(positions.Totals()), relationSurface);
    }

    public static CarrierLocation PositionOn(this Relation structure, Carrier carrier)
    {
        var positions = ReadPositionSurface(structure.Left);
        return new CarrierLocation(carrier, positions.GetValueOrDefault(carrier));
    }

    public static Relation ReadThroughStructuralTransform(this CarrierLocation incoming, Relation transform)
    {
        return ReadThroughStructuralTransform(incoming, transform, 1, out _);
    }

    public static Relation[] ReadThroughStructuralTransforms(this CarrierLocation incoming, params Relation[] transforms)
    {
        var current = incoming;
        var scale = 1UL;
        var reads = new List<Relation>();
        foreach (var transform in transforms)
        {
            var read = ReadThroughStructuralTransform(current, transform, scale, out scale);
            reads.Add(read);
            current = NextInputFrom(read);
        }

        return [.. reads];
    }

    private static Relation ReadThroughStructuralTransform(
        CarrierLocation incoming,
        Relation transform,
        ulong scale,
        out ulong nextScale)
    {
        if (TryGetPointTransform(transform, out var lowerPoint, out var upperPoint))
        {
            nextScale = 1;
            return ReadPointTransform(incoming, lowerPoint, upperPoint);
        }

        if (TryGetSegmentTransform(transform, out var lowerSegment, out var upperSegment))
        {
            return ReadSegmentTransform(incoming, lowerSegment, upperSegment, scale, out nextScale);
        }

        var zero = new CarrierLocation(incoming.Carrier, 0);
        nextScale = 1;
        return new(zero, incoming);
    }

    private static Relation ReadPointTransform(
        CarrierLocation incoming,
        PointShape lowerPoint,
        PointShape upperPoint)
    {
        var read = incoming.ReadFrom(lowerPoint.Carrier);
        if (read.Direction <= 0)
        {
            return new(upperPoint.Location, new CarrierLocation(incoming.Carrier, 0));
        }

        if (lowerPoint.Location.Ticks == 0)
        {
            return new(upperPoint.Location, incoming);
        }

        return new(
            new CarrierLocation(upperPoint.Carrier, upperPoint.Location.Ticks + (read.Magnitude / lowerPoint.Location.Ticks)),
            new CarrierLocation(lowerPoint.Carrier, read.Magnitude % lowerPoint.Location.Ticks));
    }

    private static Relation ReadSegmentTransform(
        CarrierLocation incoming,
        SegmentShape lowerSegment,
        SegmentShape upperSegment,
        ulong scale,
        out ulong nextScale)
    {
        var read = incoming.ReadFrom(lowerSegment.Carrier);
        if (read.Direction <= 0)
        {
            nextScale = 1;
            return Segment(upperSegment.Carrier, upperSegment.Start, upperSegment.Start);
        }

        var lowerSpan = lowerSegment.Length;
        if (lowerSpan == 0)
        {
            nextScale = 1;
            return Segment(upperSegment.Carrier, upperSegment.Start, upperSegment.Start);
        }

        var upperSpan = upperSegment.Length;
        var stretched = checked((read.Magnitude * scale * upperSpan) / lowerSpan);
        nextScale = stretched == 0 ? 1 : stretched;
        return Segment(upperSegment.Carrier, upperSegment.Start, upperSegment.Start + stretched);
    }

    private static CarrierLocation NextInputFrom(Relation read)
    {
        if (TryGetSegment(read, out var segment))
        {
            return new CarrierLocation(segment.Carrier, segment.Length);
        }

        if (read.Left is CarrierLocation next)
        {
            return next;
        }

        return read.CarrierLocations().FirstOrDefault();
    }

    private static bool TryGetPointTransform(
        Relation transform,
        out PointShape lowerPoint,
        out PointShape upperPoint)
    {
        if (transform.Left is Relation left &&
            transform.Right is Relation right &&
            TryGetPoint(left, out lowerPoint) &&
            TryGetPoint(right, out upperPoint) &&
            lowerPoint.Carrier != upperPoint.Carrier)
        {
            return true;
        }

        lowerPoint = default;
        upperPoint = default;
        return false;
    }

    private static Relation[] RelationsOf(IElement surface)
    {
        var relations = new List<Relation>();
        Visit(surface);
        return [.. relations];

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            relations.Add(relation);
            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static IElement ApplyRelationsUntilQuiet(
        PositionState positions,
        IElement relationSurface,
        Relation[] relations)
    {
        while (true)
        {
            var beforePositions = positions.Snapshot();
            var beforeSurface = relationSurface;
            relationSurface = ApplyRelationsOnce(positions, relationSurface, relations);

            if (positions.SameAs(beforePositions) && Equals(beforeSurface, relationSurface))
            {
                return relationSurface;
            }
        }
    }

    private static IElement ApplyRelationsOnce(
        PositionState positions,
        IElement relationSurface,
        Relation[] relations)
    {
        foreach (var relation in relations)
        {
            relationSurface = ApplyRelation(positions, relationSurface, relation);
        }

        return relationSurface;
    }

    private static IElement ApplyRelation(
        PositionState positions,
        IElement relationSurface,
        Relation relation)
    {
        if (TryGetPointTransform(relation, out var lowerPoint, out var upperPoint))
        {
            ApplyPointRelation(positions, relation, lowerPoint, upperPoint);
            return relationSurface;
        }

        if (TryGetSegmentPointTransform(relation, out var lowerSegment, out var upperPointTarget))
        {
            return ApplySegmentPointRelation(positions, relationSurface, lowerSegment, upperPointTarget);
        }

        if (TryGetSegmentTransform(relation, out var lowerSegmentSource, out var upperSegment))
        {
            ApplySegmentRelation(positions, relation, lowerSegmentSource, upperSegment);
        }

        return relationSurface;
    }

    private static void ApplyPointRelation(
        PositionState positions,
        Relation relation,
        PointShape lowerPoint,
        PointShape upperPoint)
    {
        if (lowerPoint.Location.Ticks == 0)
        {
            return;
        }

        var lowerTicks = positions.TotalOn(lowerPoint.Carrier);
        if (lowerTicks < lowerPoint.Location.Ticks)
        {
            return;
        }

        var carried = lowerTicks / lowerPoint.Location.Ticks;
        var residue = lowerTicks % lowerPoint.Location.Ticks;
        positions.ReplaceCarrier(lowerPoint.Carrier, new Relation(relation, lowerPoint.Location), residue);
        positions.Set(relation, upperPoint.Carrier, carried);
    }

    private static void ApplySegmentRelation(
        PositionState positions,
        Relation relation,
        SegmentShape lowerSegment,
        SegmentShape upperSegment)
    {
        if (positions.HasPointResidueOn(lowerSegment.Carrier) ||
            positions.HasPointResidueOn(upperSegment.Carrier))
        {
            return;
        }

        var lowerTicks = positions.TotalOn(lowerSegment.Carrier);
        if (lowerTicks == 0 || lowerSegment.Length == 0)
        {
            return;
        }

        var upperSpan = upperSegment.Length;
        var lowerScale = positions.ScaleOn(lowerSegment.Carrier);
        var stretch = checked((lowerTicks * lowerScale * upperSpan) / lowerSegment.Length);
        if (stretch == 0)
        {
            return;
        }

        var baseTicks = positions.TotalOn(upperSegment.Carrier, except: relation);
        var desired = baseTicks == 0 ? stretch : checked(baseTicks * stretch);
        var contribution = desired > baseTicks ? desired - baseTicks : 0;
        positions.Set(relation, upperSegment.Carrier, contribution, stretch);
    }

    private static IElement ApplySegmentPointRelation(
        PositionState positions,
        IElement surface,
        SegmentShape lowerSegment,
        PointShape upperPoint)
    {
        var lowerTicks = positions.TotalOn(lowerSegment.Carrier);
        if (lowerTicks == 0 || lowerSegment.Length == 0 || upperPoint.Location.Ticks == 0)
        {
            return surface;
        }

        var mapped = checked((upperPoint.Location.Ticks * lowerTicks) / lowerSegment.Length);
        return MovePoint(surface, upperPoint, mapped);
    }

    private static IElement MovePoint(IElement element, PointShape target, ulong ticks)
    {
        if (element is not Relation relation)
        {
            return element;
        }

        if (TryGetPoint(relation, out var point) && point == target)
        {
            return Point(point.Carrier, ticks);
        }

        return new Relation(MovePoint(relation.Left, target, ticks), MovePoint(relation.Right, target, ticks));
    }

    private static Dictionary<Carrier, ulong> ReadPositionSurface(IElement surface)
    {
        var positions = new Dictionary<Carrier, ulong>();
        Visit(surface);
        return positions;

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            if (TryGetPoint(relation, out var point))
            {
                Add(positions, point.Carrier, point.Location.Ticks);
                return;
            }

            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static PositionState ReadPositionState(IElement surface)
    {
        var positions = new PositionState();
        Visit(surface);
        return positions;

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            if (TryGetPoint(relation, out var point))
            {
                positions.Add(relation, point.Location);
                return;
            }

            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static IElement BuildPositionSurface(Dictionary<Carrier, ulong> positions)
    {
        var points = positions
            .OrderBy(position => position.Key.Lineage)
            .ThenBy(position => position.Key.Id)
            .ThenBy(position => position.Key.Flow)
            .Select(position => Point(position.Key, position.Value))
            .Cast<IElement>()
            .ToArray();

        return BuildSurface(points);
    }

    private static IElement BuildSurface(IElement[] elements)
    {
        return elements.Length switch
        {
            0 => Point(Carrier.Unknown, 0),
            1 => elements[0],
            _ => new Relation(elements[0], BuildSurface(elements[1..]))
        };
    }

    private static void Add(Dictionary<Carrier, ulong> positions, Carrier carrier, ulong ticks)
    {
        positions[carrier] = checked(positions.GetValueOrDefault(carrier) + ticks);
    }

    private static bool TryGetSegmentTransform(
        Relation transform,
        out SegmentShape lowerSegment,
        out SegmentShape upperSegment)
    {
        if (transform.Left is Relation left &&
            transform.Right is Relation right &&
            TryGetSegment(left, out lowerSegment) &&
            TryGetSegment(right, out upperSegment))
        {
            return true;
        }

        lowerSegment = default;
        upperSegment = default;
        return false;
    }

    private static bool TryGetSegmentPointTransform(
        Relation transform,
        out SegmentShape lowerSegment,
        out PointShape upperPoint)
    {
        if (transform.Left is Relation left &&
            transform.Right is Relation right &&
            TryGetSegment(left, out lowerSegment) &&
            TryGetPoint(right, out upperPoint))
        {
            return true;
        }

        lowerSegment = default;
        upperPoint = default;
        return false;
    }

    private static bool TryGetSegment(Relation relation, out SegmentShape segment)
    {
        if (relation.Left is Relation leftPoint &&
            relation.Right is Relation rightPoint &&
            TryGetPoint(leftPoint, out var left) &&
            TryGetPoint(rightPoint, out var right) &&
            left.Carrier == right.Carrier)
        {
            segment = new(left.Carrier, left.Location.Ticks, right.Location.Ticks);
            return true;
        }

        segment = default;
        return false;
    }

    private static bool TryGetPoint(Relation relation, out PointShape point)
    {
        if (relation.Left is CarrierLocation location &&
            relation.Right is Carrier carrier &&
            location.Carrier == carrier)
        {
            point = new(location, carrier);
            return true;
        }

        point = default;
        return false;
    }

    private static Relation Segment(Carrier carrier, ulong start, ulong end) =>
        new(Point(carrier, start), Point(carrier, end));

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private readonly record struct PointShape(CarrierLocation Location, Carrier Carrier);

    private readonly record struct SegmentShape(Carrier Carrier, ulong Start, ulong End)
    {
        public ulong Length => End >= Start ? End - Start : 0;
    }

    private readonly record struct Contribution(IElement Source, CarrierLocation Location, ulong Scale);

    private readonly record struct ContributionSnapshot(string Source, Carrier Carrier, ulong Ticks, ulong Scale);

    private sealed class PositionState
    {
        private readonly List<Contribution> contributions = [];

        public void Add(IElement source, CarrierLocation location)
        {
            if (location.Ticks == 0)
            {
                return;
            }

            var index = contributions.FindIndex(contribution =>
                Equals(contribution.Source, source) &&
                contribution.Location.Carrier == location.Carrier);
            if (index < 0)
            {
                contributions.Add(new(source, location, 1));
                return;
            }

            var contribution = contributions[index];
            contributions[index] = contribution with
            {
                Location = new CarrierLocation(
                    location.Carrier,
                    checked(contribution.Location.Ticks + location.Ticks))
            };
        }

        public void Set(IElement source, Carrier carrier, ulong ticks, ulong scale = 1)
        {
            contributions.RemoveAll(contribution =>
                Equals(contribution.Source, source) &&
                contribution.Location.Carrier == carrier);

            if (ticks != 0)
            {
                contributions.Add(new(source, new CarrierLocation(carrier, ticks), scale == 0 ? 1 : scale));
            }
        }

        public void ReplaceCarrier(Carrier carrier, IElement source, ulong ticks)
        {
            contributions.RemoveAll(contribution => contribution.Location.Carrier == carrier);
            Set(source, carrier, ticks);
        }

        public ulong TotalOn(Carrier carrier) => contributions
            .Where(contribution => contribution.Location.Carrier == carrier)
            .Aggregate(0UL, (total, contribution) => checked(total + contribution.Location.Ticks));

        public ulong TotalOn(Carrier carrier, IElement except) => contributions
            .Where(contribution =>
                contribution.Location.Carrier == carrier &&
                !Equals(contribution.Source, except))
            .Aggregate(0UL, (total, contribution) => checked(total + contribution.Location.Ticks));

        public ulong ScaleOn(Carrier carrier)
        {
            var scale = 1UL;
            foreach (var contribution in contributions)
            {
                if (contribution.Location.Carrier == carrier && contribution.Scale > 1)
                {
                    scale = checked(scale * contribution.Scale);
                }
            }

            return scale;
        }

        public bool HasPointResidueOn(Carrier carrier) => contributions.Any(contribution =>
            contribution.Location.Carrier == carrier &&
            IsPointResidueSource(contribution.Source));

        public Dictionary<Carrier, ulong> Totals()
        {
            var totals = new Dictionary<Carrier, ulong>();
            foreach (var contribution in contributions)
            {
                StructuralTransformReads.Add(totals, contribution.Location.Carrier, contribution.Location.Ticks);
            }

            return totals;
        }

        public ContributionSnapshot[] Snapshot() => [.. contributions
            .Select(contribution => new ContributionSnapshot(
                ElementKey(contribution.Source),
                contribution.Location.Carrier,
                contribution.Location.Ticks,
                contribution.Scale))
            .OrderBy(snapshot => snapshot.Carrier.Lineage)
            .ThenBy(snapshot => snapshot.Carrier.Id)
            .ThenBy(snapshot => snapshot.Carrier.Flow)
            .ThenBy(snapshot => snapshot.Source)
            .ThenBy(snapshot => snapshot.Ticks)
            .ThenBy(snapshot => snapshot.Scale)];

        public bool SameAs(ContributionSnapshot[] snapshot) =>
            Snapshot().SequenceEqual(snapshot);

        private static string ElementKey(IElement element) => element switch
        {
            Carrier carrier => $"C({carrier.Lineage},{carrier.Id},{carrier.Flow})",
            CarrierLocation location => $"L({location.Carrier.Lineage},{location.Carrier.Id},{location.Carrier.Flow},{location.Ticks})",
            Relation relation => $"R({ElementKey(relation.Left)}>{ElementKey(relation.Right)})",
            _ => element.GetType().FullName ?? string.Empty
        };

        private static bool IsPointResidueSource(IElement source) =>
            source is Relation relation &&
            relation.Left is Relation transform &&
            relation.Right is CarrierLocation &&
            TryGetPointTransform(transform, out _, out _);
    }
}
