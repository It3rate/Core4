using Gojo.Core;

namespace Gojo.Reads;

public static class StructuralTransformReads
{
    public static Relation AddAndSettle(this Relation structure, CarrierLocation incoming)
    {
        var positions = ReadPositionSurface(structure.Left);
        var relationSurface = structure.Right;
        var segmentPointTransforms = ReadSegmentPointTransforms(relationSurface);

        Add(positions, incoming.Carrier, incoming.Ticks);
        relationSurface = SettlePointCarryAndEndpointMovement(positions, relationSurface, segmentPointTransforms);
        ApplySegmentTransforms(positions, ReadSegmentTransforms(relationSurface));
        relationSurface = SettlePointCarryAndEndpointMovement(positions, relationSurface, segmentPointTransforms);
        SettlePointTransforms(positions, ReadPointTransforms(relationSurface));

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
            TryGetPoint(right, out upperPoint))
        {
            return true;
        }

        lowerPoint = default;
        upperPoint = default;
        return false;
    }

    private static PointTransformShape[] ReadPointTransforms(IElement surface)
    {
        var transforms = new List<PointTransformShape>();
        Visit(surface);
        return [.. transforms];

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            if (TryGetPointTransform(relation, out var lowerPoint, out var upperPoint))
            {
                transforms.Add(new(lowerPoint, upperPoint));
                return;
            }

            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static SegmentTransformShape[] ReadSegmentTransforms(IElement surface)
    {
        var transforms = new List<SegmentTransformShape>();
        Visit(surface);
        return [.. transforms];

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            if (TryGetSegmentTransform(relation, out var lowerSegment, out var upperSegment))
            {
                transforms.Add(new(lowerSegment, upperSegment));
                return;
            }

            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static SegmentPointTransformShape[] ReadSegmentPointTransforms(IElement surface)
    {
        var transforms = new List<SegmentPointTransformShape>();
        Visit(surface);
        return [.. transforms];

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            if (TryGetSegmentPointTransform(relation, out var lowerSegment, out var upperPoint))
            {
                transforms.Add(new(lowerSegment, upperPoint));
                return;
            }

            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static IElement SettlePointCarryAndEndpointMovement(
        Dictionary<Carrier, ulong> positions,
        IElement relationSurface,
        SegmentPointTransformShape[] segmentPointTransforms)
    {
        SettlePointTransforms(positions, ReadPointTransforms(relationSurface));
        return ApplySegmentToPointTransforms(positions, relationSurface, segmentPointTransforms);
    }

    private static void SettlePointTransforms(
        Dictionary<Carrier, ulong> positions,
        PointTransformShape[] transforms)
    {
        while (true)
        {
            var before = new Dictionary<Carrier, ulong>(positions);
            var moved = false;
            foreach (var transform in transforms)
            {
                if (transform.Lower.Location.Ticks == 0)
                {
                    continue;
                }

                var lowerTicks = positions.GetValueOrDefault(transform.Lower.Carrier);
                if (lowerTicks < transform.Lower.Location.Ticks)
                {
                    continue;
                }

                var carried = lowerTicks / transform.Lower.Location.Ticks;
                var residue = lowerTicks % transform.Lower.Location.Ticks;
                positions[transform.Lower.Carrier] = residue;
                Add(positions, transform.Upper.Carrier, carried);
                moved = true;
            }

            if (!moved || SamePositions(before, positions))
            {
                return;
            }
        }
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

    private static void ApplySegmentTransforms(
        Dictionary<Carrier, ulong> positions,
        SegmentTransformShape[] transforms)
    {
        var carrierScales = new Dictionary<Carrier, ulong>();
        foreach (var transform in transforms)
        {
            var lowerTicks = positions.GetValueOrDefault(transform.Lower.Carrier);
            if (lowerTicks == 0 || transform.Lower.Length == 0)
            {
                continue;
            }

            var upperSpan = transform.Upper.Length;
            var lowerScale = carrierScales.GetValueOrDefault(transform.Lower.Carrier, 1UL);
            var stretch = checked((lowerTicks * lowerScale * upperSpan) / transform.Lower.Length);
            if (stretch == 0)
            {
                continue;
            }

            var upperTicks = positions.GetValueOrDefault(transform.Upper.Carrier);
            positions[transform.Upper.Carrier] = upperTicks == 0
                ? stretch
                : checked(upperTicks * stretch);
            carrierScales[transform.Upper.Carrier] = stretch;
        }
    }

    private static IElement ApplySegmentToPointTransforms(
        Dictionary<Carrier, ulong> positions,
        IElement surface,
        SegmentPointTransformShape[] transforms)
    {
        foreach (var transform in transforms)
        {
            var lowerTicks = positions.GetValueOrDefault(transform.Lower.Carrier);
            if (lowerTicks == 0 || transform.Lower.Length == 0 || transform.Upper.Location.Ticks == 0)
            {
                continue;
            }

            var mapped = checked((transform.Upper.Location.Ticks * lowerTicks) / transform.Lower.Length);
            surface = MovePoint(surface, transform.Upper, mapped);
        }

        return surface;
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
