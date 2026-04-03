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

    public bool IsAtomicPair => IsComposite && First.IsAtomic && Second.IsAtomic;

    public Measure MeasureCount => new(_measures.Length);

    public ImmutableArray<Measure> Measures => _measures;

    public ImmutableArray<Attachment> Inputs => _inputs;

    public Ratio Projection => IsAtomic
        ? Basis.Pin.Project(_measures[0]).Normalize()
        : throw new InvalidOperationException("Only atomic attachments expose a single projection.");

    public Attachment First => IsComposite ? _inputs[0] : throw new InvalidOperationException("Only composite attachments expose a first child.");

    public Attachment Second => IsComposite ? _inputs[1] : throw new InvalidOperationException("Only composite attachments expose a second child.");

    public Measure KernelCode => IsAtomicPair ? Basis.KernelCode() : throw new InvalidOperationException("Only atomic pairs expose a kernel code.");

    public bool IsSameCarrierPair => IsAtomicPair && Basis.IsSameCarrierPair();

    public bool IsAlignedPair => IsAtomicPair && Basis.IsAlignedPair();

    public bool IsOpposedPair => IsAtomicPair && Basis.IsOpposedPair();

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

    public Ratio ResolvedCoefficient()
    {
        if (!IsAtomicPair)
        {
            throw new InvalidOperationException("Only atomic pairs expose a resolved coefficient.");
        }

        return First.Projection.Multiply(Second.Projection);
    }

    public ImmutableArray<Ratio> QuadrantCoefficients()
    {
        if (!IsAtomicPair)
        {
            throw new InvalidOperationException("Only atomic pairs expose ordered quadrant coefficients.");
        }

        var resolved = ResolvedCoefficient();
        var zero = Ratio.Zero(resolved.Denominator);
        var coefficients = new Ratio[4];

        for (var index = 0; index < coefficients.Length; index++)
        {
            coefficients[index] = zero;
        }

        coefficients[(int)KernelCode.Raw] = resolved;
        return coefficients.ToImmutableArray();
    }

    public Measure SegmentStart()
    {
        EnsureOpposedSameCarrierPair();
        return First.Measures[0];
    }

    public Measure SegmentPivot()
    {
        EnsureOpposedSameCarrierPair();
        return First.Basis.Origin;
    }

    public Measure SegmentEnd()
    {
        EnsureOpposedSameCarrierPair();
        return Second.Measures[0];
    }

    public Measure SegmentSpanMeasure()
    {
        EnsureOpposedSameCarrierPair();
        return SegmentEnd() - SegmentStart();
    }

    public Ratio SegmentSpanProjection()
    {
        EnsureOpposedSameCarrierPair();
        return First.Projection.Add(Second.Projection);
    }

    public static Attachment Attach(Basis basis, params Measure[] measures)
    {
        ArgumentNullException.ThrowIfNull(measures);

        return Attach(basis, measures.ToImmutableArray());
    }

    private static Attachment Attach(Basis basis, ImmutableArray<Measure> measures)
    {
        ArgumentNullException.ThrowIfNull(basis);

        if (measures.Length != basis.PinCount.Raw)
        {
            throw new ArgumentException(
                $"Basis expects {basis.PinCount.Raw} measure(s) but received {measures.Length}.",
                nameof(measures));
        }

        if (basis.IsAtomic)
        {
            return new Attachment(basis, measures, ImmutableArray<Attachment>.Empty);
        }

        var inputs = ImmutableArray.CreateBuilder<Attachment>(basis.Inputs.Length);
        var offset = 0;

        foreach (var input in basis.Inputs)
        {
            var count = (int)input.PinCount.Raw;
            inputs.Add(Attach(input, measures.Slice(offset, count)));
            offset += count;
        }

        return new Attachment(basis, measures, inputs.MoveToImmutable());
    }

    private void EnsureOpposedSameCarrierPair()
    {
        if (!IsAtomicPair || !IsSameCarrierPair || !IsOpposedPair || !HasSharedOriginPerCarrier)
        {
            throw new InvalidOperationException("This view only applies to same-carrier opposed atomic pairs with a shared origin.");
        }
    }
}
