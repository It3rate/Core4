using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class ConnectedSiteTopologyTests
{
    [Fact]
    public void ConnectedTopologyBuildsChainGeometryFromSharedRelations()
    {
        Carrier x = 1;
        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10));

        var topology = ConnectedSiteTopology.FromRelations(
            new Relation(left, middle),
            new Relation(middle, right));

        var incidence = topology.IncidenceAt(middle);
        var geometry = topology.GeometryAt(middle, x);
        var neighbors = topology.NeighborsOf(middle);

        Assert.Equal(new Measure(3), topology.SiteCount);
        Assert.Equal(new Measure(2), topology.RelationCount);
        Assert.Equal(new Measure(1), topology.CarrierCount);
        Assert.Equal(new Measure(2), neighbors.Length);
        Assert.Contains(left, neighbors);
        Assert.Contains(right, neighbors);
        Assert.True(incidence.Continues(x));
        Assert.True(geometry.IsInterior);
        Assert.True(geometry.IsCentered);
        Assert.Equal(new Ratio(5, 10), geometry.Position);
    }

    [Fact]
    public void ConnectedTopologyReadsTLikeJunctionFromChainAndOrthogonalStem()
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

        var incidence = topology.IncidenceAt(center);
        var geometry = topology.GeometryAt(center, x);
        var stemGeometry = topology.GeometryAt(center, y);
        var neighbors = topology.NeighborsOf(center);

        Assert.Equal(new Measure(3), topology.SiteCount);
        Assert.Equal(new Measure(3), topology.RelationCount);
        Assert.Equal(new Measure(2), topology.CarrierCount);
        Assert.Equal(new Measure(3), neighbors.Length);
        Assert.Contains(left, neighbors);
        Assert.Contains(right, neighbors);
        Assert.Contains(stemEnd, neighbors);
        Assert.True(incidence.Continues(x));
        Assert.True(incidence.Terminates(y));
        Assert.Equal(new Measure(1), incidence.ContinuingCarrierCount);
        Assert.Equal(new Measure(1), incidence.TerminalCarrierCount);
        Assert.True(geometry.IsInterior);
        Assert.True(geometry.IsCentered);
        Assert.True(stemGeometry.IsStartEndpoint);
        Assert.Equal(new Ratio(0, 4), stemGeometry.Position);
        Assert.Contains(center, topology.SitesOn(y));
    }

    [Fact]
    public void ConnectedTopologyReadsHLikeBridgeWithTwoJunctionSites()
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
        var right = Site.Open(x, new Measure(6))
            .Request(
                new Relation(rightStemLow, rightStemCenter),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(rightStemCenter, rightStemHigh),
                Ratio.Tension(new Measure(4)));

        var topology = ConnectedSiteTopology.Create(
            [left, right],
            new Relation(left, right));

        var leftIncidence = topology.IncidenceAt(left);
        var rightIncidence = topology.IncidenceAt(right);
        var leftGeometry = topology.GeometryAt(left, y);
        var neighbors = topology.NeighborsOf(left);

        Assert.Equal(new Measure(2), topology.SiteCount);
        Assert.Equal(new Measure(5), topology.RelationCount);
        Assert.Equal(new Measure(3), topology.CarrierCount);
        Assert.Equal(new Measure(3), neighbors.Length);
        Assert.Contains(right, neighbors);
        Assert.Contains(leftStemLow, neighbors);
        Assert.Contains(leftStemHigh, neighbors);
        Assert.True(leftIncidence.Terminates(x));
        Assert.True(rightIncidence.Terminates(x));
        Assert.True(leftIncidence.Continues(y));
        Assert.True(rightIncidence.Continues(z));
        Assert.Equal(new Measure(1), leftIncidence.ContinuingCarrierCount);
        Assert.Equal(new Measure(1), rightIncidence.ContinuingCarrierCount);
        Assert.True(leftGeometry.IsInterior);
        Assert.True(leftGeometry.IsCentered);
        Assert.Equal(new Ratio(4, 8), leftGeometry.Position);
    }
}
