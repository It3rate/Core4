namespace Gojo.Core;

public readonly record struct Relation(CarrierLocation Left, CarrierLocation Right)
{
    public static CarrierLocation[] BoundaryPointsOn(Carrier reader, params Relation[] relations)
    {
        return relations
            .Select(relation => relation.BoundariesOn(reader))
            .Where(boundary => boundary.Left.Ticks != boundary.Right.Ticks)
            .SelectMany(boundary => new[] { boundary.Left, boundary.Right })
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
            .Where(boundary => boundary.Left.Ticks != boundary.Right.Ticks)
            .ToArray();

        if (boundaries.Length == 0)
        {
            return [];
        }

        var spans = new List<Relation>();
        foreach (var group in boundaries.GroupBy(boundary => boundary.Left.Carrier))
        {
            var groupedBoundaries = group.ToArray();
            var points = groupedBoundaries
                .SelectMany(boundary => new[] { boundary.Left, boundary.Right })
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

    public Relation BoundariesOn(Carrier reader)
    {
        var left = Left.ReadFrom(reader);
        var right = Right.ReadFrom(reader);
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
        return new CarrierLocation(boundaries.Left.Carrier, boundaries.Right.Ticks - boundaries.Left.Ticks);
    }

    public CarrierLocation FlowOn(Carrier reader)
    {
        var left = Left.ReadFrom(reader);
        var right = Right.ReadFrom(reader);
        if (left.Direction == 0 || right.Direction == 0 || left.Direction != right.Direction)
        {
            return new CarrierLocation(reader, 0);
        }

        var delta = right.ProjectedTicks - left.ProjectedTicks;
        return delta >= 0
            ? new CarrierLocation(reader, checked((ulong)delta))
            : new CarrierLocation(reader.Inverse(), checked((ulong)-delta));
    }

    private static bool IsCovered(ulong start, ulong end, Relation[] boundaries) =>
        boundaries.Any(boundary => boundary.Left.Ticks <= start && boundary.Right.Ticks >= end);
}
