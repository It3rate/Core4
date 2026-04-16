using System.Collections.Immutable;
using Core4.Aliases;

namespace Core4.Interpretation;

/// <summary>
/// Interpretation-side behavior over a frozen support scaffold. This keeps <see cref="SupportStructure"/> focused on
/// canonical landmark/coincidence encoding while the read, projection, relation-view, and classification helpers sit
/// with the rest of the support interpretation layer.
/// </summary>
internal static class SupportStructureInterpretation
{
    public static Measure SupportGrade(this SupportStructure structure) => structure.Inputs.IsEmpty
        ? Measure.Zero
        : new Measure(structure.Inputs.Max(static input => input.SupportGrade().Raw) + 1L);

    public static Measure RelationCount(this SupportStructure structure) => new(structure.Spans.Length);

    public static Measure CarrierCount(this SupportStructure structure) => new(structure.SelectCarriers().Length);

    public static Ratio Stiffness(this SupportStructure structure) => structure.IsAtomic
        ? structure.AtomicStiffness()
        : structure.Inputs[0].Stiffness().Weaker(structure.Inputs[1].Stiffness());

    public static ISupportProjection OriginPositionRead(this SupportStructure structure) =>
        new SupportProjectionRead(structure.OriginPosition());

    public static Ratio OriginPosition(this SupportStructure structure) => structure.AtomicSpan().OriginPosition;

    public static ISupportProjection AnchorPositionRead(this SupportStructure structure) =>
        new SupportProjectionRead(structure.AnchorPosition());

    public static Ratio AnchorPosition(this SupportStructure structure) => structure.AtomicSpan().AnchorPosition;

    public static ISupportProjection SpanRead(this SupportStructure structure) =>
        new SupportProjectionRead(structure.Span());

    public static Ratio Span(this SupportStructure structure) => structure.AtomicSpan().Span();

    public static Measure Delta(this SupportStructure structure) => structure.Span().Recessive;

    public static Measure Potential(this SupportStructure structure) => structure.IsAtomic
        ? structure.AtomicPotential()
        : structure.IsSameCarrierPair() && structure.IsOpposedPair() && structure.HasSharedOriginPerCarrier()
            ? structure.Inputs[0].Potential() + structure.Inputs[1].Potential()
            : new Measure(checked(structure.Inputs[0].Potential().Raw * structure.Inputs[1].Potential().Raw));

    public static ISupportProjection FoldRead(this SupportStructure structure) => structure.IsComposite
        ? new SupportProjectionRead(new Ratio(structure.Inputs[0].Potential(), structure.Inputs[1].Potential()).Normalize())
        : new SupportProjectionRead(Ratio.Tension(structure.Potential()));

    public static Measure Orientation(this SupportStructure structure) =>
        structure.IsAtomic ? structure.AtomicOrientation() : Measure.Zero;

    public static bool IsRecessive(this SupportStructure structure) =>
        structure.IsAtomic && structure.Orientation().Raw < 0;

    public static bool IsDominant(this SupportStructure structure) =>
        structure.IsAtomic && structure.Orientation().Raw > 0;

    public static Relation AtomicRelation(this SupportStructure structure) => RelationFromSpan(structure.AtomicSpan());

    public static ISupportProjection LocateRead(this SupportStructure structure, ISupportProjection position, Carrier carrier) =>
        new SupportProjectionRead(structure.Locate(position.AsRatio(), carrier));

    public static Ratio Locate(this SupportStructure structure, ISupportProjection position, Carrier carrier) =>
        structure.Locate(position.AsRatio(), carrier);

    public static Ratio Locate(this SupportStructure structure, Ratio position, Carrier carrier)
    {
        var normalized = position.Normalize();

        if (structure.IsAtomic)
        {
            return structure.LocateAtomic(normalized, carrier);
        }

        if (structure.IsAtomicPair() && structure.IsSameCarrierPair() && structure.IsOpposedPair() && structure.HasSharedOriginPerCarrier())
        {
            return structure.LocateDirectedSegment(normalized, carrier);
        }

        return Ratio.Tension(normalized.Potential);
    }

    public static ImmutableArray<Carrier> SelectCarriers(this SupportStructure structure)
    {
        var carriers = ImmutableArray.CreateBuilder<Carrier>();

        foreach (var span in structure.Spans)
        {
            if (!carriers.Contains(span.Carrier))
            {
                carriers.Add(span.Carrier);
            }
        }

        return carriers.ToImmutable();
    }

    public static ImmutableArray<Relation> SelectRelations(this SupportStructure structure)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>(structure.Spans.Length);

        foreach (var span in structure.Spans)
        {
            relations.Add(RelationFromSpan(span));
        }

        return relations.MoveToImmutable();
    }

    public static ImmutableArray<Relation> SelectRelations(this SupportStructure structure, Carrier carrier)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var span in structure.Spans)
        {
            if (span.Carrier == carrier)
            {
                relations.Add(RelationFromSpan(span));
            }
        }

        return relations.ToImmutable();
    }

    public static ImmutableArray<Site> DefaultAttachmentSites(this SupportStructure structure)
    {
        var sites = ImmutableArray.CreateBuilder<Site>(structure.Spans.Length);

        foreach (var span in structure.Spans)
        {
            sites.Add(Site.Open(span.Carrier.At(span.OriginPosition)));
        }

        return sites.MoveToImmutable();
    }

    public static bool HasSharedOriginPerCarrier(this SupportStructure structure)
    {
        foreach (var carrier in structure.SelectCarriers())
        {
            var origin = default(Ratio);
            var hasOrigin = false;

            foreach (var span in structure.Spans)
            {
                if (span.Carrier != carrier)
                {
                    continue;
                }

                if (!hasOrigin)
                {
                    origin = span.OriginPosition.Normalize();
                    hasOrigin = true;
                    continue;
                }

                if (span.OriginPosition.Normalize() != origin)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool HasSharedOriginAt(this SupportStructure structure, Site site, Carrier carrier)
    {
        var sitePosition = site.On(carrier).Normalize();

        foreach (var span in structure.Spans)
        {
            if (span.OriginPosition.Normalize() != sitePosition)
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasRecessiveCalibration(this SupportStructure structure) => structure.IsAtomic
        ? structure.IsRecessive()
        : structure.IsAtomicPair() && structure.Inputs[0].IsAtomic && structure.Inputs[0].IsRecessive();

    public static bool HasDominantCalibration(this SupportStructure structure) => structure.IsAtomic
        ? structure.IsDominant()
        : structure.IsAtomicPair() && structure.Inputs[1].IsAtomic && structure.Inputs[1].IsDominant();

    public static bool IsBilateralAt(this SupportStructure structure, Site site, Carrier carrier) => structure.IsAtomicPair()
        && structure.IsSameCarrierPair()
        && structure.IsOpposedPair()
        && structure.HasSharedOriginAt(site, carrier)
        && structure.HasRecessiveCalibration()
        && structure.HasDominantCalibration();

    public static bool IsAtomicPair(this SupportStructure structure) => structure.Inputs.Length == 2
        && structure.Inputs[0].Inputs.IsEmpty
        && structure.Inputs[1].Inputs.IsEmpty
        && structure.Inputs[0].Spans.Length == 1
        && structure.Inputs[1].Spans.Length == 1;

    public static bool IsSameCarrierPair(this SupportStructure structure) => structure.IsAtomicPair()
        && structure.Inputs[0].Spans[0].Carrier == structure.Inputs[1].Spans[0].Carrier;

    public static bool IsAlignedPair(this SupportStructure structure) => structure.IsAtomicPair()
        && structure.Inputs[0].Spans[0].Orientation().Raw != 0
        && structure.Inputs[0].Spans[0].Orientation() == structure.Inputs[1].Spans[0].Orientation();

    public static bool IsOpposedPair(this SupportStructure structure) => structure.IsAtomicPair()
        && structure.Inputs[0].Spans[0].Orientation().Raw != 0
        && structure.Inputs[0].Spans[0].Orientation() == -structure.Inputs[1].Spans[0].Orientation();

    public static Measure KernelCode(this SupportStructure structure)
    {
        if (!structure.IsAtomicPair())
        {
            return Measure.Zero;
        }

        var recessive = SideCode(structure.Inputs[0].Spans[0].Orientation());
        var dominant = SideCode(structure.Inputs[1].Spans[0].Orientation());

        if (recessive < 0 || dominant < 0)
        {
            return Measure.Zero;
        }

        return new Measure((recessive << 1) | dominant);
    }

    public static ISupportProjection ProjectRead(this SupportStructure structure, Site site)
    {
        if (!structure.IsAtomic || !structure.TryGetAtomicSpan(out var span))
        {
            return new SupportProjectionRead(Ratio.Tension(site.Potential));
        }

        return new SupportProjectionRead(ProjectSpan(span, site));
    }

    public static ISupportProjection ProjectRead(this SupportStructure structure, Measure measure)
    {
        if (!structure.IsAtomic || structure.Spans.IsEmpty)
        {
            return new SupportProjectionRead(Ratio.Tension(measure));
        }

        return structure.ProjectRead(Site.Open(structure.Spans[0].Carrier, measure));
    }

    public static ImmutableArray<ISupportProjection> ProjectComponentReads(this SupportStructure structure, ImmutableArray<Site> sites)
    {
        var projections = ImmutableArray.CreateBuilder<ISupportProjection>(structure.Spans.Length);
        var defaults = structure.DefaultAttachmentSites();

        for (var index = 0; index < structure.Spans.Length; index++)
        {
            var site = index < sites.Length ? sites[index] : defaults[index];
            projections.Add(new SupportProjectionRead(ProjectSpan(structure.Spans[index], site)));
        }

        return projections.MoveToImmutable();
    }

    public static SupportStructure Flip(this SupportStructure structure)
    {
        if (structure.IsAtomic)
        {
            var span = structure.AtomicSpan();
            return SupportStructure.FromSpan(span.Carrier, span.AnchorPosition, span.OriginPosition, span.Commitment);
        }

        var flipped = SupportStructure.Fuse(structure.Inputs[0].Flip(), structure.Inputs[1].Flip());

        for (var index = 2; index < structure.Inputs.Length; index++)
        {
            flipped = SupportStructure.Fuse(flipped, structure.Inputs[index].Flip());
        }

        return flipped;
    }

    private static Ratio LocateAtomic(this SupportStructure structure, Ratio position, Carrier carrier)
    {
        if (!structure.TryGetAtomicSpan(out var span))
        {
            return carrier == structure.PrimaryCarrier
                ? position.Normalize()
                : Ratio.Tension(position.Potential);
        }

        return carrier == span.Carrier
            ? span.OriginPosition.Add(span.Span().Multiply(position)).Normalize()
            : Ratio.Tension(position.Potential);
    }

    private static Ratio LocateDirectedSegment(this SupportStructure structure, Ratio position, Carrier carrier)
    {
        var left = structure.Inputs[0].Spans[0];
        var right = structure.Inputs[1].Spans[0];

        if (left.Carrier != carrier || right.Carrier != carrier)
        {
            return Ratio.Tension(position.Potential);
        }

        return left.AnchorPosition.Add(right.AnchorPosition.Subtract(left.AnchorPosition).Multiply(position)).Normalize();
    }

    private static Measure AtomicPotential(this SupportStructure structure) =>
        structure.TryGetAtomicSpan(out var span) ? span.Potential() : Measure.Zero;

    private static Measure AtomicOrientation(this SupportStructure structure) =>
        structure.TryGetAtomicSpan(out var span) ? span.Orientation() : Measure.Zero;

    private static Ratio AtomicStiffness(this SupportStructure structure) =>
        structure.TryGetAtomicSpan(out var span) ? span.Commitment : Ratio.Zero(Measure.One);

    private static bool TryGetAtomicSpan(this SupportStructure structure, out SupportSpan span)
    {
        if (!structure.Spans.IsEmpty)
        {
            span = structure.Spans[0];
            return true;
        }

        span = default;
        return false;
    }

    private static SupportSpan AtomicSpan(this SupportStructure structure) => structure.Spans.IsEmpty
        ? throw new InvalidOperationException("This support does not expose an atomic span.")
        : structure.Spans[0];

    private static Ratio ProjectSpan(SupportSpan span, Site site)
    {
        var sitePosition = site.On(span.Carrier).Normalize();
        var delta = sitePosition.Subtract(span.OriginPosition);
        return delta.Divide(span.Span());
    }

    private static Relation RelationFromSpan(SupportSpan span) => new(
        span.Carrier,
        Site.Open(span.Carrier.At(span.OriginPosition)).Harden(span.Commitment),
        Site.Open(span.Carrier.At(span.AnchorPosition)).Harden(span.Commitment));

    private static long SideCode(Measure orientation)
    {
        if (orientation.Raw < 0)
        {
            return 0L;
        }

        if (orientation.Raw > 0)
        {
            return 1L;
        }

        return -1L;
    }

    private static Ratio Span(this SupportSpan span) => span.AnchorPosition.Subtract(span.OriginPosition).Normalize();

    private static Measure Potential(this SupportSpan span) => span.Span().Potential;

    private static Measure Orientation(this SupportSpan span) => span.Span().Recessive.Sign;
}
