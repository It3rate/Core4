using System.Collections.Immutable;
using Core4.Numerics;
namespace Core4.Structure;

public readonly record struct OrthogonalFlowGroup(
    Carrier Carrier,
    Ratio Value,
    Measure FlowCount);

/// <summary>
/// The unresolved orthogonal remainder around a site plus any articulated child flows that have attached to it.
/// The current engine stores these flows in one reservoir, but the intended interpretation is that foreign
/// publication and responsive tension are distinct directions sharing this frontier rather than one amorphous blob.
/// Example: <c>var reservoir = OrthogonalReservoir.Open.Resolve(Ratio.Tension(10)).Add(flow);</c>
/// </summary>
public readonly record struct OrthogonalReservoir(
    Ratio Residual,
    ImmutableArray<Flow> Flows,
    ImmutableArray<OrthogonalPublication> Publications,
    ImmutableArray<OrthogonalFeedback> Feedback)
{
    public OrthogonalReservoir(Ratio residual, ImmutableArray<Flow> flows)
        : this(
            residual,
            flows,
            ImmutableArray<OrthogonalPublication>.Empty,
            ImmutableArray<OrthogonalFeedback>.Empty)
    {
    }

    public static OrthogonalReservoir Open => new(
        Ratio.Zero(Measure.Zero),
        ImmutableArray<Flow>.Empty,
        ImmutableArray<OrthogonalPublication>.Empty,
        ImmutableArray<OrthogonalFeedback>.Empty);

    public Measure Potential
    {
        get
        {
            var total = Residual.Potential;

            foreach (var flow in Flows)
            {
                total += flow.Potential;
            }

            return total;
        }
    }

    public Measure ConnectionCount => new(Flows.Length);

    public Measure PublicationCount => new(Publications.Length);

    public Measure FeedbackCount => new(Feedback.Length);

    public Measure OccupiedCount
    {
        get
        {
            var total = Residual.Potential != Measure.Zero || Residual.Dominant != Measure.Zero ? 1L : 0L;
            total += Flows.Length;
            total += Publications.Length;
            total += Feedback.Length;
            return new Measure(total);
        }
    }

    public Measure BoundCount
    {
        get
        {
            var total = Residual.Dominant != Measure.Zero ? 1L : 0L;

            foreach (var flow in Flows)
            {
                if (flow.Value.Dominant != Measure.Zero)
                {
                    total++;
                }
            }

            total += Publications.Length;
            total += Feedback.Length;

            return new Measure(total);
        }
    }

    public OrthogonalReservoir Resolve(Ratio residual) => new(residual.Normalize(), Flows, Publications, Feedback);

    public OrthogonalReservoir Add(Flow flow) => new(Residual, Flows.Add(flow), Publications, Feedback);

    public OrthogonalReservoir Add(OrthogonalPublication publication) => new(Residual, Flows, Publications.Add(publication), Feedback);

    public OrthogonalReservoir Add(OrthogonalFeedback feedback) => new(Residual, Flows, Publications, Feedback.Add(feedback));

    public OrthogonalReservoir WithFlows(ImmutableArray<Flow> flows) =>
        new(Residual, flows, Publications, Feedback);

    public OrthogonalReservoir WithPublications(ImmutableArray<OrthogonalPublication> publications) =>
        new(Residual, Flows, publications, Feedback);

    public OrthogonalReservoir WithFeedback(ImmutableArray<OrthogonalFeedback> feedback) =>
        new(Residual, Flows, Publications, feedback);

    public Ratio Incoming() => Aggregate(flow => flow.IsIncoming);

    public Ratio LocalIncoming() => Aggregate(flow => flow.IsIncoming && flow.Support is not Carrier);

    public Ratio Outgoing() => Aggregate(flow => flow.IsOutgoing);

    public Ratio LocalOutgoing() => Aggregate(flow => flow.IsOutgoing && flow.Support is not Carrier);

    public Ratio Oscillating() => Aggregate(flow => flow.IsOscillating);

    public Ratio LocalOscillating() => Aggregate(flow => flow.IsOscillating && flow.Support is not Carrier);

    public ImmutableArray<OrthogonalFlowGroup> LocalIncomingGroups() =>
        Group(flow => flow.IsIncoming && flow.Support is not Carrier);

    public ImmutableArray<OrthogonalFlowGroup> LocalOutgoingGroups() =>
        Group(flow => flow.IsOutgoing && flow.Support is not Carrier);

    public ImmutableArray<OrthogonalFlowGroup> LocalOscillatingGroups() =>
        Group(flow => flow.IsOscillating && flow.Support is not Carrier);

    public ImmutableArray<OrthogonalPublication> IncomingPublications() =>
        [.. Publications.Where(publication => publication.IsIncoming)];

    public ImmutableArray<OrthogonalPublication> OutgoingPublications() =>
        [.. Publications.Where(publication => publication.IsOutgoing)];

    public ImmutableArray<OrthogonalPublication> OwnedPublications() =>
        [.. Publications.Where(publication => !publication.IsIncoming)];

    public ImmutableArray<OrthogonalFeedback> IncomingFeedback() =>
        [.. Feedback.Where(feedback => feedback.IsIncoming)];

    private Ratio Aggregate(Func<Flow, bool> predicate)
    {
        var hasAny = false;
        var total = Ratio.Zero(Measure.Zero);

        foreach (var flow in Flows)
        {
            if (!predicate(flow))
            {
                continue;
            }

            total = hasAny ? total.Add(flow.Value) : flow.Value.Normalize();
            hasAny = true;
        }

        return hasAny ? total.Normalize() : Ratio.Zero(Measure.Zero);
    }

    private ImmutableArray<OrthogonalFlowGroup> Group(Func<Flow, bool> predicate)
    {
        var groups = new Dictionary<Carrier, GroupAccumulator>();

        foreach (var flow in Flows)
        {
            if (!predicate(flow))
            {
                continue;
            }

            if (!groups.TryGetValue(flow.Carrier, out var accumulator))
            {
                accumulator = new GroupAccumulator();
                groups[flow.Carrier] = accumulator;
            }

            accumulator.Add(flow.Value);
        }

        return
        [
            .. groups
                .OrderBy(pair => pair.Key.Id)
                .Select(pair => new OrthogonalFlowGroup(
                    pair.Key,
                    pair.Value.ValueOrZero(),
                    pair.Value.FlowCount))
        ];
    }

    private sealed class GroupAccumulator
    {
        private bool _hasValue;
        private Ratio _value;

        public Measure FlowCount { get; private set; }

        public void Add(Ratio value)
        {
            _value = _hasValue ? _value.Add(value).Normalize() : value.Normalize();
            _hasValue = true;
            FlowCount += Measure.One;
        }

        public Ratio ValueOrZero() => _hasValue ? _value.Normalize() : Ratio.Zero(Measure.Zero);
    }
}
