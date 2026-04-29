using Gojo.Core;

namespace Gojo.Reads;

public static class StructuralTransformReads
{
    public static Relation AddAndSettle(this Relation structure, CarrierLocation incoming) =>
        structure.AddAndSettle((IElement)incoming);

    public static Relation AddAndSettle(this Relation structure, IElement incoming)
    {
        var positions = ContributionSurface.From(structure.Left);
        var relationSurface = structure.Right;
        var relations = RelationsOf(relationSurface);

        positions.AddSurface(incoming);
        relationSurface = ApplyRelationsUntilQuiet(positions, relationSurface, relations);

        return new(positions.Surface(), relationSurface);
    }

    public static CarrierLocation PositionOn(this Relation structure, Carrier carrier)
    {
        return structure.Left.ContributionTotalOn(carrier);
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

            if (TryGetOpenRoute(relation, out _) ||
                TryGetObservationRoute(relation, out _, out _) ||
                TryGetTransferRoute(relation, out _, out _))
            {
                relations.Add(relation);
                return;
            }

            relations.Add(relation);
            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static IElement ApplyRelationsUntilQuiet(
        ContributionSurface positions,
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
        ContributionSurface positions,
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
        ContributionSurface positions,
        IElement relationSurface,
        Relation relation)
    {
        if (TryGetOpenRoute(relation, out var openSource))
        {
            return ApplyOpenRoute(positions, relationSurface, relation, openSource);
        }

        if (TryGetObservationRoute(relation, out var window, out var observer))
        {
            ApplyObservationRoute(positions, relation, window, observer);
            return relationSurface;
        }

        if (TryGetTransferRoute(relation, out var capacity, out var receiver))
        {
            ApplyTransferRoute(positions, relation, capacity, receiver);
            return relationSurface;
        }

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

    private static IElement ApplyOpenRoute(
        ContributionSurface positions,
        IElement surface,
        Relation relation,
        CarrierLocation openSource)
    {
        if (!Contains(surface, relation))
        {
            return surface;
        }

        var sourceTicks = positions.TotalOn(openSource.Carrier);
        if (sourceTicks <= openSource.Ticks)
        {
            return surface;
        }

        return Replace(
            surface,
            relation,
            new Relation(
                new CarrierLocation(openSource.Carrier, sourceTicks),
                relation.Right));
    }

    private static void ApplyTransferRoute(
        ContributionSurface positions,
        Relation relation,
        CarrierLocation capacity,
        IElement receiver)
    {
        if (positions.HasSource(relation) || capacity.Ticks == 0)
        {
            return;
        }

        var sourceTicks = positions.TotalOn(capacity.Carrier);
        if (sourceTicks == 0)
        {
            return;
        }

        var transferred = Math.Min(sourceTicks, capacity.Ticks);
        var residual = sourceTicks - transferred;
        positions.ReplaceCarrier(
            capacity.Carrier,
            new Relation(relation, capacity),
            residual,
            scale: 0);
        positions.SetSurface(relation, Receive(receiver, new CarrierLocation(capacity.Carrier, transferred)));
    }

    private static void ApplyObservationRoute(
        ContributionSurface positions,
        Relation relation,
        BareSpanShape window,
        IElement observer)
    {
        if (positions.HasSource(relation) || window.Length == 0)
        {
            return;
        }

        var sourceTicks = positions.TotalOn(window.Carrier);
        if (sourceTicks == 0)
        {
            return;
        }

        var observed = Math.Min(sourceTicks, window.Length);
        positions.SetSurface(relation, Receive(observer, new CarrierLocation(window.Carrier, observed)));
    }

    private static bool Contains(IElement element, IElement target)
    {
        if (Equals(element, target))
        {
            return true;
        }

        return element is Relation relation &&
            (Contains(relation.Left, target) || Contains(relation.Right, target));
    }

    private static IElement Replace(IElement element, IElement target, IElement replacement)
    {
        if (Equals(element, target))
        {
            return replacement;
        }

        return element is Relation relation
            ? new Relation(Replace(relation.Left, target, replacement), Replace(relation.Right, target, replacement))
            : element;
    }

    private static void ApplyPointRelation(
        ContributionSurface positions,
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
        positions.ReplaceCarrier(lowerPoint.Carrier, new Relation(relation, lowerPoint.Location), residue, scale: 0);
        positions.Set(relation, upperPoint.Carrier, carried);
    }

    private static void ApplySegmentRelation(
        ContributionSurface positions,
        Relation relation,
        SegmentShape lowerSegment,
        SegmentShape upperSegment)
    {
        if (positions.HasZeroScaleOn(lowerSegment.Carrier) ||
            positions.HasZeroScaleOn(upperSegment.Carrier))
        {
            return;
        }

        var lowerTicks = positions.TotalOn(lowerSegment.Carrier);
        if (lowerTicks == 0 || lowerSegment.Length == 0)
        {
            return;
        }

        var upperSpan = upperSegment.Length;
        var stretch = checked((lowerTicks * upperSpan) / lowerSegment.Length);
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
        ContributionSurface positions,
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

    private static bool TryGetTransferRoute(
        Relation relation,
        out CarrierLocation capacity,
        out IElement receiver)
    {
        if (relation.Left is CarrierLocation left &&
            relation.Right is not Carrier &&
            !IsSameCarrierSpan(left, relation.Right))
        {
            capacity = left;
            receiver = relation.Right;
            return true;
        }

        capacity = default;
        receiver = default!;
        return false;
    }

    private static bool IsSameCarrierSpan(CarrierLocation left, IElement right) =>
        right is CarrierLocation location &&
        location.Carrier == left.Carrier;

    private static bool TryGetObservationRoute(
        Relation relation,
        out BareSpanShape window,
        out IElement observer)
    {
        if (relation.Left is Relation left &&
            TryGetBareSpan(left, out window))
        {
            observer = relation.Right;
            return true;
        }

        window = default;
        observer = default!;
        return false;
    }

    private static bool TryGetBareSpan(Relation relation, out BareSpanShape span)
    {
        if (relation.Left is CarrierLocation left &&
            relation.Right is CarrierLocation right &&
            left.Carrier == right.Carrier)
        {
            span = new(
                left.Carrier,
                Math.Min(left.Ticks, right.Ticks),
                Math.Max(left.Ticks, right.Ticks));
            return true;
        }

        span = default;
        return false;
    }

    private static bool TryGetOpenRoute(Relation relation, out CarrierLocation source)
    {
        if (relation.Left is CarrierLocation left &&
            relation.Right is CarrierLocation { Carrier: var carrier, Ticks: 0 } &&
            carrier == Carrier.Unknown)
        {
            source = left;
            return true;
        }

        source = default;
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

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static IElement Receive(IElement target, CarrierLocation accounted) => target switch
    {
        CarrierLocation location => new CarrierLocation(location.Carrier, checked(location.Ticks + accounted.Ticks)),
        Relation relation => new Relation(Receive(relation.Left, accounted), Receive(relation.Right, accounted)),
        _ => target
    };

    private readonly record struct PointShape(CarrierLocation Location, Carrier Carrier);

    private readonly record struct BareSpanShape(Carrier Carrier, ulong Start, ulong End)
    {
        public ulong Length => End - Start;
    }

    private readonly record struct SegmentShape(Carrier Carrier, ulong Start, ulong End)
    {
        public ulong Length => End >= Start ? End - Start : 0;
    }
}
