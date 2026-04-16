using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A basis-wide binding of live sites into a frozen basis. This is not a single local place; it is a read of one
/// whole basis through one site per atomic relation slot. Atomic attachments expose a direct local projection through
/// their basis; composite attachments also expose a higher-level fold that compares their ordered child reads without
/// pretending the whole object is itself one atomic relation.
/// Example: <c>var reading = basis.Attach(5, 4);</c>
/// </summary>
public sealed class Attachment : ISupportFold
{
    private readonly ImmutableArray<Site> _sites;
    private readonly ImmutableArray<Attachment> _inputs;

    private Attachment(Basis basis, ImmutableArray<Site> sites, ImmutableArray<Attachment> inputs)
    {
        Basis = basis;
        _sites = sites;
        _inputs = inputs;
    }

    public Basis Basis { get; }

    public bool IsAtomic => Basis.IsAtomic;

    public bool IsComposite => Basis.IsComposite;

    public bool IsAtomicPair => Basis.IsAtomicPair;

    public Measure SiteCount => new(_sites.Length);

    public ImmutableArray<Site> Sites => _sites;

    public ImmutableArray<Attachment> Inputs => _inputs;

    public Attachment Recessive => IsComposite ? _inputs[0] : this;

    public Attachment Dominant => IsComposite ? _inputs[1] : this;

    public ImmutableArray<Measure> Measures => _sites.Select(site => site.Location).ToImmutableArray();

    public ImmutableArray<Placement> Placements => _sites.Select(site => site.Placement).ToImmutableArray();

    // Compatibility projection for callers that still expect a single ratio-shaped read. Composite attachments do not
    // have one atomic projection, so they return unresolved tension and callers should prefer Fold or ComponentProjections.
    public Ratio Projection => TryGetProjection(out var projection)
        ? projection
        : Ratio.Tension(Potential);

    public Measure Potential => IsAtomic
        ? Absolute(Projection.Recessive + _sites[0].Potential)
        : IsSameCarrierPair && IsOpposedPair && HasSharedOriginPerCarrier
            ? Recessive.Potential + Dominant.Potential
            : new Measure(checked(Recessive.Potential.Raw * Dominant.Potential.Raw));

    public Ratio Fold => IsComposite
        ? new Ratio(Recessive.Potential, Dominant.Potential).Normalize()
        : Projection;

    // Compatibility alias while callers move from ratio-as-primitive wording toward fold/support reads.
    public Ratio Ratio => Fold;

    public Measure KernelCode => Basis.KernelCode();

    public bool IsSameCarrierPair => Basis.IsSameCarrierPair();

    public bool IsAlignedPair => Basis.IsAlignedPair();

    public bool IsOpposedPair => Basis.IsOpposedPair();

    public bool IsSelfPair => IsSameCarrierPair && IsAlignedPair;

    public bool HasSharedOriginPerCarrier => Basis.HasSharedOriginPerCarrier();

    public bool TryGetProjection(out Ratio projection)
    {
        if (IsAtomic)
        {
            projection = Basis.Project(_sites[0]).Normalize();
            return true;
        }

        projection = Ratio.Tension(Potential);
        return false;
    }

    public ImmutableArray<Ratio> ComponentProjections()
    {
        var components = ImmutableArray.CreateBuilder<Ratio>(_sites.Length);

        for (var index = 0; index < _sites.Length; index++)
        {
            components.Add(Basis.Relations[index].Project(_sites[index]));
        }

        return components.MoveToImmutable();
    }

    public ImmutableArray<Ratio> Components() => ComponentProjections();

    public Ratio ResolvedCoefficient() => IsAtomicPair
        ? Recessive.Projection.Multiply(Dominant.Projection)
        : Ratio.Tension(Potential);

    public ImmutableArray<Ratio> QuadrantCoefficients()
    {
        var resolved = ResolvedCoefficient();
        var zero = Ratio.Zero(resolved.Dominant);
        var coefficients = new Ratio[4];

        for (var index = 0; index < coefficients.Length; index++)
        {
            coefficients[index] = zero;
        }

        var kernel = KernelCode.Raw;
        var slot = kernel >= 0 && kernel < coefficients.Length ? (int)kernel : 0;
        coefficients[slot] = resolved;
        return coefficients.ToImmutableArray();
    }

    public Measure SegmentStart() => IsSegmentPair() ? Recessive.Sites[0].Location : Measure.Zero;

    public Measure SegmentPivot() => IsSegmentPair() ? Recessive.Basis.AtomicRelation.Origin.Location : Measure.Zero;

    public Measure SegmentEnd() => IsSegmentPair() ? Dominant.Sites[0].Location : Measure.Zero;

    public Measure SegmentSpanMeasure() => IsSegmentPair() ? SegmentEnd() - SegmentStart() : Potential;

    public Ratio SegmentSpanProjection() => IsSegmentPair()
        ? Recessive.Projection.Add(Dominant.Projection)
        : Ratio.Tension(Potential);

    public static Attachment Attach(Basis basis, params Site[] sites) => Attach(basis, sites.ToImmutableArray());

    public static Attachment Attach(Basis basis, params Measure[] measures)
    {
        var relations = basis.SelectRelations();
        var sites = new Site[measures.Length];

        for (var index = 0; index < measures.Length; index++)
        {
            var carrier = index < relations.Length ? relations[index].Carrier : new Carrier(0);
            sites[index] = Site.Open(carrier, measures[index]);
        }

        return Attach(basis, sites.ToImmutableArray());
    }

    private static Attachment Attach(Basis basis, ImmutableArray<Site> sites)
    {
        var aligned = AlignSites(basis, sites);

        if (basis.IsAtomic)
        {
            return new Attachment(basis, aligned, ImmutableArray<Attachment>.Empty);
        }

        var inputs = ImmutableArray.CreateBuilder<Attachment>(basis.Inputs.Length);
        var offset = 0;

        foreach (var input in basis.Inputs)
        {
            var count = (int)input.RelationCount.Raw;
            inputs.Add(Attach(input, aligned.Slice(offset, count)));
            offset += count;
        }

        return new Attachment(basis, aligned, inputs.MoveToImmutable());
    }

    private bool IsSegmentPair() => IsAtomicPair && IsSameCarrierPair && IsOpposedPair && HasSharedOriginPerCarrier;

    private static ImmutableArray<Site> AlignSites(Basis basis, ImmutableArray<Site> sites)
    {
        var count = (int)basis.RelationCount.Raw;
        var aligned = new Site[count];

        for (var index = 0; index < count; index++)
        {
            if (index < sites.Length)
            {
                aligned[index] = sites[index];
                continue;
            }

            var relation = basis.Relations[index];
            aligned[index] = Site.Open(relation.Carrier, relation.Origin.Location);
        }

        return aligned.ToImmutableArray();
    }

    private static Measure Absolute(Measure value) => value.Raw < 0 ? -value : value;
}
