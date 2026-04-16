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
    private readonly Func<Ratio, Carrier, Ratio> _locate;

    private SupportStructure(
        Carrier primaryCarrier,
        Measure supportGrade,
        ImmutableArray<SupportLandmark> landmarks,
        ImmutableArray<LandmarkCoincidence> coincidences,
        ImmutableArray<SupportStructure> inputs,
        Func<Ratio, Carrier, Ratio> locate)
    {
        PrimaryCarrier = primaryCarrier;
        SupportGrade = supportGrade;
        Landmarks = landmarks;
        Coincidences = coincidences;
        Inputs = inputs;
        _locate = locate;
    }

    public Carrier PrimaryCarrier { get; }

    public Measure SupportGrade { get; }

    public ImmutableArray<SupportLandmark> Landmarks { get; }

    public ImmutableArray<LandmarkCoincidence> Coincidences { get; }

    public ImmutableArray<SupportStructure> Inputs { get; }

    public Ratio Locate(Ratio position, Carrier carrier) => _locate(position.Normalize(), carrier).Normalize();

    public static SupportStructure ForCarrier(Carrier carrier) => new(
        carrier,
        Measure.Zero,
        ImmutableArray<SupportLandmark>.Empty,
        ImmutableArray<LandmarkCoincidence>.Empty,
        ImmutableArray<SupportStructure>.Empty,
        (position, target) => target == carrier
            ? position.Normalize()
            : Ratio.Tension(position.Potential));

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
            ImmutableArray<LandmarkCoincidence>.Empty,
            ImmutableArray<SupportStructure>.Empty,
            (position, target) => target == carrier
                ? originPosition.Add(span.Multiply(position)).Normalize()
                : Ratio.Tension(position.Potential));
    }

    public static SupportStructure Fuse(
        SupportStructure recessive,
        SupportStructure dominant,
        Func<Ratio, Carrier, Ratio> locate) =>
        Compose([recessive, dominant], locate);

    public static SupportStructure Fuse(
        SupportStructure recessive,
        SupportStructure dominant,
        ImmutableArray<SupportStructure> remaining,
        Func<Ratio, Carrier, Ratio> locate)
    {
        var inputs = ImmutableArray.CreateBuilder<SupportStructure>(remaining.Length + 2);
        inputs.Add(recessive);
        inputs.Add(dominant);
        inputs.AddRange(remaining);
        return Compose(inputs.ToImmutable(), locate);
    }

    private static SupportStructure Compose(
        ImmutableArray<SupportStructure> inputs,
        Func<Ratio, Carrier, Ratio> locate)
    {
        var landmarks = ImmutableArray.CreateBuilder<SupportLandmark>();
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
            coincidences.ToImmutable(),
            inputs,
            locate);
    }
}
