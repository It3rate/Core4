namespace Core4.Kernel.Core;

public readonly record struct DirectedSupportSpan(long Start, long End, int Direction)
{
    public long Width => End - Start;
}

public readonly record struct SupportStateSpan
{
    public SupportStateSpan(long start, long end, long reverse, long forward)
    {
        Start = start;
        End = end;
        Reverse = reverse;
        Forward = forward;
    }

    public long Start { get; init; }

    public long End { get; init; }

    public long Reverse { get; init; }

    public long Forward { get; init; }

    public long Net => Forward - Reverse;

    public long Support => Forward + Reverse;

    public long Tension => Math.Min(Forward, Reverse);

    public long Exclusive => Math.Abs(Net);

    public bool HasSupport => Support > 0;

    public bool HasTension => Tension > 0;

    public bool HasForwardResidue => Net > 0;

    public bool HasReverseResidue => Net < 0;
}

public static class SupportSpanUtilities
{
    public static DirectedSupportSpan[] DiscoverDirectedSpans(IElement element, Carrier carrier)
    {
        var spans = new List<DirectedSupportSpan>();
        CollectDirectedSpans(element, carrier, spans);
        return spans.ToArray();
    }

    public static SupportStateSpan[] BuildStateSpans(IElement element, Carrier carrier) =>
        BuildStateSpans(DiscoverDirectedSpans(element, carrier));

    public static SupportStateSpan[] BuildStateSpans(IEnumerable<DirectedSupportSpan> spans)
    {
        var ordered = spans
            .Where(span => span.Start < span.End && span.Direction != 0)
            .OrderBy(span => span.Start)
            .ThenBy(span => span.End)
            .ToArray();

        if (ordered.Length == 0)
        {
            return [];
        }

        var boundaries = ordered
            .SelectMany(span => new[] { span.Start, span.End })
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        var states = new List<SupportStateSpan>();
        for (var i = 0; i < boundaries.Length - 1; i++)
        {
            var start = boundaries[i];
            var end = boundaries[i + 1];
            if (start == end)
            {
                continue;
            }

            var forward = 0L;
            var reverse = 0L;
            foreach (var span in ordered)
            {
                if (span.Start >= end || span.End <= start)
                {
                    continue;
                }

                if (span.Direction > 0)
                {
                    forward++;
                }
                else
                {
                    reverse++;
                }
            }

            var support = forward + reverse;
            if (support == 0)
            {
                continue;
            }

            states.Add(new(start, end, reverse, forward));
        }

        return states.ToArray();
    }

    private static void CollectDirectedSpans(IElement element, Carrier carrier, List<DirectedSupportSpan> spans)
    {
        switch (element)
        {
            case Measure measure when measure.Carrier == carrier:
                TryAddDirectedSpan(measure, spans);
                break;

            case Relation relation:
                CollectDirectedSpans(relation.Left, carrier, spans);
                CollectDirectedSpans(relation.Right, carrier, spans);
                break;

            case RelationFold fold when ShouldDescendIntoReduced(fold):
                CollectDirectedSpans(fold.Reduced, carrier, spans);
                break;

            case RelationFold fold when ContainsCarrier(fold, carrier):
                TryAddDirectedSpan(fold, spans);
                break;
        }
    }

    private static bool ContainsCarrier(IElement element, Carrier carrier) => element switch
    {
        Measure measure => measure.Carrier == carrier,
        Relation relation => ContainsCarrier(relation.Left, carrier) || ContainsCarrier(relation.Right, carrier),
        RelationFold fold => ContainsCarrier(fold.Left, carrier) || ContainsCarrier(fold.Right, carrier),
        _ => false
    };

    private static bool ShouldDescendIntoReduced(RelationFold fold) =>
        fold.Reduced is Relation relation &&
        (relation.Left is Relation or RelationFold || relation.Right is Relation or RelationFold);

    private static void TryAddDirectedSpan(IElement element, List<DirectedSupportSpan> spans)
    {
        var partitions = element.Partitions().ToArray();
        if (partitions.Length == 0)
        {
            return;
        }

        var direction = Math.Sign(partitions.First(step => step.Width != 0).Width);
        var start = long.MaxValue;
        var end = long.MinValue;

        foreach (var partition in partitions)
        {
            var partitionStart = Math.Min(partition.Start, partition.Start + partition.Width);
            var partitionEnd = Math.Max(partition.Start, partition.Start + partition.Width);
            start = Math.Min(start, partitionStart);
            end = Math.Max(end, partitionEnd);
        }

        if (start == long.MaxValue || end == long.MinValue || start == end)
        {
            return;
        }

        spans.Add(new(start, end, direction));
    }
}
