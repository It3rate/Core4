using System.Collections.Immutable;
using Core4.Engine;

namespace Core4.Sessions;

public static class AttachmentExamples
{
    public static ImmutableArray<(string Name, Attachment Attachment)> CreateAtomic()
    {
        var bases = GradeOneExamples.CreateAtomic();

        return
        [
            (bases[0].Name, bases[0].Basis.Attach(7)),
            (bases[1].Name, bases[1].Basis.Attach(107))
        ];
    }

    public static ImmutableArray<(string Name, Attachment Attachment)> Create()
    {
        var bases = GradeOneExamples.Create();

        return
        [
            (bases[0].Name, bases[0].Basis.Attach(7, 14)),
            (bases[1].Name, bases[1].Basis.Attach(-2, 5)),
            (bases[2].Name, bases[2].Basis.Attach(-2, -3)),
            (bases[3].Name, bases[3].Basis.Attach(-2, 4)),
            (bases[4].Name, bases[4].Basis.Attach(5, -3)),
            (bases[5].Name, bases[5].Basis.Attach(5, 4))
        ];
    }

    public static ImmutableArray<(string Name, Attachment Attachment)> CreateShiftedOrigins()
    {
        var bases = GradeOneExamples.CreateShiftedOrigins();

        return
        [
            (bases[0].Name, bases[0].Basis.Attach(107, 114)),
            (bases[1].Name, bases[1].Basis.Attach(98, 105)),
            (bases[2].Name, bases[2].Basis.Attach(98, 297)),
            (bases[3].Name, bases[3].Basis.Attach(98, 304)),
            (bases[4].Name, bases[4].Basis.Attach(105, 297)),
            (bases[5].Name, bases[5].Basis.Attach(105, 304))
        ];
    }

    public static ImmutableArray<string> Describe()
    {
        var attachments = Create();
        var descriptions = ImmutableArray.CreateBuilder<string>(attachments.Length);

        foreach (var (name, attachment) in attachments)
        {
            descriptions.Add($"{name}: {Describe(attachment)}");
        }

        return descriptions.MoveToImmutable();
    }

    public static ImmutableArray<string> DescribeAtomic()
    {
        var attachments = CreateAtomic();
        var descriptions = ImmutableArray.CreateBuilder<string>(attachments.Length);

        foreach (var (name, attachment) in attachments)
        {
            descriptions.Add($"{name}: {Describe(attachment)}");
        }

        return descriptions.MoveToImmutable();
    }

    public static ImmutableArray<string> DescribeShiftedOrigins()
    {
        var attachments = CreateShiftedOrigins();
        var descriptions = ImmutableArray.CreateBuilder<string>(attachments.Length);

        foreach (var (name, attachment) in attachments)
        {
            descriptions.Add($"{name}: {Describe(attachment)}");
        }

        return descriptions.MoveToImmutable();
    }

    private static string Describe(Attachment attachment)
    {
        var components = attachment.Components();
        var componentText = string.Join(
            ", ",
            components.Select(component =>
                $"{component.Normalize()} sign={component.Sign.Raw} whole={component.Whole.Raw} rem={component.Remainder.Raw} approx={component.Approximate:0.###}"));
        var siteText = string.Join(
            ", ",
            attachment.Sites.Select(site =>
                $"pos={site.Position.Normalize()} on={site.On(site.Carrier).Normalize()} slots=[{string.Join(", ", site.Field.Slots().Select(slot => slot.Normalize().ToString()))}]"));

        return
            $"measures=[{string.Join(", ", attachment.Measures.Select(measure => measure.Raw))}], " +
            $"sites=[{siteText}], " +
            $"potential={attachment.Potential.Raw}, ratio={attachment.Ratio.Normalize()}, projection={attachment.Projection.Normalize()}, " +
            $"components=[{componentText}], " +
            $"{DescribeComposite(attachment)}";
    }

    private static string DescribeComposite(Attachment attachment)
    {
        if (!attachment.IsAtomicPair)
        {
            return "composite=atomic";
        }

        if (attachment.IsSelfPair)
        {
            return $"self[coefficient={attachment.ResolvedCoefficient().Normalize()}]";
        }

        if (attachment.IsSameCarrierPair && attachment.IsOpposedPair && attachment.HasSharedOriginPerCarrier)
        {
            var span = attachment.SegmentSpanProjection();

            return
                $"segment[start={attachment.SegmentStart().Raw}, pivot={attachment.SegmentPivot().Raw}, end={attachment.SegmentEnd().Raw}, " +
                $"span={attachment.SegmentSpanMeasure().Raw}, projection={span.Normalize()}]";
        }

        var quadrants = attachment.QuadrantCoefficients();

        return
            $"quadrants[rr={quadrants[0].Normalize()}, rd={quadrants[1].Normalize()}, dr={quadrants[2].Normalize()}, dd={quadrants[3].Normalize()}]";
    }
}
