using System.Collections;
using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Aliases;

/// <summary>
/// A thin alias over one positive-only frozen unit whose ticks are seeded from an ordered enum.
/// The enum supplies names and sample count; the mechanics remain ordinary Core4 carrier, site, relation, and basis work.
/// Example: <c>var phases = new EnumerableUnit&lt;TEnum&gt;(new Carrier(-4107)); var settled = phases.PositionAt(6);</c>
/// </summary>
public sealed class EnumerableUnit<TEnum> : IEnumerable<TEnum> where TEnum : struct, Enum
{
    private readonly ImmutableArray<TEnum> _samples = [.. Enum.GetValues<TEnum>()];
    private long _currentIndex = -1L;

    public EnumerableUnit(Carrier carrier, string? name = null)
    {
        Carrier = carrier;
        Name = name ?? typeof(TEnum).Name;
        TickCount = new Measure(_samples.Length);
        Unit = Basis.Freeze(carrier, Measure.Zero, TickCount);
    }

    public string Name { get; }

    public Carrier Carrier { get; }

    public Basis Unit { get; }

    public Measure TickCount { get; }

    public Measure LastTick => new(TickCount.Raw - 1);

    public ImmutableArray<TEnum> Samples => _samples;

    public Measure CurrentIndex => new(_currentIndex);

    public bool HasCurrent => _currentIndex >= 0L && _currentIndex < _samples.Length;

    public TEnum Current => HasCurrent ? _samples[(int)_currentIndex] : default;

    public string SampleName(TEnum sample) => sample.ToString();

    public string NameOf(TEnum sample) => sample.ToString();

    public bool MoveNext(Measure? stepTicks = null)
    {
        var step = StepMagnitude(stepTicks ?? Measure.One);

        if (_samples.IsEmpty || step == Measure.Zero)
        {
            _currentIndex = -1L;
            return false;
        }

        if (_currentIndex < 0L)
        {
            _currentIndex = 0L;
            return true;
        }

        var next = checked(_currentIndex + step.Raw);

        if (next >= _samples.Length)
        {
            _currentIndex = _samples.Length;
            return false;
        }

        _currentIndex = next;
        return true;
    }

    public bool MovePrevious(Measure? stepTicks = null)
    {
        var step = StepMagnitude(stepTicks ?? Measure.One);

        if (_samples.IsEmpty || step == Measure.Zero)
        {
            _currentIndex = -1L;
            return false;
        }

        if (_currentIndex >= _samples.Length)
        {
            _currentIndex = _samples.Length - 1L;
            return true;
        }

        var next = checked(_currentIndex - step.Raw);

        if (next < 0L)
        {
            _currentIndex = -1L;
            return false;
        }

        _currentIndex = next;
        return true;
    }

    public void Reset() => _currentIndex = -1L;

    public bool MoveTo(TEnum sample)
    {
        var index = _samples.IndexOf(sample);

        if (index < 0)
        {
            _currentIndex = -1L;
            return false;
        }

        _currentIndex = index;
        return true;
    }

    public bool MoveTo(Measure index)
    {
        _currentIndex = index.Raw;
        return HasCurrent;
    }

    public bool TrySampleAt(Measure index, out TEnum sample)
    {
        if (index.Raw >= 0L && index.Raw < _samples.Length)
        {
            sample = _samples[(int)index.Raw];
            return true;
        }

        sample = default;
        return false;
    }

    public TraversalSignature SignatureAt(Measure index, Measure? perspective = null)
    {
        var carried = PositionAt(index);
        var direction = perspective ?? Measure.Zero;
        return TraversalSignature.FromPlacement(
            PlacementAt(index),
            direction,
            carried,
            TraversalSignature.ProvenanceFor(direction, carried.Potential == Measure.Zero ? Measure.One : carried.Potential));
    }

    public bool TryObserve(Measure index, out TraversalObservation observation, Measure? perspective = null)
    {
        if (index.Raw < 0L || index.Raw >= _samples.Length)
        {
            observation = default;
            return false;
        }

        var direction = perspective ?? Measure.Zero;
        var source = SignatureAt(index, direction);
        var continuations = ImmutableArray.CreateBuilder<TraversalContinuation>(2);

        if (index.Raw > 0L)
        {
            var targetIndex = new Measure(index.Raw - 1L);
            continuations.Add(new TraversalContinuation(
                Carrier,
                source,
                StepTarget(targetIndex, -Measure.One),
                TraversalDisposition.Create(true, -Measure.One, false)));
        }

        if (index.Raw < _samples.Length - 1L)
        {
            var targetIndex = new Measure(index.Raw + 1L);
            continuations.Add(new TraversalContinuation(
                Carrier,
                source,
                StepTarget(targetIndex, Measure.One),
                TraversalDisposition.Create(true, Measure.One, false)));
        }

        observation = new TraversalObservation(source, continuations.ToImmutable());
        return true;
    }

    public bool TryObserveCurrent(out TraversalObservation observation, Measure? perspective = null)
    {
        if (HasCurrent)
        {
            return TryObserve(CurrentIndex, out observation, perspective);
        }

        observation = default;
        return false;
    }

    public IEnumerable<TEnum> IterateSamples(Measure? stepTicks = null)
    {
        Reset();

        while (MoveNext(stepTicks))
        {
            yield return Current;
        }
    }

    public Measure TickOf(TEnum sample)
    {
        var index = _samples.IndexOf(sample);
        return new Measure(index);
    }

    public Ratio PositionAt(Measure index) => new Ratio(index, TickCount).Normalize();

    public Ratio PositionOf(TEnum sample) => PositionAt(TickOf(sample));

    public Placement PlacementAt(Measure index) => new Placement(Unit, PositionAt(index));

    public Placement PlacementOf(TEnum sample) => PlacementAt(TickOf(sample));

    public Site SiteAt(Measure index) => Site.Open(Carrier, index);

    public Site SiteOf(TEnum sample) => SiteAt(TickOf(sample));

    public Relation Relate(TEnum start, TEnum end) => new(Carrier, SiteOf(start), SiteOf(end));

    public Basis Freeze(TEnum start, TEnum end) => Relate(start, end).Freeze();

    public Measure OrderOf(TEnum sample) => TickOf(sample);

    public ImmutableArray<Ratio> IteratePositions(Measure? stepTicks = null)
    {
        var step = Step(stepTicks ?? Measure.One);
        return IteratePositions(Start(), End(), step);
    }

    public ImmutableArray<Placement> Iterate(Measure? stepTicks = null)
    {
        var step = Step(stepTicks ?? Measure.One);
        return Iterate(Start(), End(), step);
    }

    public ImmutableArray<Ratio> IteratePositions(TEnum start, TEnum end, Measure? stepTicks = null)
    {
        var step = Step(stepTicks ?? Measure.One);
        return IteratePositions(PositionOf(start), PositionOf(end), step);
    }

    public ImmutableArray<Placement> Iterate(TEnum start, TEnum end, Measure? stepTicks = null)
    {
        var step = Step(stepTicks ?? Measure.One);
        return Iterate(PositionOf(start), PositionOf(end), step);
    }

    public Ratio Step(Measure ticks) => new Ratio(ticks, TickCount).Normalize();

    private Ratio Start() => Ratio.Zero(TickCount).Normalize();

    private Ratio End() => new Ratio(LastTick, TickCount).Normalize();

    public IEnumerator<TEnum> GetEnumerator() => IterateSamples().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static Measure StepMagnitude(Measure ticks) => ticks.Raw == 0L ? Measure.Zero : ticks.Absolute;

    private ImmutableArray<Ratio> IteratePositions(Ratio start, Ratio end, Ratio step)
    {
        var origin = start.Normalize();
        var target = end.Normalize();
        var magnitude = StepMagnitude(step);

        if (origin.IsTension || target.IsTension || magnitude.Recessive == Measure.Zero || magnitude.Dominant == Measure.Zero)
        {
            return ImmutableArray<Ratio>.Empty;
        }

        var forward = Compare(origin, target) <= 0;
        var positions = ImmutableArray.CreateBuilder<Ratio>();
        var current = origin;

        while (forward ? Compare(current, target) <= 0 : Compare(current, target) >= 0)
        {
            positions.Add(current);
            current = forward
                ? current.Add(magnitude).Normalize()
                : current.Subtract(magnitude).Normalize();
        }

        return positions.ToImmutable();
    }

    private ImmutableArray<Placement> Iterate(Ratio start, Ratio end, Ratio step)
    {
        var positions = IteratePositions(start, end, step);
        var placements = ImmutableArray.CreateBuilder<Placement>(positions.Length);

        foreach (var position in positions)
        {
            placements.Add(new Placement(Unit, position));
        }

        return placements.ToImmutable();
    }

    private static Ratio StepMagnitude(Ratio step)
    {
        var normalized = step.Normalize();

        if (normalized.IsTension || normalized.Recessive == Measure.Zero || normalized.Dominant == Measure.Zero)
        {
            return Ratio.Zero(Measure.Zero);
        }

        return new Ratio(normalized.Recessive.Absolute, normalized.Dominant.Absolute).Normalize();
    }

    private static int Compare(Ratio left, Ratio right)
        => left.CompareValue(right);

    private TraversalSignature StepTarget(Measure index, Measure direction)
    {
        var carried = new Ratio(direction, Measure.One).Normalize();
        return TraversalSignature.FromPlacement(
            PlacementAt(index),
            direction,
            carried,
            TraversalSignature.ProvenanceFor(direction, Measure.One));
    }
}
