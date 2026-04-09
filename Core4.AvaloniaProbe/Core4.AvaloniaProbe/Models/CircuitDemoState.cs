using System.Collections.Generic;
using System.Collections.Immutable;
using Avalonia;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// One circuit-like topology preset backed by real Core4 relations plus fixed source nodes and resistive branch
/// weights. The same topology can be read under either source polarity by swapping the fixed node potentials.
/// </summary>
public readonly record struct CircuitDemoState(
    string Label,
    string Description,
    ConnectedSiteTopology Topology,
    ImmutableArray<ResistiveBranch> Branches,
    Site PositiveSite,
    Site NegativeSite,
    Measure SourcePotential,
    ImmutableDictionary<Site, Point> SeedPositions,
    ImmutableDictionary<Site, string> SiteLabels)
{
    public static CircuitDemoState BalancedLoop => CreateLoop(
        "Balanced Loop",
        "A fixed source drives one source lead, two equal parallel paths, and one tail resistor. The diamond between split and merge acts like a first passive loop.",
        upperLeft: new Measure(2),
        upperRight: new Measure(2),
        lowerLeft: new Measure(2),
        lowerRight: new Measure(2),
        lead: Measure.One,
        tail: Measure.One);

    public static CircuitDemoState UnevenLoop => CreateLoop(
        "Uneven Loop",
        "One looped branch is easier to traverse than the other, so the split still closes at the merge but the branch drops and midpoint potentials settle differently.",
        upperLeft: Measure.One,
        upperRight: new Measure(3),
        lowerLeft: new Measure(2),
        lowerRight: new Measure(2),
        lead: Measure.One,
        tail: Measure.One);

    public ResistiveCircuit CreateCircuit(bool reversed)
    {
        var high = reversed ? NegativeSite : PositiveSite;
        var low = reversed ? PositiveSite : NegativeSite;

        return ResistiveCircuit.Create(
            Topology,
            Branches,
            [
                ResistiveBoundary.Create(high, SourcePotential),
                ResistiveBoundary.Create(low, Measure.Zero)
            ]);
    }

    public ImmutableDictionary<Placement, Point> NodePositions(ResistiveCircuit circuit)
    {
        var positions = ImmutableDictionary.CreateBuilder<Placement, Point>();

        foreach (var node in circuit.Nodes)
        {
            var totalX = 0d;
            var totalY = 0d;
            var count = 0d;

            foreach (var site in node.Sites)
            {
                if (!SeedPositions.TryGetValue(site, out var point))
                {
                    continue;
                }

                totalX += point.X;
                totalY += point.Y;
                count += 1d;
            }

            positions[node.Key] = count == 0d
                ? new Point(0.5d, 0.5d)
                : new Point(totalX / count, totalY / count);
        }

        return positions.ToImmutable();
    }

    public string NodeLabel(ResistiveNode node)
    {
        foreach (var site in node.Sites)
        {
            if (SiteLabels.TryGetValue(site, out var label))
            {
                return label;
            }
        }

        return $"{CarrierLabel(node.Key.PrimaryCarrier)}{node.Key.On(node.Key.PrimaryCarrier).Whole.Raw}";
    }

    private static CircuitDemoState CreateLoop(
        string label,
        string description,
        Measure upperLeft,
        Measure upperRight,
        Measure lowerLeft,
        Measure lowerRight,
        Measure lead,
        Measure tail)
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var source = Site.Open(x, new Measure(0));
        var split = Site.Open(x, new Measure(4));
        var sink = Site.Open(x, new Measure(18));

        var upperEntry = Site.Open(y, new Measure(0));
        var upperMid = Site.Open(y, new Measure(4));
        var upperExit = Site.Open(y, new Measure(8));
        var lowerEntry = Site.Open(z, new Measure(0));
        var lowerMid = Site.Open(z, new Measure(4));
        var lowerExit = Site.Open(z, new Measure(8));

        var topLeft = new Relation(upperEntry, upperMid);
        var topRight = new Relation(y, upperMid, upperExit);
        var bottomLeft = new Relation(lowerEntry, lowerMid);
        var bottomRight = new Relation(z, lowerMid, lowerExit);
        var merge = Site.Open(x, new Measure(12))
            .Request(topRight, Ratio.Tension(upperRight))
            .Request(bottomRight, Ratio.Tension(lowerRight));
        split = split
            .Publish(topLeft, Ratio.Tension(upperLeft))
            .Publish(bottomLeft, Ratio.Tension(lowerLeft));

        var sourceLead = new Relation(source, split);
        var tailLead = new Relation(merge, sink);

        return new CircuitDemoState(
            label,
            description,
            ConnectedSiteTopology.Create(
                [split, merge],
                sourceLead,
                tailLead),
            [
                ResistiveBranch.Create(sourceLead, lead),
                ResistiveBranch.Create(topLeft, upperLeft),
                ResistiveBranch.Create(topRight, upperRight),
                ResistiveBranch.Create(bottomLeft, lowerLeft),
                ResistiveBranch.Create(bottomRight, lowerRight),
                ResistiveBranch.Create(tailLead, tail)
            ],
            source,
            sink,
            new Measure(12),
            Seed(
                (source, 0.14d, 0.54d),
                (split, 0.30d, 0.54d),
                (upperEntry, 0.30d, 0.54d),
                (upperMid, 0.46d, 0.28d),
                (upperExit, 0.62d, 0.54d),
                (lowerEntry, 0.30d, 0.54d),
                (lowerMid, 0.46d, 0.80d),
                (lowerExit, 0.62d, 0.54d),
                (merge, 0.62d, 0.54d),
                (sink, 0.84d, 0.54d)),
            new Dictionary<Site, string>
            {
                [source] = "V+",
                [split] = "split",
                [upperMid] = "top",
                [lowerMid] = "bottom",
                [merge] = "merge",
                [sink] = "V-"
            }.ToImmutableDictionary());
    }

    private static ImmutableDictionary<Site, Point> Seed(params (Site Site, double X, double Y)[] items)
    {
        var builder = ImmutableDictionary.CreateBuilder<Site, Point>();

        foreach (var (site, x, y) in items)
        {
            builder[site] = new Point(x, y);
        }

        return builder.ToImmutable();
    }

    private static string CarrierLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "x",
        2L => "y",
        3L => "z",
        4L => "w",
        _ => $"c{carrier.Id}"
    };
}
