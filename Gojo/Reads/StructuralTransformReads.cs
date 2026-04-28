using Gojo.Core;

namespace Gojo.Reads;

public static class StructuralTransformReads
{
    public static Relation AddAndSettle(this Relation structure, CarrierLocation incoming)
    {
        var positions = ReadPositionSurface(structure.Left);
        var relationSurface = structure.Right;
        var relations = RelationsOf(relationSurface);

        Add(positions, incoming.Carrier, incoming.Ticks);
        relationSurface = ApplyRelationsUntilQuiet(positions, relationSurface, relations);
        relationSurface = ApplyRelationsOnce(positions, relationSurface, relations, includeSegmentRelations: true);
        relationSurface = ApplyRelationsUntilQuiet(positions, relationSurface, relations);

        return new(BuildPositionSurface(positions), relationSurface);
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
        Dictionary<Carrier, ulong> positions,
        IElement relationSurface,
        Relation[] relations)
    {
        while (true)
        {
            var beforePositions = new Dictionary<Carrier, ulong>(positions);
            var beforeSurface = relationSurface;
            relationSurface = ApplyRelationsOnce(
                positions,
                relationSurface,
                relations,
                includeSegmentRelations: false);

            if (SamePositions(beforePositions, positions) && Equals(beforeSurface, relationSurface))
            {
                return relationSurface;
            }
        }
    }

    private static IElement ApplyRelationsOnce(
        Dictionary<Carrier, ulong> positions,
        IElement relationSurface,
        Relation[] relations,
        bool includeSegmentRelations)
    {
        var segmentScales = new Dictionary<Carrier, ulong>();
        foreach (var relation in relations)
        {
            relationSurface = ApplyRelation(
                positions,
                relationSurface,
                relation,
                segmentScales,
                includeSegmentRelations);
        }

        return relationSurface;
    }

    private static IElement ApplyRelation(
        Dictionary<Carrier, ulong> positions,
        IElement relationSurface,
        Relation relation,
        Dictionary<Carrier, ulong> segmentScales,
        bool includeSegmentRelations)
    {
        if (TryGetPointTransform(relation, out var lowerPoint, out var upperPoint))
        {
            ApplyPointRelation(positions, lowerPoint, upperPoint);
            return relationSurface;
        }

        if (TryGetSegmentPointTransform(relation, out var lowerSegment, out var upperPointTarget))
        {
            return ApplySegmentPointRelation(positions, relationSurface, lowerSegment, upperPointTarget);
        }

        if (includeSegmentRelations &&
            TryGetSegmentTransform(relation, out var lowerSegmentSource, out var upperSegment))
        {
            ApplySegmentRelation(positions, segmentScales, lowerSegmentSource, upperSegment);
        }

        return relationSurface;
    }

    private static void ApplyPointRelation(
        Dictionary<Carrier, ulong> positions,
        PointShape lowerPoint,
        PointShape upperPoint)
    {
        if (lowerPoint.Location.Ticks == 0)
        {
            return;
        }

        var lowerTicks = positions.GetValueOrDefault(lowerPoint.Carrier);
        if (lowerTicks < lowerPoint.Location.Ticks)
        {
            return;
        }

        var carried = lowerTicks / lowerPoint.Location.Ticks;
        var residue = lowerTicks % lowerPoint.Location.Ticks;
        positions[lowerPoint.Carrier] = residue;
        Add(positions, upperPoint.Carrier, carried);
    }

    private static void ApplySegmentRelation(
        Dictionary<Carrier, ulong> positions,
        Dictionary<Carrier, ulong> segmentScales,
        SegmentShape lowerSegment,
        SegmentShape upperSegment)
    {
        var lowerTicks = positions.GetValueOrDefault(lowerSegment.Carrier);
        if (lowerTicks == 0 || lowerSegment.Length == 0)
        {
            return;
        }

        var upperSpan = upperSegment.Length;
        var lowerScale = segmentScales.GetValueOrDefault(lowerSegment.Carrier, 1UL);
        var stretch = checked((lowerTicks * lowerScale * upperSpan) / lowerSegment.Length);
        if (stretch == 0)
        {
            return;
        }

        var upperTicks = positions.GetValueOrDefault(upperSegment.Carrier);
        positions[upperSegment.Carrier] = upperTicks == 0
            ? stretch
            : checked(upperTicks * stretch);
        segmentScales[upperSegment.Carrier] = stretch;
    }

    private static IElement ApplySegmentPointRelation(
        Dictionary<Carrier, ulong> positions,
        IElement surface,
        SegmentShape lowerSegment,
        PointShape upperPoint)
    {
        var lowerTicks = positions.GetValueOrDefault(lowerSegment.Carrier);
        if (lowerTicks == 0 || lowerSegment.Length == 0 || upperPoint.Location.Ticks == 0)
        {
            return surface;
        }

        var mapped = checked((upperPoint.Location.Ticks * lowerTicks) / lowerSegment.Length);
        return MovePoint(surface, upperPoint, mapped);
    }

    private static bool SamePositions(
        Dictionary<Carrier, ulong> left,
        Dictionary<Carrier, ulong> right)
    {
        foreach (var carrier in left.Keys.Concat(right.Keys).Distinct())
        {
            if (left.GetValueOrDefault(carrier) != right.GetValueOrDefault(carrier))
            {
                return false;
            }
        }

        return true;
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

    private readonly record struct PointTransformShape(PointShape Lower, PointShape Upper);

    private readonly record struct SegmentTransformShape(SegmentShape Lower, SegmentShape Upper);

    private readonly record struct SegmentPointTransformShape(SegmentShape Lower, PointShape Upper);

    private readonly record struct SegmentShape(Carrier Carrier, ulong Start, ulong End)
    {
        public ulong Length => End >= Start ? End - Start : 0;
    }
}
