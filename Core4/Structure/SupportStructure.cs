using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.Structure;

internal readonly record struct LandmarkId(long Value)
{
    public static implicit operator LandmarkId(long value) => new(value);
}

internal readonly record struct SupportLandmark(
    LandmarkId Id,
    Carrier Carrier,
    Ratio Position,
    Ratio Commitment)
{
    public SupportLandmark Reidentify(LandmarkId id) => new(id, Carrier, Position.Normalize(), Commitment.Normalize());
}

internal readonly record struct SupportSpan(
    Carrier Carrier,
    Ratio OriginPosition,
    Ratio AnchorPosition,
    Ratio Commitment)
{
    public Ratio Span => AnchorPosition.Subtract(OriginPosition).Normalize();

    public Measure Potential => Span.Potential;

    public Measure Orientation => Span.Recessive.Sign;
}

internal readonly record struct LandmarkCoincidence(
    LandmarkId Left,
    LandmarkId Right,
    Ratio Commitment);

/// <summary>
/// The shared bottom-level frozen support family behind the current public support aliases. Public types such as
/// carrier, relation, and basis still provide the ergonomic API, while this type holds the underlying landmark and
/// coincidence picture those views are built from.
/// </summary>
internal sealed class SupportStructure : ISupport
{
    private SupportStructure(
        Carrier primaryCarrier,
        Measure supportGrade,
        ImmutableArray<SupportLandmark> landmarks,
        ImmutableArray<SupportSpan> spans,
        ImmutableArray<LandmarkCoincidence> coincidences,
        ImmutableArray<SupportStructure> inputs)
    {
        PrimaryCarrier = primaryCarrier;
        SupportGrade = supportGrade;
        Landmarks = landmarks;
        Spans = spans;
        Coincidences = coincidences;
        Inputs = inputs;
    }

    public Carrier PrimaryCarrier { get; }

    public Measure SupportGrade { get; }

    public ImmutableArray<SupportLandmark> Landmarks { get; }

    public ImmutableArray<SupportSpan> Spans { get; }

    public ImmutableArray<LandmarkCoincidence> Coincidences { get; }

    public ImmutableArray<SupportStructure> Inputs { get; }

    public bool IsAtomic => Inputs.IsEmpty;

    public bool IsComposite => !IsAtomic;

    public Measure RelationCount => new(Spans.Length);

    public Measure CarrierCount => new(SelectCarriers().Length);

    public Ratio Stiffness => IsAtomic
        ? AtomicStiffness()
        : Inputs[0].Stiffness.Weaker(Inputs[1].Stiffness);

    public Measure Potential => IsAtomic
        ? AtomicPotential()
        : IsSameCarrierPair() && IsOpposedPair() && HasSharedOriginPerCarrier()
            ? Inputs[0].Potential + Inputs[1].Potential
            : new Measure(checked(Inputs[0].Potential.Raw * Inputs[1].Potential.Raw));

    public Ratio Fold => IsComposite
        ? new Ratio(Inputs[0].Potential, Inputs[1].Potential).Normalize()
        : Ratio.Tension(Potential);

    public Measure Orientation => IsAtomic ? AtomicOrientation() : Measure.Zero;

    public Ratio Locate(Ratio position, Carrier carrier)
    {
        var normalized = position.Normalize();

        if (IsAtomic)
        {
            return LocateAtomic(normalized, carrier);
        }

        if (IsAtomicPair() && IsSameCarrierPair() && IsOpposedPair() && HasSharedOriginPerCarrier())
        {
            return LocateDirectedSegment(normalized, carrier);
        }

        return Ratio.Tension(normalized.Potential);
    }

    public ImmutableArray<Carrier> SelectCarriers()
    {
        var carriers = ImmutableArray.CreateBuilder<Carrier>();

        foreach (var span in Spans)
        {
            if (!carriers.Contains(span.Carrier))
            {
                carriers.Add(span.Carrier);
            }
        }

        return carriers.ToImmutable();
    }

    public bool HasSharedOriginPerCarrier()
    {
        foreach (var carrier in SelectCarriers())
        {
            var origin = default(Ratio);
            var hasOrigin = false;

            foreach (var span in Spans)
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

    public bool IsAtomicPair() => Inputs.Length == 2
        && Inputs[0].Inputs.IsEmpty
        && Inputs[1].Inputs.IsEmpty
        && Inputs[0].Spans.Length == 1
        && Inputs[1].Spans.Length == 1;

    public bool IsSameCarrierPair() => IsAtomicPair()
        && Inputs[0].Spans[0].Carrier == Inputs[1].Spans[0].Carrier;

    public bool IsAlignedPair() => IsAtomicPair()
        && Inputs[0].Spans[0].Orientation.Raw != 0
        && Inputs[0].Spans[0].Orientation == Inputs[1].Spans[0].Orientation;

    public bool IsOpposedPair() => IsAtomicPair()
        && Inputs[0].Spans[0].Orientation.Raw != 0
        && Inputs[0].Spans[0].Orientation == -Inputs[1].Spans[0].Orientation;

    public Measure KernelCode()
    {
        if (!IsAtomicPair())
        {
            return Measure.Zero;
        }

        var recessive = SideCode(Inputs[0].Spans[0].Orientation);
        var dominant = SideCode(Inputs[1].Spans[0].Orientation);

        if (recessive < 0 || dominant < 0)
        {
            return Measure.Zero;
        }

        return new Measure((recessive << 1) | dominant);
    }

    public Ratio Project(Site site)
    {
        if (!IsAtomic || Spans.IsEmpty)
        {
            return Ratio.Tension(site.Potential);
        }

        var span = Spans[0];
        var sitePosition = site.On(span.Carrier).Normalize();
        var delta = sitePosition.Subtract(span.OriginPosition);
        return delta.Divide(span.Span);
    }

    public Ratio Project(Measure measure)
    {
        if (!IsAtomic || Spans.IsEmpty)
        {
            return Ratio.Tension(measure);
        }

        return Project(Site.Open(Spans[0].Carrier, measure));
    }

    public static SupportStructure ForCarrier(Carrier carrier) => new(
        carrier,
        Measure.Zero,
        ImmutableArray<SupportLandmark>.Empty,
        ImmutableArray<SupportSpan>.Empty,
        ImmutableArray<LandmarkCoincidence>.Empty,
        ImmutableArray<SupportStructure>.Empty);

    public static SupportStructure FromRelation(Relation relation) => FromRelation(
        relation.Carrier,
        relation.Origin,
        relation.Anchor,
        relation.Stiffness);

    public static SupportStructure FromRelation(
        Carrier carrier,
        Site origin,
        Site anchor,
        Ratio commitment)
    {
        var normalizedCommitment = commitment.Normalize();
        var originPosition = origin.On(carrier).Normalize();
        var anchorPosition = anchor.On(carrier).Normalize();
        var span = anchorPosition.Subtract(originPosition).Normalize();

        return new SupportStructure(
            carrier,
            Measure.Zero,
            [
                new SupportLandmark(0, carrier, originPosition, normalizedCommitment),
                new SupportLandmark(1, carrier, anchorPosition, normalizedCommitment)
            ],
            [
                new SupportSpan(carrier, originPosition, anchorPosition, normalizedCommitment)
            ],
            ImmutableArray<LandmarkCoincidence>.Empty,
            ImmutableArray<SupportStructure>.Empty);
    }

    public static SupportStructure Fuse(SupportStructure recessive, SupportStructure dominant) =>
        Compose([recessive, dominant]);

    public static SupportStructure Fuse(
        SupportStructure recessive,
        SupportStructure dominant,
        ImmutableArray<SupportStructure> remaining)
    {
        var fused = Fuse(recessive, dominant);

        for (var index = 0; index < remaining.Length; index++)
        {
            fused = Fuse(fused, remaining[index]);
        }

        return fused;
    }

    private static SupportStructure Compose(ImmutableArray<SupportStructure> inputs)
    {
        var landmarks = ImmutableArray.CreateBuilder<SupportLandmark>();
        var spans = ImmutableArray.CreateBuilder<SupportSpan>();
        var coincidences = ImmutableArray.CreateBuilder<LandmarkCoincidence>();
        var flattened = new List<(int InputIndex, SupportLandmark Landmark)>();
        long nextId = 0L;

        for (var inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
        {
            var input = inputs[inputIndex];
            var remap = new Dictionary<LandmarkId, LandmarkId>();

            foreach (var landmark in input.Landmarks)
            {
                var remapped = landmark.Reidentify(nextId++);
                remap[landmark.Id] = remapped.Id;
                landmarks.Add(remapped);
                flattened.Add((inputIndex, remapped));
            }

            foreach (var coincidence in input.Coincidences)
            {
                if (remap.TryGetValue(coincidence.Left, out var left)
                    && remap.TryGetValue(coincidence.Right, out var right))
                {
                    coincidences.Add(new LandmarkCoincidence(left, right, coincidence.Commitment.Normalize()));
                }
            }

            spans.AddRange(input.Spans);
        }

        for (var leftIndex = 0; leftIndex < flattened.Count; leftIndex++)
        {
            for (var rightIndex = leftIndex + 1; rightIndex < flattened.Count; rightIndex++)
            {
                var left = flattened[leftIndex];
                var right = flattened[rightIndex];

                if (left.InputIndex == right.InputIndex
                    || left.Landmark.Carrier != right.Landmark.Carrier
                    || left.Landmark.Position.Normalize() != right.Landmark.Position.Normalize())
                {
                    continue;
                }

                coincidences.Add(new LandmarkCoincidence(
                    left.Landmark.Id,
                    right.Landmark.Id,
                    left.Landmark.Commitment.Weaker(right.Landmark.Commitment).Normalize()));
            }
        }

        var primaryCarrier = inputs.Length == 0 ? new Carrier(0) : inputs[0].PrimaryCarrier;
        var supportGrade = inputs.Length == 0
            ? Measure.Zero
            : new Measure(inputs.Max(input => input.SupportGrade.Raw) + 1L);

        return new SupportStructure(
            primaryCarrier,
            supportGrade,
            landmarks.ToImmutable(),
            spans.ToImmutable(),
            coincidences.ToImmutable(),
            inputs);
    }

    private Ratio LocateAtomic(Ratio position, Carrier carrier)
    {
        if (Spans.IsEmpty)
        {
            return carrier == PrimaryCarrier
                ? position.Normalize()
                : Ratio.Tension(position.Potential);
        }

        var span = Spans[0];
        return carrier == span.Carrier
            ? span.OriginPosition.Add(span.Span.Multiply(position)).Normalize()
            : Ratio.Tension(position.Potential);
    }

    private Ratio LocateDirectedSegment(Ratio position, Carrier carrier)
    {
        var left = Inputs[0].Spans[0];
        var right = Inputs[1].Spans[0];

        if (left.Carrier != carrier || right.Carrier != carrier)
        {
            return Ratio.Tension(position.Potential);
        }

        return left.AnchorPosition.Add(right.AnchorPosition.Subtract(left.AnchorPosition).Multiply(position)).Normalize();
    }

    private Measure AtomicPotential() => Spans.IsEmpty ? Measure.Zero : Spans[0].Potential;

    private Measure AtomicOrientation() => Spans.IsEmpty ? Measure.Zero : Spans[0].Orientation;

    private Ratio AtomicStiffness() => Spans.IsEmpty ? Ratio.Zero(Measure.One) : Spans[0].Commitment;

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
}
