using System.Collections.Immutable;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// One topology demo preset backed by a real connected Core4 topology plus seed positions for the visual hubs.
/// The seed positions are only view hints; all structure, chains, and connector readings come from Core4 itself.
/// </summary>
public readonly record struct CarrierChainDemoState(
    string Label,
    string Description,
    ConnectedSiteTopology Topology,
    Site FocusSite,
    ImmutableDictionary<Site, Point> SeedPositions)
{
    public SiteConnector FocusConnector => Topology.ConnectorAt(FocusSite);

    public ImmutableArray<CarrierChain> Chains => Topology.Chains();

    public static CarrierChainDemoState TJunction => CreateTJunction();

    public static CarrierChainDemoState HBridge => CreateHBridge();

    public static CarrierChainDemoState FanIn => CreateFanIn();

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
