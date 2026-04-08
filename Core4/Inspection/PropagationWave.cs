using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Inspection;

/// <summary>
/// A topology-wide propagation pass. Outbound fronts follow forward-flowing continuations and park at termini until
/// the whole wave goes quiescent. Return fronts then walk their own trails backward. Fronts that meet at one site
/// fuse by summing multiplicity, so merge remains numeric rather than becoming a UI-only deletion rule.
/// </summary>
public readonly record struct PropagationWave(
    Measure Perspective,
    ImmutableArray<PropagationFrontier> Active,
    ImmutableArray<PropagationFrontier> Parked)
{
    public static PropagationWave Start(ConnectedSiteTopology topology, Site site) =>
        new(
            Measure.One,
            [new PropagationFrontier(TopologyTrolley.Start(topology, site), Measure.One)],
            ImmutableArray<PropagationFrontier>.Empty);

    public static PropagationWave FromTrolleys(ImmutableArray<TopologyTrolley> trolleys, Measure perspective)
    {
        var active = ImmutableArray.CreateBuilder<PropagationFrontier>(trolleys.Length);

        foreach (var trolley in trolleys)
        {
            MergeFrontier(active, new PropagationFrontier(trolley, Measure.One));
        }

        return new PropagationWave(perspective, active.ToImmutable(), ImmutableArray<PropagationFrontier>.Empty);
    }

    public bool IsOutbound => Perspective.Raw > 0L;

    public bool IsReturning => Perspective.Raw < 0L;

    public bool IsComplete => Perspective.Raw == 0L;

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

        return IsOutbound
            ? AdvanceOutbound()
            : AdvanceReturn();
    }

    private PropagationWaveStep AdvanceOutbound()
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
            var forward = frontier.Trolley.Observe().ForwardContinuations;

            if (forward.Length == 0)
            {
                MergeFrontier(nextParked, frontier);
                continue;
            }

            foreach (var continuation in forward)
            {
                var moved = new PropagationFrontier(frontier.Trolley.Move(continuation), frontier.Multiplicity);
                MergeFrontier(nextActive, moved);
                moves.Add(new PropagationMove(frontier, moved));
            }
        }

        if (nextActive.Count == 0)
        {
            var returning = new PropagationWave(
                nextParked.Count == 0 ? Measure.Zero : -Measure.One,
                nextParked.ToImmutable(),
                ImmutableArray<PropagationFrontier>.Empty);
            return new PropagationWaveStep(this, returning, ImmutableArray<PropagationMove>.Empty);
        }

        return new PropagationWaveStep(
            this,
            new PropagationWave(Measure.One, nextActive.ToImmutable(), nextParked.ToImmutable()),
            moves.ToImmutable());
    }

    private PropagationWaveStep AdvanceReturn()
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
            if (frontier.Trolley.TryReverse(out var reversed))
            {
                var moved = new PropagationFrontier(reversed, frontier.Multiplicity);
                MergeFrontier(nextActive, moved);
                moves.Add(new PropagationMove(frontier, moved));
            }
            else
            {
                MergeFrontier(nextParked, frontier);
            }
        }

        if (nextActive.Count == 0)
        {
            var completed = new PropagationWave(
                Measure.Zero,
                ImmutableArray<PropagationFrontier>.Empty,
                nextParked.ToImmutable());
            return new PropagationWaveStep(this, completed, ImmutableArray<PropagationMove>.Empty);
        }

        return new PropagationWaveStep(
            this,
            new PropagationWave(-Measure.One, nextActive.ToImmutable(), nextParked.ToImmutable()),
            moves.ToImmutable());
    }

    private static void MergeFrontier(ImmutableArray<PropagationFrontier>.Builder frontiers, PropagationFrontier frontier)
    {
        for (var index = 0; index < frontiers.Count; index++)
        {
            if (!SameNode(frontiers[index].Site, frontier.Site))
            {
                continue;
            }

            frontiers[index] = frontiers[index] with
            {
                Multiplicity = frontiers[index].Multiplicity + frontier.Multiplicity
            };
            return;
        }

        frontiers.Add(frontier);
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}

public readonly record struct PropagationFrontier(
    TopologyTrolley Trolley,
    Measure Multiplicity)
{
    public Site Site => Trolley.Site;
}

public readonly record struct PropagationMove(
    PropagationFrontier Source,
    PropagationFrontier Target)
{
    public Relation Relation => Target.Trolley.TrailRelations[^1];
}

public readonly record struct PropagationWaveStep(
    PropagationWave Before,
    PropagationWave After,
    ImmutableArray<PropagationMove> Moves)
{
    public bool HasMotion => !Moves.IsDefaultOrEmpty && Moves.Length != 0;
}
