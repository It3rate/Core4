using System;
using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Dynamics;

/// <summary>
/// One passive resistive span between two physical circuit nodes. The relation remains the Core4 support, while the
/// resistance only changes how strongly mismatch can transmit across it during local relaxation.
/// </summary>
public readonly record struct ResistiveBranch(Relation Relation, Ratio Resistance)
{
    public Ratio Conductance => Resistance.IsResolved && Resistance.Recessive != Measure.Zero
        ? new Ratio(Resistance.Dominant.Absolute, Resistance.Recessive.Absolute).Canonicalize()
        : Ratio.Zero(Measure.One);

    public static ResistiveBranch Create(Relation relation, Measure resistance) =>
        new(relation, new Ratio(resistance, Measure.One).Canonicalize());
}

/// <summary>
/// A fixed site-local potential used to bias a passive network.
/// </summary>
public readonly record struct ResistiveBoundary(Site Site, Ratio Potential)
{
    public static ResistiveBoundary Create(Site site, Measure potential) =>
        new(site, new Ratio(potential, Measure.One).Canonicalize());
}

/// <summary>
/// One physical circuit node derived from connected topology. Several carrier-local sites may collapse into one node
/// when they pin to the same junction.
/// </summary>
public readonly record struct ResistiveNode(
    Placement Key,
    ImmutableArray<Site> Sites,
    bool IsFixed,
    Ratio FixedPotential);

/// <summary>
/// A passive resistive network derived from real Core4 topology. The network stays entirely in Core4 terms:
/// placements identify nodes, relations are branches, and site potentials relax by lowering mismatch numerically.
/// </summary>
public readonly record struct ResistiveCircuit(
    ConnectedSiteTopology Topology,
    ImmutableArray<ResistiveNode> Nodes,
    ImmutableArray<ResolvedResistiveBranch> Branches,
    ImmutableDictionary<Placement, Placement> NodeLookup)
{
    public static ResistiveCircuit Create(
        ConnectedSiteTopology topology,
        IEnumerable<ResistiveBranch> branches,
        IEnumerable<ResistiveBoundary> boundaries)
    {
        var configured = ImmutableDictionary.CreateBuilder<Relation, Ratio>();

        foreach (var branch in branches)
        {
            configured[branch.Relation] = branch.Resistance.Canonicalize();
        }

        var boundaryValues = ImmutableDictionary.CreateBuilder<Placement, Ratio>();

        foreach (var boundary in boundaries)
        {
            boundaryValues[boundary.Site.Placement] = boundary.Potential.Canonicalize();
        }

        var groups = BuildGroups(topology);
        var nodes = ImmutableArray.CreateBuilder<ResistiveNode>(groups.Length);
        var lookup = ImmutableDictionary.CreateBuilder<Placement, Placement>();

        foreach (var group in groups)
        {
            var representative = SelectRepresentative(topology, group, boundaryValues);
            var key = representative.Placement;
            var fixedPotential = FixedPotential(group, boundaryValues);
            var isFixed = fixedPotential.IsResolved;
            var sites = ImmutableArray.CreateBuilder<Site>(group.Count);

            foreach (var site in group)
            {
                sites.Add(site);
                lookup[site.Placement] = key;
            }

            nodes.Add(new ResistiveNode(key, sites.ToImmutable(), isFixed, fixedPotential));
        }

        var resolved = ImmutableArray.CreateBuilder<ResolvedResistiveBranch>();

        foreach (var relation in UniqueRelations(topology))
        {
            var from = lookup[relation.Origin.Placement];
            var to = lookup[relation.Anchor.Placement];

            if (from == to)
            {
                continue;
            }

            var resistance = configured.TryGetValue(relation, out var configuredResistance)
                ? configuredResistance
                : new Ratio(Measure.One, Measure.One);
            resolved.Add(new ResolvedResistiveBranch(relation, from, to, resistance.Canonicalize()));
        }

        return new ResistiveCircuit(
            topology,
            nodes.ToImmutable(),
            resolved.ToImmutable(),
            lookup.ToImmutable());
    }

    public ResistiveCircuitState Seed()
    {
        var potentials = ImmutableDictionary.CreateBuilder<Placement, Ratio>();
        var defaultPotential = AverageFixedPotential();

        foreach (var node in Nodes)
        {
            potentials[node.Key] = node.IsFixed
                ? node.FixedPotential.Canonicalize()
                : defaultPotential;
        }

        return new ResistiveCircuitState(this, potentials.ToImmutable(), Measure.Zero, Ratio.Zero(Measure.One));
    }

    public Placement NodeKeyOf(Site site) => NodeLookup.TryGetValue(site.Placement, out var key)
        ? key
        : site.Placement;

    private Ratio AverageFixedPotential()
    {
        var total = Ratio.Zero(Measure.One);
        var count = Measure.Zero;

        foreach (var node in Nodes)
        {
            if (!node.IsFixed)
            {
                continue;
            }

            total = total.Add(node.FixedPotential).Canonicalize();
            count += Measure.One;
        }

        return count == Measure.Zero
            ? Ratio.Zero(Measure.One)
            : total.Divide(new Ratio(count, Measure.One)).Canonicalize();
    }

    private static ImmutableArray<List<Site>> BuildGroups(ConnectedSiteTopology topology)
    {
        var groups = new List<List<Site>>();

        foreach (var chain in topology.Chains())
        {
            foreach (var node in chain.Nodes)
            {
                MergeGroup(groups, SitesFor(node));
            }
        }

        foreach (var site in topology.Sites)
        {
            MergeGroup(groups, [site]);
        }

        return [.. groups];
    }

    private static List<Site> SitesFor(CarrierChainNode node)
    {
        var sites = new List<Site> { node.Site };

        foreach (var attached in node.AttachedSites)
        {
            AddUnique(sites, attached);
        }

        return sites;
    }

    private static void MergeGroup(List<List<Site>> groups, List<Site> members)
    {
        if (members.Count == 0)
        {
            return;
        }

        var merged = new List<Site>();
        var indexes = new List<int>();

        foreach (var member in members)
        {
            AddUnique(merged, member);
        }

        for (var index = 0; index < groups.Count; index++)
        {
            if (!SharesAny(groups[index], merged))
            {
                continue;
            }

            indexes.Add(index);

            foreach (var site in groups[index])
            {
                AddUnique(merged, site);
            }
        }

        for (var index = indexes.Count - 1; index >= 0; index--)
        {
            groups.RemoveAt(indexes[index]);
        }

        groups.Add(merged);
    }

    private static bool SharesAny(List<Site> left, List<Site> right)
    {
        foreach (var site in left)
        {
            if (Contains(right, site))
            {
                return true;
            }
        }

        return false;
    }

    private static Site SelectRepresentative(
        ConnectedSiteTopology topology,
        List<Site> group,
        ImmutableDictionary<Placement, Ratio>.Builder boundaryValues)
    {
        var best = group[0];
        var bestPriority = RepresentativePriority(topology, best, boundaryValues);

        for (var index = 1; index < group.Count; index++)
        {
            var candidate = group[index];
            var priority = RepresentativePriority(topology, candidate, boundaryValues);

            if (priority > bestPriority)
            {
                best = candidate;
                bestPriority = priority;
            }
        }

        return best;
    }

    private static long RepresentativePriority(
        ConnectedSiteTopology topology,
        Site site,
        ImmutableDictionary<Placement, Ratio>.Builder boundaryValues)
    {
        long priority = 0L;

        if (boundaryValues.ContainsKey(site.Placement))
        {
            priority += 1_000_000L;
        }

        var connector = topology.ConnectorAt(site);
        priority += connector.Carriers.Length * 1_000L;
        priority += topology.IncidenceAt(site).LegCount.Raw * 10L;
        priority -= site.Carrier.Id;
        return priority;
    }

    private static Ratio FixedPotential(
        List<Site> group,
        ImmutableDictionary<Placement, Ratio>.Builder boundaryValues)
    {
        foreach (var site in group)
        {
            if (boundaryValues.TryGetValue(site.Placement, out var potential))
            {
                return potential.Canonicalize();
            }
        }

        return Ratio.Tension(Measure.Zero);
    }

    private static ImmutableArray<Relation> UniqueRelations(ConnectedSiteTopology topology)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var chain in topology.Chains())
        {
            foreach (var relation in chain.Relations)
            {
                if (!relations.Contains(relation))
                {
                    relations.Add(relation);
                }
            }
        }

        return relations.ToImmutable();
    }

    private static void AddUnique(List<Site> sites, Site site)
    {
        if (!Contains(sites, site))
        {
            sites.Add(site);
        }
    }

    private static bool Contains(List<Site> sites, Site site)
    {
        foreach (var known in sites)
        {
            if (known.Carrier == site.Carrier && known.Location == site.Location)
            {
                return true;
            }
        }

        return false;
    }
}

public readonly record struct ResolvedResistiveBranch(
    Relation Relation,
    Placement From,
    Placement To,
    Ratio Resistance)
{
    public Ratio Conductance => Resistance.IsResolved && Resistance.Recessive != Measure.Zero
        ? new Ratio(Resistance.Dominant.Absolute, Resistance.Recessive.Absolute).Canonicalize()
        : Ratio.Zero(Measure.One);
}

/// <summary>
/// One relaxed potential field over a passive resistive circuit. Each step lowers node mismatch by replacing every
/// non-fixed node with the conductance-weighted average of its neighbors.
/// </summary>
public readonly record struct ResistiveCircuitState(
    ResistiveCircuit Circuit,
    ImmutableDictionary<Placement, Ratio> Potentials,
    Measure Iteration,
    Ratio MaxResidual)
{
    private static readonly Measure RelaxationResolution = new(4096);

    public Ratio PotentialAt(Site site) => PotentialAt(Circuit.NodeKeyOf(site));

    public Ratio PotentialAt(Placement key) => Potentials.TryGetValue(key, out var potential)
        ? potential.Canonicalize()
        : Ratio.Zero(Measure.One);

    public Ratio DropOn(ResolvedResistiveBranch branch) =>
        PotentialAt(branch.From).Subtract(PotentialAt(branch.To)).Canonicalize();

    public Ratio CurrentOn(ResolvedResistiveBranch branch) =>
        branch.Resistance.Recessive == Measure.Zero
            ? Ratio.Tension(DropOn(branch).Potential)
            : DropOn(branch).Divide(branch.Resistance).Canonicalize();

    public Ratio DissipationOn(ResolvedResistiveBranch branch) =>
        Absolute(CurrentOn(branch).Multiply(DropOn(branch)).Canonicalize());

    public ResistiveCircuitState Relax()
    {
        var next = Potentials.ToBuilder();
        var maxResidual = Ratio.Zero(Measure.One);

        foreach (var node in Circuit.Nodes)
        {
            if (node.IsFixed)
            {
                next[node.Key] = node.FixedPotential.Canonicalize();
                continue;
            }

            var weighted = 0d;
            var total = 0d;

            foreach (var branch in Circuit.Branches)
            {
                var isFrom = branch.From == node.Key;
                var isTo = branch.To == node.Key;

                if (!isFrom && !isTo)
                {
                    continue;
                }

                var neighbor = isFrom ? branch.To : branch.From;
                weighted += PotentialAt(neighbor).Approximate * branch.Conductance.Approximate;
                total += branch.Conductance.Approximate;
            }

            var resolved = total <= 0d
                ? PotentialAt(node.Key)
                : Quantize(weighted / total);
            next[node.Key] = resolved;

            var residual = Absolute(resolved.Subtract(PotentialAt(node.Key)).Canonicalize());

            if (Compare(residual, maxResidual) > 0)
            {
                maxResidual = residual;
            }
        }

        return new ResistiveCircuitState(
            Circuit,
            next.ToImmutable(),
            Iteration + Measure.One,
            maxResidual);
    }

    public ResistiveCircuitState Relax(Measure steps)
    {
        var state = this;

        for (var index = 0L; index < steps.Raw; index++)
        {
            state = state.Relax();
        }

        return state;
    }

    public ResistiveCircuitState Settle(Measure maxSteps)
    {
        var state = this;

        for (var index = 0L; index < maxSteps.Raw; index++)
        {
            state = state.Relax();

            if (state.MaxResidual.Approximate <= 0.000_001d)
            {
                break;
            }
        }

        return state;
    }

    private static Ratio Absolute(Ratio value) => value.IsTension
        ? Ratio.Tension(value.Potential)
        : new Ratio(value.Recessive.Absolute, value.Dominant.Absolute).Canonicalize();

    private static int Compare(Ratio left, Ratio right) => left.Approximate.CompareTo(right.Approximate);

    private static Ratio Quantize(double value)
    {
        var scaled = value * RelaxationResolution.Raw;

        if (double.IsNaN(scaled) || double.IsInfinity(scaled) || scaled > long.MaxValue || scaled < long.MinValue)
        {
            throw new OverflowException("Relaxed circuit potential exceeded the current Measure range.");
        }

        return new Ratio(new Measure((long)Math.Round(scaled)), RelaxationResolution).Canonicalize();
    }
}
