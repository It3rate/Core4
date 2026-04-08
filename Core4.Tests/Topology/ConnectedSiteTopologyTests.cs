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
    public void ConnectedTopologyReadsLCornerAsTwoTerminalCarriers()
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

        var incidence = topology.IncidenceAt(corner);
        var horizontal = topology.GeometryAt(corner, x);
        var vertical = topology.GeometryAt(corner, y);
        var neighbors = topology.NeighborsOf(corner);

        Assert.Equal(new Measure(2), topology.SiteCount);
        Assert.Equal(new Measure(2), topology.RelationCount);
        Assert.Equal(new Measure(2), topology.CarrierCount);
        Assert.Equal(new Measure(2), neighbors.Length);
        Assert.Contains(left, neighbors);
        Assert.Contains(top, neighbors);
        Assert.True(incidence.Terminates(x));
        Assert.True(incidence.Terminates(y));
        Assert.Equal(new Measure(2), incidence.TerminalCarrierCount);
        Assert.True(horizontal.IsEndEndpoint);
        Assert.True(vertical.IsStartEndpoint);
        Assert.Equal(new Ratio(0, 5), vertical.Position);
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

    [Fact]
    public void ConnectedTopologyReadsYBranchAsThreeTerminalCarriers()
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

        var incidence = topology.IncidenceAt(center);
        var stem = topology.GeometryAt(center, y);
        var leftBranch = topology.GeometryAt(center, x);
        var rightBranch = topology.GeometryAt(center, z);
        var neighbors = topology.NeighborsOf(center);

        Assert.Equal(new Measure(2), topology.SiteCount);
        Assert.Equal(new Measure(3), topology.RelationCount);
        Assert.Equal(new Measure(3), topology.CarrierCount);
        Assert.Equal(new Measure(3), neighbors.Length);
        Assert.Contains(stemBase, neighbors);
        Assert.Contains(leftEnd, neighbors);
        Assert.Contains(rightEnd, neighbors);
        Assert.Equal(new Measure(3), incidence.TerminalCarrierCount);
        Assert.True(stem.IsEndEndpoint);
        Assert.True(leftBranch.IsStartEndpoint);
        Assert.True(rightBranch.IsStartEndpoint);
    }

    [Fact]
    public void ConnectedTopologyKeepsTriangleLoopAsThreeConnectedCarrierRuns()
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

        var topology = ConnectedSiteTopology.Create(
            [left, right, apex],
            new Relation(left, right));

        var leftIncidence = topology.IncidenceAt(left);
        var apexIncidence = topology.IncidenceAt(apex);
        var chains = topology.Chains();
        var leftNeighbors = topology.NeighborsOf(left);
        var apexNeighbors = topology.NeighborsOf(apex);

        Assert.Equal(new Measure(3), topology.SiteCount);
        Assert.Equal(new Measure(3), topology.RelationCount);
        Assert.Equal(new Measure(3), topology.CarrierCount);
        Assert.Equal(3, chains.Length);
        Assert.Equal(new Measure(2), leftIncidence.TerminalCarrierCount);
        Assert.Equal(new Measure(2), apexIncidence.TerminalCarrierCount);
        Assert.Equal(new Measure(2), leftNeighbors.Length);
        Assert.Contains(right, leftNeighbors);
        Assert.Contains(Site.Open(y, new Measure(6)), leftNeighbors);
        Assert.Equal(new Measure(2), apexNeighbors.Length);
        Assert.Contains(leftJoin, apexNeighbors);
        Assert.Contains(rightJoin, apexNeighbors);
        Assert.True(topology.GeometryAt(left, x).IsStartEndpoint);
        Assert.True(topology.GeometryAt(apex, y).IsEndEndpoint);
        Assert.True(topology.GeometryAt(apex, z).IsStartEndpoint);
    }
}
