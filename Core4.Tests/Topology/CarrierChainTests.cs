using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class CarrierChainTests
{
    [Fact]
    public void CarrierChainOrdersAlignedRunAndPreservesAttachedSites()
    {
        Carrier x = 1;
        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10));

        var chain = ConnectedSiteTopology.FromRelations(
            new Relation(left, middle),
            new Relation(middle, right))
            .ChainsOn(x)[0];

        Assert.Equal(new Measure(3), chain.NodeCount);
        Assert.Equal(new Measure(2), chain.RelationCount);
        Assert.Equal(new Measure(3), chain.AttachmentCount);
        Assert.Equal(new Measure(10), chain.TotalSpan);
        Assert.Equal(new Ratio(0, 1), chain.Start.Position);
        Assert.Equal(new Ratio(10, 1), chain.End.Position);
        Assert.Contains(middle, chain.Nodes[1].AttachedSites);
    }

    [Fact]
    public void CarrierChainProjectsOrthogonalAttachmentOntoCarrierLocalNode()
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

        var chain = ConnectedSiteTopology.Create(
            [center],
            new Relation(left, center),
            new Relation(center, right))
            .ChainsOn(y)[0];

        Assert.Equal(new Measure(2), chain.NodeCount);
        Assert.Equal(new Measure(1), chain.RelationCount);
        Assert.Equal(new Measure(1), chain.AttachmentCount);
        Assert.Equal(new Measure(4), chain.TotalSpan);
        Assert.Equal(new Ratio(0, 1), chain.Start.Position);
        Assert.Equal(new Ratio(4, 1), chain.End.Position);
        Assert.Contains(center, chain.Start.AttachedSites);
        Assert.Empty(chain.End.AttachedSites);
    }

    [Fact]
    public void CarrierChainSplitsDisconnectedComponentsOnSameCarrier()
    {
        Carrier y = 2;
        var first = new Relation(
            Site.Open(y, new Measure(0)),
            Site.Open(y, new Measure(4)));
        var second = new Relation(
            Site.Open(y, new Measure(10)),
            Site.Open(y, new Measure(14)));

        var chains = ConnectedSiteTopology.FromRelations(first, second).ChainsOn(y);

        Assert.Equal(2, chains.Length);
        Assert.Equal(new Measure(2), chains[0].NodeCount);
        Assert.Equal(new Measure(1), chains[0].RelationCount);
        Assert.Equal(new Measure(4), chains[0].TotalSpan);
        Assert.Equal(new Measure(2), chains[1].NodeCount);
        Assert.Equal(new Measure(1), chains[1].RelationCount);
        Assert.Equal(new Measure(4), chains[1].TotalSpan);
        Assert.Equal(new Ratio(0, 1), chains[0].Start.Position);
        Assert.Equal(new Ratio(10, 1), chains[1].Start.Position);
    }

    [Fact]
    public void CarrierChainKeepsDistinctExactNodesThatShareWholeLocation()
    {
        Carrier y = 2;
        var first = new Relation(
            Site.Open(y.At(new Ratio(new Measure(1), new Measure(4)))),
            Site.Open(y.At(new Ratio(new Measure(2), new Measure(4)))));
        var second = new Relation(
            Site.Open(y.At(new Ratio(new Measure(3), new Measure(4)))),
            Site.Open(y.At(new Ratio(new Measure(4), new Measure(4)))));

        var chains = ConnectedSiteTopology.FromRelations(first, second).ChainsOn(y);

        Assert.Equal(2, chains.Length);
        Assert.Equal(new Measure(2), chains[0].NodeCount);
        Assert.Equal(new Measure(1), chains[0].RelationCount);
        Assert.Equal(new Ratio(new Measure(1), new Measure(4)), chains[0].Start.Position);
        Assert.Equal(new Ratio(new Measure(2), new Measure(4)), chains[0].End.Position);
        Assert.Equal(new Measure(2), chains[1].NodeCount);
        Assert.Equal(new Measure(1), chains[1].RelationCount);
        Assert.Equal(new Ratio(new Measure(3), new Measure(4)), chains[1].Start.Position);
        Assert.Equal(new Ratio(new Measure(4), new Measure(4)), chains[1].End.Position);
    }
}
