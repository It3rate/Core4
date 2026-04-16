using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.Structure;

public readonly record struct OrthogonalFlowGroup(
    Carrier Carrier,
    SupportProjectionRead ValueRead,
    Measure FlowCount)
{
    public OrthogonalFlowGroup(Carrier carrier, ISupportProjection value, Measure flowCount)
        : this(carrier, value.ToSupportRead(), flowCount)
    {
    }

    // TODO(core): Delete this compatibility alias once callers consume ValueRead directly.
    public Ratio Value => ValueRead.CompatibilityRatio();
}

/// <summary>
/// The unresolved orthogonal remainder around a site plus any articulated child flows that have attached to it.
/// The current engine stores these flows in one reservoir, but the intended interpretation is that foreign
/// publication and responsive tension are distinct directions sharing this frontier rather than one amorphous blob.
/// Example: <c>var reservoir = OrthogonalReservoir.Open.Resolve(Ratio.Tension(10)).Add(flow);</c>
/// </summary>
public readonly record struct OrthogonalReservoir(
    SupportProjectionRead ResidualRead,
    ImmutableArray<Flow> Flows,
    ImmutableArray<OrthogonalPublication> Publications,
    ImmutableArray<OrthogonalFeedback> Feedback)
{
    public OrthogonalReservoir(
        ISupportProjection residual,
        ImmutableArray<Flow> flows,
        ImmutableArray<OrthogonalPublication> publications,
        ImmutableArray<OrthogonalFeedback> feedback)
        : this(residual.ToSupportRead(), flows, publications, feedback)
    {
    }

    public OrthogonalReservoir(ISupportProjection residual, ImmutableArray<Flow> flows)
        : this(
            residual.ToSupportRead(),
            flows,
            ImmutableArray<OrthogonalPublication>.Empty,
            ImmutableArray<OrthogonalFeedback>.Empty)
    {
    }

    public static OrthogonalReservoir Open => new(
        Ratio.Zero(Measure.Zero).ToSupportRead(),
        ImmutableArray<Flow>.Empty,
        ImmutableArray<OrthogonalPublication>.Empty,
        ImmutableArray<OrthogonalFeedback>.Empty);

    // TODO(core): Delete this compatibility alias once callers consume ResidualRead directly.
    public Ratio Residual => ResidualRead.CompatibilityRatio();

    public Measure Potential
    {
        get
        {
            var total = ResidualRead.Potential;

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
            var residual = ResidualRead.CompatibilityRatio();
            var total = ResidualRead.Potential != Measure.Zero || residual.Dominant != Measure.Zero ? 1L : 0L;
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
            var total = ResidualRead.CompatibilityRatio().Dominant != Measure.Zero ? 1L : 0L;

            foreach (var flow in Flows)
            {
                if (flow.ValueRead.CompatibilityRatio().Dominant != Measure.Zero)
                {
                    total++;
                }
            }

            total += Publications.Length;
            total += Feedback.Length;

            return new Measure(total);
        }
    }

    public OrthogonalReservoir ResolveRead(ISupportProjection residual) =>
        new(residual.ToSupportRead(), Flows, Publications, Feedback);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public OrthogonalReservoir Resolve(Ratio residual) => ResolveRead(residual);

    public OrthogonalReservoir Add(Flow flow) => new(ResidualRead, Flows.Add(flow), Publications, Feedback);

    public OrthogonalReservoir Add(OrthogonalPublication publication) => new(ResidualRead, Flows, Publications.Add(publication), Feedback);

    public OrthogonalReservoir Add(OrthogonalFeedback feedback) => new(ResidualRead, Flows, Publications, Feedback.Add(feedback));

    public OrthogonalReservoir WithFlows(ImmutableArray<Flow> flows) =>
        new(ResidualRead, flows, Publications, Feedback);

    public OrthogonalReservoir WithPublications(ImmutableArray<OrthogonalPublication> publications) =>
        new(ResidualRead, Flows, publications, Feedback);

    public OrthogonalReservoir WithFeedback(ImmutableArray<OrthogonalFeedback> feedback) =>
        new(ResidualRead, Flows, Publications, feedback);

    public ISupportProjection IncomingRead() => AggregateRead(flow => flow.IsIncoming);

    // TODO(core): Delete this compatibility alias once callers consume IncomingRead directly.
    public Ratio Incoming() => IncomingRead().CompatibilityRatio();

    public ISupportProjection LocalIncomingRead() => AggregateRead(flow => flow.IsIncoming && flow.Support is not Carrier);

    // TODO(core): Delete this compatibility alias once callers consume LocalIncomingRead directly.
    public Ratio LocalIncoming() => LocalIncomingRead().CompatibilityRatio();

    public ISupportProjection OutgoingRead() => AggregateRead(flow => flow.IsOutgoing);

    // TODO(core): Delete this compatibility alias once callers consume OutgoingRead directly.
    public Ratio Outgoing() => OutgoingRead().CompatibilityRatio();

    public ISupportProjection LocalOutgoingRead() => AggregateRead(flow => flow.IsOutgoing && flow.Support is not Carrier);

    // TODO(core): Delete this compatibility alias once callers consume LocalOutgoingRead directly.
    public Ratio LocalOutgoing() => LocalOutgoingRead().CompatibilityRatio();

    public ISupportProjection OscillatingRead() => AggregateRead(flow => flow.IsOscillating);

    // TODO(core): Delete this compatibility alias once callers consume OscillatingRead directly.
    public Ratio Oscillating() => OscillatingRead().CompatibilityRatio();

    public ISupportProjection LocalOscillatingRead() => AggregateRead(flow => flow.IsOscillating && flow.Support is not Carrier);

    // TODO(core): Delete this compatibility alias once callers consume LocalOscillatingRead directly.
    public Ratio LocalOscillating() => LocalOscillatingRead().CompatibilityRatio();

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

    private ISupportProjection AggregateRead(Func<Flow, bool> predicate)
    {
        var hasAny = false;
        var total = Ratio.Zero(Measure.Zero);

        foreach (var flow in Flows)
        {
            if (!predicate(flow))
            {
                continue;
            }

            total = hasAny
                ? total.Add(flow.ValueRead.CompatibilityRatio())
                : flow.ValueRead.CompatibilityRatio().Normalize();
            hasAny = true;
        }

        return hasAny ? new SupportProjectionRead(total.Normalize()) : Ratio.Zero(Measure.Zero);
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

            accumulator.Add(flow.ValueRead);
        }

        return
        [
            .. groups
                .OrderBy(pair => pair.Key.Id)
                .Select(pair => new OrthogonalFlowGroup(
                    pair.Key,
                    pair.Value.ValueOrZeroRead(),
                    pair.Value.FlowCount))
        ];
    }

    private sealed class GroupAccumulator
    {
        private bool _hasValue;
        private SupportProjectionRead _value;

        public Measure FlowCount { get; private set; }

        public void Add(ISupportProjection value)
        {
            _value = _hasValue
                ? new SupportProjectionRead(_value.CompatibilityRatio().Add(value.CompatibilityRatio()).Normalize())
                : value.ToSupportRead();
            _hasValue = true;
            FlowCount += Measure.One;
        }

        public ISupportProjection ValueOrZeroRead() => _hasValue ? _value : Ratio.Zero(Measure.Zero);
    }
}
