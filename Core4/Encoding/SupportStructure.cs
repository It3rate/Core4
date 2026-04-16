using System.Collections.Immutable;

namespace Core4.Encoding;

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
/// The shared frozen measuring scaffold behind the current public support aliases. Conceptually this is the ruler:
/// pinned landmarks, frozen coincidences, and fused sub-supports that together define how later samples can be read.
/// Public types such as carrier, relation, and basis still provide the ergonomic API, while this type holds the
/// underlying landmark and coincidence picture those views are built from.
/// </summary>
internal sealed class SupportStructure
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

    public static SupportStructure ForCarrier(Carrier carrier) => new(
        carrier,
        Measure.Zero,
        ImmutableArray<SupportLandmark>.Empty,
        ImmutableArray<SupportSpan>.Empty,
        ImmutableArray<LandmarkCoincidence>.Empty,
        ImmutableArray<SupportStructure>.Empty);

    public static SupportStructure FromSpan(
        Carrier carrier,
        Ratio originPosition,
        Ratio anchorPosition,
        Ratio commitment)
    {
        var normalizedCommitment = commitment.Normalize();
        var normalizedOrigin = originPosition.Normalize();
        var normalizedAnchor = anchorPosition.Normalize();
        var span = normalizedAnchor.Subtract(normalizedOrigin).Normalize();

        return new SupportStructure(
            carrier,
            Measure.Zero,
            [
                new SupportLandmark(0, carrier, normalizedOrigin, normalizedCommitment),
                new SupportLandmark(1, carrier, normalizedAnchor, normalizedCommitment)
            ],
            [
                new SupportSpan(carrier, normalizedOrigin, normalizedAnchor, normalizedCommitment)
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
}

