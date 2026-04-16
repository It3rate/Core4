using System.Collections.Immutable;
using Core4.Aliases;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A frozen measuring scaffold with bound live samples. This is not a single local place; it is a read of one whole
/// basis through one site per atomic relation slot. The attachment itself stores only the reusable ruler and the
/// aligned sample group; any child attachments are derived from that pair so the live binding stays one shared
/// mechanism instead of a second parallel tree. Atomic attachments expose a direct local projection through their
/// basis; composite attachments also expose a higher-level fold that compares their ordered child reads without
/// pretending the whole object is itself one atomic relation.
/// Example: <c>var reading = basis.Attach(5, 4);</c>
/// </summary>
public sealed class Attachment : ISupportFold
{
    private readonly SampleSet _samples;

    private Attachment(Basis basis, SampleSet samples)
    {
        Basis = basis;
        _samples = samples;
    }

    public Basis Basis { get; }

    public SampleSet Samples => _samples;

    internal SupportStructure Structure => Basis.Structure;

    public bool IsAtomic => Structure.IsAtomic;

    public bool IsComposite => Structure.IsComposite;

    public bool IsAtomicPair => Structure.IsAtomicPair();

    public Measure SiteCount => Samples.SiteCount;

    public ImmutableArray<Site> Sites => Samples.Sites;

    public ImmutableArray<Attachment> Inputs => BuildInputs();

    public Attachment Recessive => IsComposite ? Inputs[0] : this;

    public Attachment Dominant => IsComposite ? Inputs[1] : this;

    public ImmutableArray<Measure> Measures => Samples.Measures;

    public ImmutableArray<Placement> Placements => Samples.Placements;

    public SampleSet SelectSamples(Carrier carrier) => Samples.Select(carrier);

    // Compatibility projection for callers that still expect a single ratio-shaped read. Composite attachments do not
    // have one atomic projection, so they return unresolved tension and callers should prefer Fold or ComponentProjections.
    public ISupportProjection ProjectionRead => TryGetProjectionRead(out var projection)
        ? projection
        : new SupportProjectionRead(Ratio.Tension(Potential));

    // TODO(core): Delete this compatibility alias once callers consume ProjectionRead directly.
    public Ratio Projection => ProjectionRead.AsRatio();

    public Measure Potential => IsAtomic
        ? Absolute(Projection.Recessive + Sites[0].Potential)
        : IsSameCarrierPair && IsOpposedPair && HasSharedOriginPerCarrier
            ? Recessive.Potential + Dominant.Potential
            : new Measure(checked(Recessive.Potential.Raw * Dominant.Potential.Raw));

    public ISupportProjection FoldRead => IsComposite
        ? new SupportProjectionRead(new Ratio(Recessive.Potential, Dominant.Potential).Normalize())
        : ProjectionRead;

    // TODO(core): Delete this compatibility alias once callers consume FoldRead directly.
    public Ratio Fold => FoldRead.AsRatio();

    // TODO(core): Delete this compatibility alias once callers stop expecting attachment-wide Ratio.
    public Ratio Ratio => Fold;

    public Measure KernelCode => Structure.KernelCode();

    public bool IsSameCarrierPair => Structure.IsSameCarrierPair();

    public bool IsAlignedPair => Structure.IsAlignedPair();

    public bool IsOpposedPair => Structure.IsOpposedPair();

    public bool IsSelfPair => IsSameCarrierPair && IsAlignedPair;

    public bool HasSharedOriginPerCarrier => Structure.HasSharedOriginPerCarrier();

    public bool TryGetProjectionRead(out ISupportProjection projection)
    {
        if (IsAtomic)
        {
            projection = Structure.ProjectRead(Sites[0]);
            return true;
        }

        projection = new SupportProjectionRead(Ratio.Tension(Potential));
        return false;
    }

    // TODO(core): Delete this compatibility alias once callers consume TryGetProjectionRead directly.
    public bool TryGetProjection(out Ratio projection)
    {
        var success = TryGetProjectionRead(out var read);
        projection = read.AsRatio();
        return success;
    }

    public ImmutableArray<ISupportProjection> ComponentProjectionReads()
        => Structure.ProjectComponentReads(Sites);

    // TODO(core): Delete this compatibility alias once callers consume ComponentProjectionReads directly.
    public ImmutableArray<Ratio> ComponentProjections() => ComponentProjectionReads()
        .Select(static projection => projection.AsRatio())
        .ToImmutableArray();

    // TODO(core): Delete this compatibility alias once callers stop expecting ratio-only component reads.
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

    public Measure SegmentPivot() => IsSegmentPair() ? Recessive.Structure.AtomicRelation.Origin.Location : Measure.Zero;

    public Measure SegmentEnd() => IsSegmentPair() ? Dominant.Sites[0].Location : Measure.Zero;

    public Measure SegmentSpanMeasure() => IsSegmentPair() ? SegmentEnd() - SegmentStart() : Potential;

    public Ratio SegmentSpanProjection() => IsSegmentPair()
        ? Recessive.Projection.Add(Dominant.Projection)
        : Ratio.Tension(Potential);

    public static Attachment Attach(Basis basis, params Site[] sites) => Attach(basis, SampleSet.Collect(sites));

    public static Attachment Attach(Basis basis, params Measure[] measures)
    {
        var defaults = basis.Structure.DefaultAttachmentSites();
        var sites = new Site[measures.Length];

        for (var index = 0; index < measures.Length; index++)
        {
            var carrier = index < defaults.Length ? defaults[index].Carrier : new Carrier(0);
            sites[index] = Site.Open(carrier, measures[index]);
        }

        return Attach(basis, SampleSet.Collect(sites));
    }

    public static Attachment Attach(Basis basis, SampleSet samples)
    {
        var structure = basis.Structure;
        var aligned = AlignSamples(structure, samples);
        return new Attachment(basis, aligned);
    }

    private bool IsSegmentPair() => IsAtomicPair && IsSameCarrierPair && IsOpposedPair && HasSharedOriginPerCarrier;

    private static SampleSet AlignSamples(SupportStructure structure, SampleSet samples)
    {
        if (structure.IsAtomic)
        {
            return AlignAtomicSamples(structure, samples.Sites);
        }

        if (samples.IsComposite && samples.Inputs.Length == structure.Inputs.Length)
        {
            var alignedInputs = ImmutableArray.CreateBuilder<SampleSet>(structure.Inputs.Length);

            for (var index = 0; index < structure.Inputs.Length; index++)
            {
                alignedInputs.Add(AlignSamples(structure.Inputs[index], samples.Inputs[index]));
            }

            return SampleSet.Compose(alignedInputs.MoveToImmutable());
        }

        return AlignSampleSlices(structure, samples.Sites);
    }

    private static SampleSet AlignAtomicSamples(SupportStructure structure, ImmutableArray<Site> sites)
    {
        var count = (int)structure.RelationCount.Raw;
        var aligned = new Site[count];
        var defaults = structure.DefaultAttachmentSites();

        for (var index = 0; index < count; index++)
        {
            if (index < sites.Length)
            {
                aligned[index] = sites[index];
                continue;
            }

            aligned[index] = defaults[index];
        }

        return SampleSet.Collect(aligned.ToImmutableArray());
    }

    private static SampleSet AlignSampleSlices(SupportStructure structure, ImmutableArray<Site> sites)
    {
        if (structure.IsAtomic)
        {
            return AlignAtomicSamples(structure, sites);
        }

        var inputs = ImmutableArray.CreateBuilder<SampleSet>(structure.Inputs.Length);
        var offset = 0;

        foreach (var input in structure.Inputs)
        {
            var count = (int)input.RelationCount.Raw;
            var available = offset < sites.Length
                ? sites.Slice(offset, Math.Min(count, sites.Length - offset))
                : ImmutableArray<Site>.Empty;
            inputs.Add(AlignSampleSlices(input, available));
            offset += count;
        }

        return SampleSet.Compose(inputs.MoveToImmutable());
    }

    private ImmutableArray<Attachment> BuildInputs()
    {
        if (IsAtomic)
        {
            return ImmutableArray<Attachment>.Empty;
        }

        var inputs = ImmutableArray.CreateBuilder<Attachment>(Structure.Inputs.Length);

        for (var index = 0; index < Structure.Inputs.Length; index++)
        {
            inputs.Add(new Attachment(new Basis(Structure.Inputs[index]), Samples.Inputs[index]));
        }

        return inputs.MoveToImmutable();
    }

    private static Measure Absolute(Measure value) => value.Raw < 0 ? -value : value;
}
