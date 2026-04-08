using Core4.Inspection;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class PropagationWaveTests
{
    [Fact]
    public void WaveParksTerminiThenReturnsOnlyAfterOutboundQuiescence()
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

        var wave = PropagationWave.Start(topology, stemBase);

        var intoCenter = wave.Advance();
        Assert.True(intoCenter.HasMotion);
        Assert.True(intoCenter.After.IsOutbound);
        Assert.Single(intoCenter.After.Active);
        Assert.True(SameNode(intoCenter.After.Active[0].Site, center));

        var intoBranches = intoCenter.After.Advance();
        Assert.True(intoBranches.HasMotion);
        Assert.True(intoBranches.After.IsOutbound);
        Assert.Equal(2, intoBranches.After.Active.Length);
        Assert.Contains(intoBranches.After.Active, frontier => SameNode(frontier.Site, leftEnd));
        Assert.Contains(intoBranches.After.Active, frontier => SameNode(frontier.Site, rightEnd));

        var phaseFlip = intoBranches.After.Advance();
        Assert.False(phaseFlip.HasMotion);
        Assert.True(phaseFlip.After.IsReturning);
        Assert.Equal(2, phaseFlip.After.Active.Length);
        Assert.Empty(phaseFlip.After.Parked);

        var mergeReturn = phaseFlip.After.Advance();
        Assert.True(mergeReturn.HasMotion);
        Assert.True(mergeReturn.After.IsReturning);
        var merged = Assert.Single(mergeReturn.After.Active);
        Assert.True(SameNode(merged.Site, center));
        Assert.Equal(new Measure(2), merged.Multiplicity);

        var home = mergeReturn.After.Advance();
        Assert.True(home.HasMotion);
        var returned = Assert.Single(home.After.Active);
        Assert.True(SameNode(returned.Site, stemBase));
        Assert.Equal(new Measure(2), returned.Multiplicity);

        var complete = home.After.Advance();
        Assert.False(complete.HasMotion);
        Assert.True(complete.After.IsComplete);
        var parkedHome = Assert.Single(complete.After.Parked);
        Assert.True(SameNode(parkedHome.Site, stemBase));
        Assert.Equal(new Measure(2), parkedHome.Multiplicity);
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}
