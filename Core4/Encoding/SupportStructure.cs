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
    Ratio Commitment);

internal readonly record struct SupportSpan(
    Carrier Carrier,
    Ratio OriginPosition,
    Ratio AnchorPosition,
    Ratio Commitment);

internal readonly record struct LandmarkCoincidence(
    LandmarkId Left,
    LandmarkId Right,
    Ratio Commitment);

/// <summary>
/// The shared frozen local measuring scaffold behind the current public support aliases. Conceptually this is one
/// ruler-like calibration complex on one or more carriers: pinned landmarks, frozen coincidences, and fused local
/// components that together define how nearby samples can be read. This type is intentionally downward-facing; it does
/// not own any larger connected network it may participate in, and richer network/topology views should be discovered
/// by interpretation/traversal rather than stored here as universal authority.
/// </summary>
internal sealed class SupportStructure
{
    private SupportStructure(
        ImmutableArray<SupportLandmark> landmarks,
        ImmutableArray<SupportSpan> spans,
        ImmutableArray<LandmarkCoincidence> coincidences,
        ImmutableArray<SupportStructure> components)
    {
        Landmarks = landmarks;
        Spans = spans;
        Coincidences = coincidences;
        Components = components;
    }

    public ImmutableArray<SupportLandmark> Landmarks { get; }

    public ImmutableArray<SupportSpan> Spans { get; }

    public ImmutableArray<LandmarkCoincidence> Coincidences { get; }

    public ImmutableArray<SupportStructure> Components { get; }

    public bool IsAtomic => Components.IsEmpty;

    public bool IsComposite => !IsAtomic;

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

    private static SupportStructure Compose(ImmutableArray<SupportStructure> components)
    {
        var landmarks = ImmutableArray.CreateBuilder<SupportLandmark>();
        var spans = ImmutableArray.CreateBuilder<SupportSpan>();
        var coincidences = ImmutableArray.CreateBuilder<LandmarkCoincidence>();
        var flattened = new List<(int InputIndex, SupportLandmark Landmark)>();
        long nextId = 0L;

        for (var inputIndex = 0; inputIndex < components.Length; inputIndex++)
        {
            var component = components[inputIndex];
            var remap = new Dictionary<LandmarkId, LandmarkId>();

            foreach (var landmark in component.Landmarks)
            {
                var remapped = Reidentify(landmark, nextId++);
                remap[landmark.Id] = remapped.Id;
                landmarks.Add(remapped);
                flattened.Add((inputIndex, remapped));
            }

            foreach (var coincidence in component.Coincidences)
            {
                if (remap.TryGetValue(coincidence.Left, out var left)
                    && remap.TryGetValue(coincidence.Right, out var right))
                {
                    coincidences.Add(new LandmarkCoincidence(left, right, coincidence.Commitment.Normalize()));
                }
            }

            spans.AddRange(component.Spans);
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

        return new SupportStructure(
            landmarks.ToImmutable(),
            spans.ToImmutable(),
            coincidences.ToImmutable(),
            components);
    }

    private static SupportLandmark Reidentify(SupportLandmark landmark, LandmarkId id) =>
        new(id, landmark.Carrier, landmark.Position.Normalize(), landmark.Commitment.Normalize());
}

