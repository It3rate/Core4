using System.Collections.Immutable;

namespace Core4.Engine;

public sealed class Attachment
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

    public Ratio Projection => IsAtomic ? Basis.Project(_sites[0]) : Ratio.Tension(Potential);

    public Measure Potential => IsAtomic
        ? Absolute(Projection.Recessive + _sites[0].Potential)
        : IsSameCarrierPair && IsOpposedPair && HasSharedOriginPerCarrier
            ? Recessive.Potential + Dominant.Potential
            : new Measure(checked(Recessive.Potential.Raw * Dominant.Potential.Raw));

    public Ratio Ratio => IsComposite
        ? new Ratio(Recessive.Potential, Dominant.Potential).Normalize()
        : Ratio.Tension(Potential);

    public Measure KernelCode => Basis.KernelCode();

    public bool IsSameCarrierPair => Basis.IsSameCarrierPair();

    public bool IsAlignedPair => Basis.IsAlignedPair();

    public bool IsOpposedPair => Basis.IsOpposedPair();

    public bool IsSelfPair => IsSameCarrierPair && IsAlignedPair;

    public bool HasSharedOriginPerCarrier => Basis.HasSharedOriginPerCarrier();

    public ImmutableArray<Ratio> Components()
    {
        var components = ImmutableArray.CreateBuilder<Ratio>(_sites.Length);

        for (var index = 0; index < _sites.Length; index++)
        {
            components.Add(Basis.Relations[index].Project(_sites[index]));
        }

        return components.MoveToImmutable();
    }

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
