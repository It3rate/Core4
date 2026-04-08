using System.Collections.Immutable;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// One topology demo preset backed by a real connected Core4 topology plus seed positions for the visual hubs.
/// The visible seed positions are only view hints; the tension read can compare those against a hidden rest layout
/// when a sample wants live tension without visibly distorting its starting shape.
/// </summary>
public readonly record struct CarrierChainDemoState(
    string Label,
    string Description,
    ConnectedSiteTopology Topology,
    Site FocusSite,
    ImmutableDictionary<Site, Point> SeedPositions)
{
    public const string DiamondCrossLabel = "Diamond Cross";

    public SiteConnector FocusConnector => Topology.ConnectorAt(FocusSite);

    public ImmutableArray<CarrierChain> Chains => Topology.Chains();

    public ImmutableDictionary<Site, Point> DesiredSeedPositions => Label == DiamondCrossLabel
        ? DiamondCrossDesiredSeeds(SeedPositions)
        : SeedPositions;

    public static CarrierChainDemoState TJunction => CreateTJunction();

    public static CarrierChainDemoState LCorner => CreateLCorner();

    public static CarrierChainDemoState YBranch => CreateYBranch();

    public static CarrierChainDemoState HBridge => CreateHBridge();

    public static CarrierChainDemoState FanIn => CreateFanIn();

    public static CarrierChainDemoState Triangle => CreateTriangle();

    public static CarrierChainDemoState DiamondCross => CreateDiamondCross();

    private static CarrierChainDemoState CreateTJunction()
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Site.Open(x, new Measure(0));
        var stemStart = Site.Open(y, new Measure(0));
        var stemEnd = Site.Open(y, new Measure(4));
        var center = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(stemStart, stemEnd),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(10));

        return new CarrierChainDemoState(
            "T Junction",
            "A horizontal carrier continues through the center while one orthogonal strand publishes upward with live tension.",
            ConnectedSiteTopology.Create(
                [center],
                new Relation(left, center),
                new Relation(center, right)),
            center,
            Seed(
                (left, 0.14d, 0.64d),
                (center, 0.50d, 0.64d),
                (right, 0.86d, 0.64d),
                (stemStart, 0.50d, 0.64d),
                (stemEnd, 0.50d, 0.24d)));
    }

    private static CarrierChainDemoState CreateLCorner()
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Site.Open(x, new Measure(0));
        var verticalBase = Site.Open(y, new Measure(0));
        var top = Site.Open(y, new Measure(5));
        var corner = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(verticalBase, top),
                Ratio.Tension(new Measure(5)));

        return new CarrierChainDemoState(
            "L Corner",
            "One aligned run terminates at a corner site, then turns into one orthogonal strand that climbs away from the same pinned place.",
            ConnectedSiteTopology.Create(
                [corner],
                new Relation(left, corner)),
            corner,
            Seed(
                (left, 0.18d, 0.68d),
                (corner, 0.50d, 0.68d),
                (verticalBase, 0.50d, 0.68d),
                (top, 0.50d, 0.24d)));
    }

    private static CarrierChainDemoState CreateYBranch()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var stemBase = Site.Open(y, new Measure(-6));
        var leftJoin = Site.Open(x, new Measure(0));
        var leftEnd = Site.Open(x, new Measure(-4));
        var rightJoin = Site.Open(z, new Measure(0));
        var rightEnd = Site.Open(z, new Measure(4));
        var center = Site.Open(y, new Measure(0))
            .Publish(
                new Relation(leftJoin, leftEnd),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(z, rightJoin, rightEnd),
                Ratio.Tension(new Measure(4)));

        return new CarrierChainDemoState(
            "Y Branch",
            "One stem rises into a shared site and then splits across two terminal orthogonal strands, giving a first branch-like primitive.",
            ConnectedSiteTopology.Create(
                [center],
                new Relation(stemBase, center)),
            center,
            Seed(
                (stemBase, 0.50d, 0.84d),
                (center, 0.50d, 0.55d),
                (leftJoin, 0.50d, 0.55d),
                (leftEnd, 0.28d, 0.24d),
                (rightJoin, 0.50d, 0.55d),
                (rightEnd, 0.72d, 0.24d)));
    }

    private static CarrierChainDemoState CreateHBridge()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var leftStemLow = Site.Open(y, new Measure(-4));
        var leftStemCenter = Site.Open(y, new Measure(0));
        var leftStemHigh = Site.Open(y, new Measure(4));
        var left = Site.Open(x, new Measure(0))
            .Request(
                new Relation(leftStemLow, leftStemCenter),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(leftStemCenter, leftStemHigh),
                Ratio.Tension(new Measure(4)));

        var rightStemLow = Site.Open(z, new Measure(-4));
        var rightStemCenter = Site.Open(z, new Measure(0));
        var rightStemHigh = Site.Open(z, new Measure(4));
        var right = Site.Open(x, new Measure(8))
            .Request(
                new Relation(rightStemLow, rightStemCenter),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(rightStemCenter, rightStemHigh),
                Ratio.Tension(new Measure(4)));

        return new CarrierChainDemoState(
            "H Bridge",
            "Two topology sites are joined by one carrier while each site hardens its own orthogonal through-line above and below the bridge.",
            ConnectedSiteTopology.Create(
                [left, right],
                new Relation(left, right)),
            left,
            Seed(
                (left, 0.28d, 0.55d),
                (right, 0.72d, 0.55d),
                (leftStemLow, 0.28d, 0.82d),
                (leftStemCenter, 0.28d, 0.55d),
                (leftStemHigh, 0.28d, 0.28d),
                (rightStemLow, 0.72d, 0.82d),
                (rightStemCenter, 0.72d, 0.55d),
                (rightStemHigh, 0.72d, 0.28d)));
    }

    private static CarrierChainDemoState CreateFanIn()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var left = Site.Open(x, new Measure(0));
        var center = Site.Open(x, new Measure(5))
            .Request(
                new Relation(
                    Site.Open(y, new Measure(-4)),
                    Site.Open(y, new Measure(0))),
                Ratio.Tension(new Measure(4)))
            .Request(
                new Relation(
                    Site.Open(z, new Measure(-4)),
                    Site.Open(z, new Measure(0))),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(10));
        var inputA = Site.Open(y, new Measure(-4));
        var inputAJoin = Site.Open(y, new Measure(0));
        var inputB = Site.Open(z, new Measure(-4));
        var inputBJoin = Site.Open(z, new Measure(0));

        return new CarrierChainDemoState(
            "Fan In",
            "Two orthogonal inputs negotiate into one aligned carrier, giving a first look at merge-like structure around a single shared site.",
            ConnectedSiteTopology.Create(
                [center],
                new Relation(left, center),
                new Relation(center, right)),
            center,
            Seed(
                (left, 0.16d, 0.36d),
                (center, 0.50d, 0.36d),
                (right, 0.84d, 0.36d),
                (inputA, 0.34d, 0.76d),
                (inputAJoin, 0.50d, 0.36d),
                (inputB, 0.66d, 0.76d),
                (inputBJoin, 0.50d, 0.36d)));
    }

    private static CarrierChainDemoState CreateTriangle()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var leftJoin = Site.Open(y, new Measure(0));
        var rightJoin = Site.Open(z, new Measure(0));
        var apex = Site.Open(y, new Measure(6));
        var apexJoin = Site.Open(z, new Measure(6));
        var leftSlope = new Relation(leftJoin, apex);
        var rightSlope = new Relation(z, apexJoin, rightJoin);
        var left = Site.Open(x, new Measure(0))
            .Publish(
                leftSlope,
                Ratio.Tension(new Measure(6)));
        var right = Site.Open(x, new Measure(8))
            .Request(
                rightSlope,
                Ratio.Tension(new Measure(6)));
        apex = apex.Publish(
            rightSlope,
            Ratio.Tension(new Measure(6)));

        return new CarrierChainDemoState(
            "Triangle",
            "Three terminal connectors close into one three-side loop: an aligned base and two negotiated strands that meet at the apex.",
            ConnectedSiteTopology.Create(
                [left, right, apex],
                new Relation(left, right)),
            apex,
            Seed(
                (left, 0.24d, 0.74d),
                (right, 0.76d, 0.74d),
                (leftJoin, 0.24d, 0.74d),
                (apex, 0.50d, 0.22d),
                (apexJoin, 0.50d, 0.22d),
                (rightJoin, 0.76d, 0.74d)));
    }

    private static CarrierChainDemoState CreateDiamondCross()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;
        Carrier w = 4;

        var left = Site.Open(x, new Measure(0));
        var entry = Site.Open(x, new Measure(4))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(
                    Site.Open(z, new Measure(0)),
                    Site.Open(z, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var upperApex = Site.Open(y, new Measure(4));
        var upperExit = Site.Open(y, new Measure(8));
        var lowerApex = Site.Open(z, new Measure(4));
        var lowerExit = Site.Open(z, new Measure(8));
        var merge = Site.Open(x, new Measure(12))
            .Request(
                new Relation(y, upperApex, upperExit),
                Ratio.Tension(new Measure(4)))
            .Request(
                new Relation(z, lowerApex, lowerExit),
                Ratio.Tension(new Measure(4)));

        var verticalLow = Site.Open(w, new Measure(-4));
        var verticalMid = Site.Open(w, new Measure(0));
        var verticalHigh = Site.Open(w, new Measure(4));
        var cross = Site.Open(x, new Measure(18))
            .Request(
                new Relation(verticalLow, verticalMid),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(verticalMid, verticalHigh),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(22));

        var upperEntry = Site.Open(y, new Measure(0));
        var lowerEntry = Site.Open(z, new Measure(0));

        return new CarrierChainDemoState(
            DiamondCrossLabel,
            "One carrier enters a diamond split, rejoins on the far side, then continues into a vertical through-site so one walk expresses branch, merge, continuation, and crossing.",
            ConnectedSiteTopology.Create(
                [entry, merge, cross],
                new Relation(left, entry),
                new Relation(merge, cross),
                new Relation(cross, right)),
            merge,
            Seed(
                (left, 0.10d, 0.54d),
                (entry, 0.28d, 0.54d),
                (upperEntry, 0.28d, 0.54d),
                (upperApex, 0.42d, 0.30d),
                (upperExit, 0.56d, 0.54d),
                (lowerEntry, 0.28d, 0.54d),
                (lowerApex, 0.42d, 0.78d),
                (lowerExit, 0.56d, 0.54d),
                (merge, 0.56d, 0.54d),
                (cross, 0.78d, 0.56d),
                (right, 0.90d, 0.54d),
                (verticalLow, 0.78d, 0.86d),
                (verticalMid, 0.78d, 0.56d),
                (verticalHigh, 0.78d, 0.14d)));
    }

    private static ImmutableDictionary<Site, Point> DiamondCrossDesiredSeeds(ImmutableDictionary<Site, Point> visible)
    {
        var builder = visible.ToBuilder();

        foreach (var (site, point) in visible)
        {
            builder[site] = site.Carrier.Id switch
            {
                1L when site.Location == new Measure(0) => new Point(0.02d, 0.54d),
                1L when site.Location == new Measure(4) => new Point(0.28d, 0.54d),
                2L when site.Location == new Measure(0) => new Point(0.28d, 0.54d),
                3L when site.Location == new Measure(0) => new Point(0.28d, 0.54d),
                2L when site.Location == new Measure(4) => new Point(0.36d, 0.40d),
                3L when site.Location == new Measure(4) => new Point(0.36d, 0.68d),
                1L when site.Location == new Measure(12) => new Point(0.66d, 0.54d),
                2L when site.Location == new Measure(8) => new Point(0.66d, 0.54d),
                3L when site.Location == new Measure(8) => new Point(0.66d, 0.54d),
                1L when site.Location == new Measure(18) => new Point(0.82d, 0.56d),
                4L when site.Location == new Measure(0) => new Point(0.82d, 0.56d),
                1L when site.Location == new Measure(22) => new Point(0.95d, 0.54d),
                4L when site.Location == new Measure(-4) => new Point(0.82d, 0.78d),
                4L when site.Location == new Measure(4) => new Point(0.82d, 0.04d),
                _ => point
            };
        }

        return builder.ToImmutable();
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
}
