namespace Core4.Experimental.Bootstrap;

public static class SupportProjection
{
    public static IElement ProjectOn(
        IElement element,
        Carrier carrier,
        Func<IElement, IElement> emit,
        bool collapseToSupport = false) =>
        BuildProjection(
            SupportSpanUtilities.BuildStateSpans(element, carrier),
            carrier,
            emit,
            collapseToSupport);

    public static IElement SupportOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.Support, collapseToSupport: true);

    public static IElement TensionOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.Tension, collapseToSupport: true);

    public static IElement ExclusiveSupportOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.Exclusive, collapseToSupport: true);

    public static IElement ForwardResidueOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.ForwardResidue);

    public static IElement ReverseResidueOn(IElement element, Carrier carrier) =>
        ProjectOn(element, carrier, SupportReadEquations.ReverseResidue);

    private static IElement BuildProjection(
        IReadOnlyList<SupportStateSpan> states,
        Carrier carrier,
        Func<IElement, IElement> emit,
        bool collapseToSupport)
    {
        var projected = new List<ProjectedSpan>();
        foreach (var state in states)
        {
            var emission = emit(new Relation(Measure.Fixed(state.Reverse), Measure.Fixed(state.Forward)));
            ReadPair(emission, out var reverse, out var forward);

            if (collapseToSupport)
            {
                if (reverse < 0 || forward > 0)
                {
                    projected.Add(new(state.Start, state.End, 1));
                }

                continue;
            }

            if (reverse < 0)
            {
                projected.Add(new(state.Start, state.End, -1));
            }

            if (forward > 0)
            {
                projected.Add(new(state.Start, state.End, 1));
            }
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

    private static void ReadPair(IElement pair, out long reverse, out long forward)
    {
        if (pair is Relation relation &&
            relation.Left is Measure reverseMeasure &&
            relation.Right is Measure forwardMeasure)
        {
            reverse = reverseMeasure.Raw;
            forward = forwardMeasure.Raw;
            return;
        }

        reverse = 0;
        forward = 0;
    }

    private record struct ProjectedSpan(long Start, long End, int Direction);
}
