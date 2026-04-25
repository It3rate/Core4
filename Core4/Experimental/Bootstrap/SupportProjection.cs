namespace Core4.Experimental.Bootstrap;

public static class SupportProjection
{
    public static IElement ProjectOn(IElement element, Carrier carrier, Func<long, long, long> read, bool preserveDirection = false) =>
        BuildProjection(
            SupportSpanUtilities.BuildStateSpans(element, carrier),
            carrier,
            read,
            preserveDirection: preserveDirection);

    public static IElement SupportOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.TotalSupport);

    public static IElement TensionOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.Tension);

    public static IElement ExclusiveSupportOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.Exclusive);

    public static IElement ForwardResidueOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.ForwardResidue, preserveDirection: true);

    public static IElement ReverseResidueOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.ReverseResidue, preserveDirection: true);

    private static IElement BuildProjection(
        IReadOnlyList<SupportStateSpan> states,
        Carrier carrier,
        Func<long, long, long> read,
        bool preserveDirection = false)
    {
        var projected = new List<ProjectedSpan>();
        foreach (var state in states)
        {
            var value = read(state.Reverse, state.Forward);
            if (value == 0)
            {
                continue;
            }

            projected.Add(new(
                state.Start,
                state.End,
                preserveDirection ? Math.Sign(value) : 1));
        }

        if (projected.Count == 0)
        {
            return Measure.Fixed(0, carrier);
        }

        var merged = MergeProjected(projected);
        var built = merged
            .Select(span => BuildSpan(span, carrier))
            .ToArray();

        return built.Aggregate((left, right) => new Relation(left, right));
    }

    private static ProjectedSpan[] MergeProjected(List<ProjectedSpan> spans)
    {
        if (spans.Count == 0)
        {
            return [];
        }

        var merged = new List<ProjectedSpan> { spans[0] };
        for (var i = 1; i < spans.Count; i++)
        {
            var current = spans[i];
            var lastIndex = merged.Count - 1;
            var last = merged[lastIndex];
            if (last.Direction == current.Direction && last.End == current.Start)
            {
                merged[lastIndex] = last with { End = current.End };
            }
            else
            {
                merged.Add(current);
            }
        }

        return merged.ToArray();
    }

    private static IElement BuildSpan(ProjectedSpan span, Carrier carrier) =>
        span.Direction >= 0
            ? new Relation(Measure.Fixed(span.Start, carrier), Measure.Fixed(span.End, carrier))
            : new Relation(Measure.Fixed(span.End, carrier), Measure.Fixed(span.Start, carrier));

    private record struct ProjectedSpan(long Start, long End, int Direction);
}
