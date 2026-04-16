using System.Collections.Immutable;
using Core4.Topology;

namespace Core4.Inspection;

/// <summary>
/// A topology-wide propagation pass. Outbound fronts follow forward continuations under positive perspective and park
/// at termini until the whole wave goes quiescent. Return fronts then run the same continuation law under opposite
/// perspective instead of replaying stored path history, so reverse branching now follows structural reverse flow
/// rather than trail replay.
/// </summary>
public readonly record struct PropagationWave(
    Measure Perspective,
    Measure Period,
    ImmutableArray<PropagationFrontier> Active,
    ImmutableArray<PropagationFrontier> Parked)
{
    public static PropagationWave Start(ConnectedSiteTopology topology, Site site) =>
        new(
            Measure.One,
            Measure.Zero,
            [new PropagationFrontier(TopologyTrolley.Start(topology, site))],
            ImmutableArray<PropagationFrontier>.Empty);

    public static PropagationWave FromTrolleys(ImmutableArray<TopologyTrolley> trolleys, Measure perspective)
    {
        var active = ImmutableArray.CreateBuilder<PropagationFrontier>(trolleys.Length);

        foreach (var trolley in trolleys)
        {
            MergeFrontier(active, new PropagationFrontier(trolley));
        }

        return new PropagationWave(perspective, Measure.Zero, active.ToImmutable(), ImmutableArray<PropagationFrontier>.Empty);
    }

    public bool IsOutbound => Perspective.Raw > 0L;

    public bool IsReturning => Perspective.Raw < 0L;

    public bool IsComplete => Perspective.Raw == 0L;

    public bool IsPeriodic => IsComplete && Period != Measure.Zero;

    public ImmutableArray<PropagationFrontier> VisibleFrontiers
    {
        get
        {
            var visible = ImmutableArray.CreateBuilder<PropagationFrontier>(Parked.Length + Active.Length);

            foreach (var frontier in Parked)
            {
                MergeFrontier(visible, frontier);
            }

            foreach (var frontier in Active)
            {
                MergeFrontier(visible, frontier);
            }

            return visible.ToImmutable();
        }
    }

    public ImmutableArray<TopologyTrolley> VisibleTrolleys()
    {
        var trolleys = ImmutableArray.CreateBuilder<TopologyTrolley>(VisibleFrontiers.Length);

        foreach (var frontier in VisibleFrontiers)
        {
            trolleys.Add(frontier.Trolley);
        }

        return trolleys.ToImmutable();
    }

    public PropagationWaveStep Advance()
    {
        if (IsComplete)
        {
            return new PropagationWaveStep(this, this, ImmutableArray<PropagationMove>.Empty);
        }

        return AdvanceInPerspective(Perspective);
    }

    private PropagationWaveStep AdvanceInPerspective(Measure observerPerspective)
    {
        var nextActive = ImmutableArray.CreateBuilder<PropagationFrontier>();
        var nextParked = ImmutableArray.CreateBuilder<PropagationFrontier>(Parked.Length + Active.Length);
        var moves = ImmutableArray.CreateBuilder<PropagationMove>();

        foreach (var frontier in Parked)
        {
            MergeFrontier(nextParked, frontier);
        }

        foreach (var frontier in Active)
        {
            if (observerPerspective.Raw < 0L
                && frontier.Trolley.TrailSites.Length != 0
                && SameNode(frontier.Site, frontier.Trolley.TrailSites[0]))
            {
                MergeFrontier(nextParked, frontier);
                continue;
            }

            var observation = frontier.Trolley.Observe(observerPerspective);
            var forward = observerPerspective.Raw < 0L && observation.TryPreferred(observerPerspective, out var preferred)
                ? [preferred]
                : observation.ForwardContinuationsIn(observerPerspective);

            if (forward.Length == 0)
            {
                MergeFrontier(nextParked, frontier);
                continue;
            }

            foreach (var continuation in forward)
            {
                if (continuation.TransmissionRead.Potential != Measure.Zero)
                {
                    var moved = frontier.Move(continuation);
                    MergeFrontier(nextActive, moved);
                    moves.Add(new PropagationMove(
                        frontier.Trolley,
                        moved.Trolley,
                        continuation.Relation,
                        observerPerspective,
                        continuation.TransmissionRead,
                        continuation.RetentionRead));
                }

                if (continuation.RetentionRead.Potential != Measure.Zero)
                {
                    MergeFrontier(nextParked, frontier.Retain(continuation.RetentionRead));
                }
            }
        }

        if (observerPerspective.Raw > 0L && nextParked.Count == 0 && nextActive.Count != 0 && AllPeriodic(nextActive))
        {
            var completed = new PropagationWave(
                Measure.Zero,
                SharedPeriod(nextActive),
                ImmutableArray<PropagationFrontier>.Empty,
                nextActive.ToImmutable());
            return new PropagationWaveStep(this, completed, moves.ToImmutable());
        }

        if (nextActive.Count == 0)
        {
            if (observerPerspective.Raw > 0L)
            {
                var returning = new PropagationWave(
                    nextParked.Count == 0 ? Measure.Zero : -Measure.One,
                    Measure.Zero,
                    nextParked.ToImmutable(),
                    ImmutableArray<PropagationFrontier>.Empty);
                return new PropagationWaveStep(this, returning, ImmutableArray<PropagationMove>.Empty);
            }

            var completed = new PropagationWave(
                Measure.Zero,
                Measure.Zero,
                ImmutableArray<PropagationFrontier>.Empty,
                nextParked.ToImmutable());
            return new PropagationWaveStep(this, completed, ImmutableArray<PropagationMove>.Empty);
        }

        return new PropagationWaveStep(
            this,
            new PropagationWave(observerPerspective.Sign, Measure.Zero, nextActive.ToImmutable(), nextParked.ToImmutable()),
            moves.ToImmutable());
    }

    private static void MergeFrontier(ImmutableArray<PropagationFrontier>.Builder frontiers, PropagationFrontier frontier)
    {
        for (var index = 0; index < frontiers.Count; index++)
        {
            if (!frontiers[index].CanMergeWith(frontier))
            {
                continue;
            }

            frontiers[index] = frontiers[index] with
            {
                Histories = frontiers[index].Histories.AddRange(frontier.Histories),
                WeightRead = frontiers[index].WeightRead.AddRead(frontier.WeightRead)
            };
            return;
        }

        frontiers.Add(frontier);
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();

    private static bool AllPeriodic(ImmutableArray<PropagationFrontier>.Builder frontiers)
    {
        foreach (var frontier in frontiers)
        {
            if (!frontier.HasExactReentry)
            {
                return false;
            }
        }

        return true;
    }

    private static Measure SharedPeriod(ImmutableArray<PropagationFrontier>.Builder frontiers)
    {
        var best = Measure.Zero;

        foreach (var frontier in frontiers)
        {
            if (best == Measure.Zero || frontier.ExactReentryPeriod.Raw < best.Raw)
            {
                best = frontier.ExactReentryPeriod;
            }
        }

        return best;
    }
}

public readonly record struct PropagationFrontier(
    ImmutableArray<TopologyTrolley> Histories,
    SupportProjectionRead WeightRead)
{
    public PropagationFrontier(TopologyTrolley trolley) : this([trolley], new SupportProjectionRead(new Ratio(Measure.One, Measure.One)))
    {
    }

    public TopologyTrolley Trolley => Histories[0];

    public Measure Multiplicity => new(Histories.Length);

    // TODO(core): Delete this compatibility alias once callers consume WeightRead directly.
    public Ratio Weight => WeightRead.CompatibilityRatio();

    public ISupportProjection IntensityRead => WeightRead.CanonicalizeRead();

    // TODO(core): Delete this compatibility alias once callers consume IntensityRead directly.
    public Ratio Intensity => IntensityRead.CompatibilityRatio();

    public Measure ExactReentryPeriod
    {
        get
        {
            var best = Measure.Zero;

            foreach (var history in Histories)
            {
                if (history.ExactReentryPeriod == Measure.Zero)
                {
                    return Measure.Zero;
                }

                if (best == Measure.Zero || history.ExactReentryPeriod.Raw < best.Raw)
                {
                    best = history.ExactReentryPeriod;
                }
            }

            return best;
        }
    }

    public bool HasExactReentry => ExactReentryPeriod != Measure.Zero;

    public Site Site => Trolley.Site;

    public Core4.Topology.TraversalSignature Signature => Trolley.Signature;

    public bool CanMergeWith(PropagationFrontier other) =>
        Signature.SameProjection(other.Signature)
        && Signature.Perspective.Sign == other.Signature.Perspective.Sign;

    public PropagationFrontier Move(TopologyTrolleyContinuation continuation) => Move(continuation, continuation.TransmissionRead);

    public PropagationFrontier Move(TopologyTrolleyContinuation continuation, ISupportProjection transmission)
    {
        var moved = ImmutableArray.CreateBuilder<TopologyTrolley>(Histories.Length);

        foreach (var history in Histories)
        {
            moved.Add(history.Move(continuation));
        }

        return new PropagationFrontier(
            moved.ToImmutable(),
            WeightRead.MultiplyRead(transmission));
    }

    // TODO(core): Delete this compatibility overload once callers pass support-shaped transmission reads directly.
    public PropagationFrontier Move(TopologyTrolleyContinuation continuation, Ratio transmission) =>
        Move(continuation, (ISupportProjection)transmission);

    public PropagationFrontier Retain(ISupportProjection retention) =>
        new(Histories, WeightRead.MultiplyRead(retention));

    // TODO(core): Delete this compatibility overload once callers pass support-shaped retention reads directly.
    public PropagationFrontier Retain(Ratio retention) => Retain((ISupportProjection)retention);
}

public readonly record struct PropagationMove(
    TopologyTrolley Source,
    TopologyTrolley Target,
    Relation Relation,
    Measure Perspective,
    SupportProjectionRead TransmissionRead,
    SupportProjectionRead RetentionRead)
{
    public PropagationMove(
        TopologyTrolley source,
        TopologyTrolley target,
        Relation relation,
        Measure perspective,
        ISupportProjection transmission,
        ISupportProjection retention) : this(
            source,
            target,
            relation,
            perspective,
            new SupportProjectionRead(transmission.CompatibilityRatio()),
            new SupportProjectionRead(retention.CompatibilityRatio()))
    {
    }

    // TODO(core): Delete this compatibility alias once callers consume TransmissionRead directly.
    public Ratio Transmission => TransmissionRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume RetentionRead directly.
    public Ratio Retention => RetentionRead.CompatibilityRatio();

    public bool IsReturning => Perspective.Raw < 0L;
}

public readonly record struct PropagationWaveStep(
    PropagationWave Before,
    PropagationWave After,
    ImmutableArray<PropagationMove> Moves)
{
    public bool HasMotion => !Moves.IsDefaultOrEmpty && Moves.Length != 0;
}
