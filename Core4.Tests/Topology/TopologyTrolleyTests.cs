using Core4.Inspection;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class TopologyTrolleyTests
{
    [Fact]
    public void TrolleyPrefersSameCarrierContinuationThroughTJunction()
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

        var topology = ConnectedSiteTopology.Create(
            [center],
            new Relation(left, center),
            new Relation(center, right));

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.Equal(center.Carrier, trolley.Site.Carrier);
        Assert.Equal(center.Location, trolley.Site.Location);

        var observation = trolley.Observe();

        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.Equal(right.Carrier, preferred.To.Carrier);
        Assert.Equal(right.Location, preferred.To.Location);

        var forks = trolley.ForkAll();

        Assert.Equal(2, forks.Length);
        Assert.Contains(forks, fork => SameNode(fork.Site, right));
        Assert.Contains(forks, fork => SameNode(fork.Site, stemEnd));
    }

    [Fact]
    public void TrolleyTurnsAutomaticallyAtSimpleLCorner()
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

        var topology = ConnectedSiteTopology.Create(
            [corner],
            new Relation(left, corner));

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));

        Assert.True(SameNode(trolley.Site, top));
        Assert.False(trolley.TryContinuePreferred(out _));
        Assert.True(trolley.TryReverse(out var reversed));
        Assert.True(SameNode(reversed.Site, corner));
        Assert.Single(reversed.TrailRelations);
        Assert.Equal(2, reversed.TrailSites.Length);
    }

    [Fact]
    public void TrolleySurfacesBranchChoicesAtYJunction()
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

        var topology = ConnectedSiteTopology.Create(
            [center],
            new Relation(stemBase, center));

        var trolley = TopologyTrolley.Start(topology, stemBase);
        Assert.True(trolley.TryContinuePreferred(out trolley));

        var observation = trolley.Observe();

        Assert.True(observation.IsDecisionPoint);
        Assert.False(observation.TryPreferred(out _));

        var forks = trolley.ForkAll();

        Assert.Equal(2, forks.Length);
        Assert.Contains(forks, fork => SameNode(fork.Site, leftEnd));
        Assert.Contains(forks, fork => SameNode(fork.Site, rightEnd));
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}
