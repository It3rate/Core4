using Gojo.Core;

namespace Gojo.Reads;

public static class StructuralTransformReads
{
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
}
