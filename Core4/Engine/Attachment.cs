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

    public Measure MeasureCount => new(_measures.Length);

    public ImmutableArray<Measure> Measures => _measures;

    public ImmutableArray<Attachment> Inputs => _inputs;

    public Ratio Projection => IsAtomic
        ? Basis.Pin.Project(_measures[0]).Normalize()
        : throw new InvalidOperationException("Only atomic attachments expose a single projection.");

    public Attachment First => IsComposite ? _inputs[0] : throw new InvalidOperationException("Only composite attachments expose a first child.");

    public Attachment Second => IsComposite ? _inputs[1] : throw new InvalidOperationException("Only composite attachments expose a second child.");

    public ImmutableArray<Ratio> Components()
    {
        var components = ImmutableArray.CreateBuilder<Ratio>(_measures.Length);

        for (var index = 0; index < _measures.Length; index++)
        {
            components.Add(Basis.Pins[index].Project(_measures[index]).Normalize());
        }

        return components.MoveToImmutable();
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
}
