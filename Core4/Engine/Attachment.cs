using System.Collections.Immutable;

namespace Core4.Engine;

public sealed class Attachment
{
    private readonly ImmutableArray<Measure> _measures;
    private readonly ImmutableArray<Attachment> _inputs;

    private Attachment(Basis basis, ImmutableArray<Measure> measures, ImmutableArray<Attachment> inputs)
    {
        Basis = basis;
        _measures = measures;
        _inputs = inputs;
    }

    public Basis Basis { get; }

    public bool IsAtomic => Basis.IsAtomic;

    public bool IsComposite => Basis.IsComposite;

    public bool IsAtomicPair => Basis.IsAtomicPair;

    public Measure MeasureCount => new(_measures.Length);

    public ImmutableArray<Measure> Measures => _measures;

    public ImmutableArray<Attachment> Inputs => _inputs;

    public Attachment Recessive => IsComposite ? _inputs[0] : this;

    public Attachment Dominant => IsComposite ? _inputs[1] : this;

    public Ratio Projection => IsAtomic ? Basis.Project(_measures[0]) : Ratio.Tension(Potential);

    public Measure Potential => IsAtomic
        ? Absolute(Projection.Recessive)
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
        var components = ImmutableArray.CreateBuilder<Ratio>(_measures.Length);

        for (var index = 0; index < _measures.Length; index++)
        {
            components.Add(Basis.Pins[index].Project(_measures[index]).Normalize());
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

    public Measure SegmentStart() => IsSegmentPair() ? Recessive.Measures[0] : Measure.Zero;

    public Measure SegmentPivot() => IsSegmentPair() ? Recessive.Basis.AtomicPin.Origin : Measure.Zero;

    public Measure SegmentEnd() => IsSegmentPair() ? Dominant.Measures[0] : Measure.Zero;

    public Measure SegmentSpanMeasure() => IsSegmentPair() ? SegmentEnd() - SegmentStart() : Potential;

    public Ratio SegmentSpanProjection() => IsSegmentPair()
        ? Recessive.Projection.Add(Dominant.Projection)
        : Ratio.Tension(Potential);

    public static Attachment Attach(Basis basis, params Measure[] measures) => Attach(
        basis,
        measures.ToImmutableArray());

    private static Attachment Attach(Basis basis, ImmutableArray<Measure> measures)
    {
        var aligned = AlignMeasures(basis, measures);

        if (basis.IsAtomic)
        {
            return new Attachment(basis, aligned, ImmutableArray<Attachment>.Empty);
        }

        var inputs = ImmutableArray.CreateBuilder<Attachment>(basis.Inputs.Length);
        var offset = 0;

        foreach (var input in basis.Inputs)
        {
            var count = (int)input.PinCount.Raw;
            inputs.Add(Attach(input, aligned.Slice(offset, count)));
            offset += count;
        }

        return new Attachment(basis, aligned, inputs.MoveToImmutable());
    }

    private bool IsSegmentPair() => IsAtomicPair && IsSameCarrierPair && IsOpposedPair && HasSharedOriginPerCarrier;

    private static ImmutableArray<Measure> AlignMeasures(Basis basis, ImmutableArray<Measure> measures)
    {
        var count = (int)basis.PinCount.Raw;
        var aligned = new Measure[count];

        for (var index = 0; index < count; index++)
        {
            aligned[index] = index < measures.Length ? measures[index] : Measure.Zero;
        }

        return aligned.ToImmutableArray();
    }

    private static Measure Absolute(Measure value) => value.Raw < 0 ? -value : value;
}
