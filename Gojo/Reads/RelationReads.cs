using Gojo.Core;

namespace Gojo.Reads;

public static class RelationReads
{
    public static CarrierLocation[] BoundaryPointsOn(Carrier reader, params Relation[] relations)
    {
        return relations
            .Select(relation => relation.BoundariesOn(reader))
            .Where(boundary => boundary.LeftLocation.Ticks != boundary.RightLocation.Ticks)
            .SelectMany(boundary => new[] { boundary.LeftLocation, boundary.RightLocation })
            .Distinct()
            .OrderBy(location => location.Carrier.Id)
            .ThenBy(location => location.Ticks)
            .ToArray();
    }

    public static CarrierLocation[] TraversedBoundaryPointsOn(Carrier reader, Relation travel, params Relation[] relations)
    {
        if (!travel.TryGetCarrierLocations(out var leftLocation, out var rightLocation))
        {
            return [];
        }

        if (!TryReadInCarrier(reader, leftLocation, rightLocation, out var start, out var end) ||
            start == end)
        {
            return [];
        }

        return BoundaryPointsOn(reader, relations)
            .Where(point => point.Carrier.SameIdentityAs(reader) && IsTraversed(point.Ticks, start, end))
            .OrderBy(point => start < end ? point.Ticks : -point.Ticks)
            .ToArray();
    }

    public static Relation[] TraversedBoundaryStatesOn(Carrier reader, Relation travel, params Relation[] relations)
    {
        var flow = travel.FlowOn(reader);
        return flow.ReadFrom(reader).IsVisible
            ? TraversedBoundaryPointsOn(reader, travel, relations)
                .Select(point => new Relation(point, flow))
                .ToArray()
            : [];
    }

    public static Relation[] TraversedSpansOn(Carrier reader, Relation travel, params Relation[] relations)
    {
        if (!travel.TryGetCarrierLocations(out var leftLocation, out var rightLocation))
        {
            return [];
        }

        if (!TryReadInCarrier(reader, leftLocation, rightLocation, out var start, out var end) ||
            start == end)
        {
            return [];
        }

        var points = new List<CarrierLocation>
        {
            new(reader, start)
        };

        foreach (var point in TraversedBoundaryPointsOn(reader, travel, relations))
        {
            AddPoint(point);
        }

        AddPoint(new CarrierLocation(reader, end));

        var spans = new List<Relation>();
        for (var i = 0; i < points.Count - 1; i++)
        {
            if (points[i].Ticks != points[i + 1].Ticks)
            {
                spans.Add(new(points[i], points[i + 1]));
            }
        }

        return [.. spans];

        void AddPoint(CarrierLocation point)
        {
            if (!points.Contains(point))
            {
                points.Add(point);
            }
        }
    }

    public static Relation[] TraversedSpanStatesOn(Carrier reader, Relation travel, params Relation[] relations) =>
        TraversedSpansOn(reader, travel, relations)
            .Select(span => new Relation(span, span.FlowOn(reader)))
            .ToArray();

    public static Relation[] TraversedSupportStatesOn(Carrier reader, Relation travel, params Relation[] support) =>
        TraversedSpansOn(reader, travel, support)
            .Select(span => new Relation(span, SupportStateOn(span, reader, support)))
            .ToArray();

    public static Relation[] TraversedSupportOn(Carrier reader, Relation travel, params Relation[] support) =>
        ProjectTraversedStatesOn(reader, travel, state => state.SupportOn(reader), support);

    public static Relation[] TraversedTensionOn(Carrier reader, Relation travel, params Relation[] support) =>
        ProjectTraversedStatesOn(reader, travel, state => state.TensionOn(reader), support);

    public static Relation[] TraversedExclusiveOn(Carrier reader, Relation travel, params Relation[] support) =>
        ProjectTraversedStatesOn(reader, travel, state => state.ExclusiveOn(reader), support);

    public static Relation[] TraversedPassOn(Carrier reader, Relation travel, params Relation[] support) =>
        TraversedSupportOn(reader, travel, support);

    public static Relation[] TraversedBlockOn(Carrier reader, Relation travel, params Relation[] support) =>
        TraversedSupportStatesOn(reader, travel, support)
            .Where(state => state.Left is Relation && state.Right is Relation supportState && !HasStateParticipationOn(supportState, reader))
            .Select(state => (Relation)state.Left)
            .ToArray();

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

    public static Relation[] CoveredSpansWithinOn(Carrier reader, Relation scope, params Relation[] relations)
    {
        var scopeBoundary = scope.BoundariesOn(reader);
        if (scopeBoundary.LeftLocation.Ticks == scopeBoundary.RightLocation.Ticks)
        {
            return [];
        }

        var coveredBoundaries = ClipToScope(reader, scopeBoundary, relations);
        if (coveredBoundaries.Length == 0)
        {
            return [];
        }

        return SplitScopeOn(scopeBoundary, coveredBoundaries)
            .Where(span => IsCovered(span.LeftLocation.Ticks, span.RightLocation.Ticks, coveredBoundaries))
            .ToArray();
    }

    public static Relation[] SupportStatesOn(Carrier reader, params Relation[] relations) =>
        CoveredSpansOn(reader, relations)
            .Select(span => new Relation(span, SupportStateOn(span, reader, relations)))
            .ToArray();

    public static Relation[] SupportStatesWithinOn(Carrier reader, Relation scope, params Relation[] relations) =>
        CoveredSpansWithinOn(reader, scope, relations)
            .Select(span => new Relation(span, SupportStateOn(span, reader, relations)))
            .ToArray();

    public static Relation[] SupportOn(Carrier reader, params Relation[] relations) =>
        ProjectStatesOn(reader, state => state.SupportOn(reader), relations);

    public static Relation[] TensionOn(Carrier reader, params Relation[] relations) =>
        ProjectStatesOn(reader, state => state.TensionOn(reader), relations);

    public static Relation[] ExclusiveOn(Carrier reader, params Relation[] relations) =>
        ProjectStatesOn(reader, state => state.ExclusiveOn(reader), relations);

    public static Relation[] SupportWithinOn(Carrier reader, Relation scope, params Relation[] relations) =>
        ProjectStatesWithinOn(reader, scope, state => state.SupportOn(reader), relations);

    public static Relation[] TensionWithinOn(Carrier reader, Relation scope, params Relation[] relations) =>
        ProjectStatesWithinOn(reader, scope, state => state.TensionOn(reader), relations);

    public static Relation[] ExclusiveWithinOn(Carrier reader, Relation scope, params Relation[] relations) =>
        ProjectStatesWithinOn(reader, scope, state => state.ExclusiveOn(reader), relations);

    public static Relation[] UncoveredSpansOn(Carrier reader, Relation scope, params Relation[] covered)
    {
        var scopeBoundary = scope.BoundariesOn(reader);
        if (scopeBoundary.LeftLocation.Ticks == scopeBoundary.RightLocation.Ticks)
        {
            return [];
        }

        var coveredBoundaries = ClipToScope(reader, scopeBoundary, covered);
        return SplitScopeOn(scopeBoundary, coveredBoundaries)
            .Where(span => !IsCovered(span.LeftLocation.Ticks, span.RightLocation.Ticks, coveredBoundaries))
            .ToArray();
    }

    private static Relation[] ProjectStatesOn(
        Carrier reader,
        Func<Relation, Relation> projection,
        Relation[] relations) =>
        SupportStatesOn(reader, relations)
            .Select(state => new Relation(state.Left, projection(state)))
            .Where(state => state.Right is Relation projected && HasStateParticipationOn(projected, reader))
            .ToArray();

    private static Relation[] ProjectStatesWithinOn(
        Carrier reader,
        Relation scope,
        Func<Relation, Relation> projection,
        Relation[] relations) =>
        SupportStatesWithinOn(reader, scope, relations)
            .Select(state => new Relation(state.Left, projection(state)))
            .Where(state => state.Right is Relation projected && HasStateParticipationOn(projected, reader))
            .ToArray();

    private static Relation[] ProjectTraversedStatesOn(
        Carrier reader,
        Relation travel,
        Func<Relation, Relation> projection,
        Relation[] relations) =>
        TraversedSupportStatesOn(reader, travel, relations)
            .Select(state => new Relation(state.Left, projection(state)))
            .Where(state => state.Right is Relation projected && HasStateParticipationOn(projected, reader))
            .ToArray();

    private static Relation SupportStateOn(Relation span, Carrier reader, Relation[] relations)
    {
        var reverse = 0UL;
        var forward = 0UL;
        var spanStart = Math.Min(span.LeftLocation.Ticks, span.RightLocation.Ticks);
        var spanEnd = Math.Max(span.LeftLocation.Ticks, span.RightLocation.Ticks);
        foreach (var relation in relations)
        {
            var boundaries = relation.BoundariesOn(reader);
            if (!boundaries.LeftLocation.Carrier.SameIdentityAs(span.LeftLocation.Carrier) ||
                !IsCovered(spanStart, spanEnd, [boundaries]))
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
            new CarrierLocation(reader, -checked((long)reverse)),
            new CarrierLocation(reader, forward));
    }

    private static bool HasStateParticipationOn(Relation state, Carrier reader) =>
        state.CarrierLocations().Any(location => location.ReadFrom(reader).IsVisible);

    private static bool IsCovered(long start, long end, Relation[] boundaries) =>
        boundaries.Any(boundary => boundary.LeftLocation.Ticks <= start && boundary.RightLocation.Ticks >= end);

    private static bool IsTraversed(long point, long from, long to) =>
        from < to
            ? point > from && point <= to
            : point < from && point >= to;

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

    private static Relation ClipTo(Relation scope, Relation boundary)
    {
        if (!scope.LeftLocation.Carrier.SameIdentityAs(boundary.LeftLocation.Carrier))
        {
            var zero = new CarrierLocation(scope.LeftLocation.Carrier, 0);
            return new(zero, zero);
        }

        var start = Math.Max(scope.LeftLocation.Ticks, boundary.LeftLocation.Ticks);
        var end = Math.Min(scope.RightLocation.Ticks, boundary.RightLocation.Ticks);
        return start < end
            ? new Relation(new CarrierLocation(scope.LeftLocation.Carrier, start), new CarrierLocation(scope.LeftLocation.Carrier, end))
            : new Relation(scope.LeftLocation, scope.LeftLocation);
    }

    private static Relation[] ClipToScope(Carrier reader, Relation scope, Relation[] relations) =>
        relations
            .Select(relation => relation.BoundariesOn(reader))
            .Select(boundary => ClipTo(scope, boundary))
            .Where(boundary => boundary.LeftLocation.Ticks != boundary.RightLocation.Ticks)
            .ToArray();

    private static Relation[] SplitScopeOn(Relation scope, Relation[] boundaries)
    {
        var points = boundaries
            .SelectMany(boundary => new[] { boundary.LeftLocation, boundary.RightLocation })
            .Append(scope.LeftLocation)
            .Append(scope.RightLocation)
            .Distinct()
            .OrderBy(location => location.Ticks)
            .ToArray();

        var spans = new List<Relation>();
        for (var i = 0; i < points.Length - 1; i++)
        {
            var start = points[i];
            var end = points[i + 1];
            spans.Add(new(start, end));
        }

        return [.. spans];
    }
}
