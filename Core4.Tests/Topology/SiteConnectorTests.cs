using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class SiteConnectorTests
{
    [Fact]
    public void PassThroughConnectorReadsOneCenteredInteriorCarrier()
    {
        Carrier x = 1;
        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10));

        var connector = ConnectedSiteTopology.FromRelations(
            new Relation(left, middle),
            new Relation(middle, right))
            .ConnectorAt(middle);

        Assert.Equal(new Measure(2), connector.NeighborCount);
        Assert.Equal(new Measure(1), connector.Incidence.CarrierCount);
        Assert.Equal(new Measure(1), connector.Incidence.ContinuingCarrierCount);
        Assert.Equal(Measure.Zero, connector.Incidence.TerminalCarrierCount);
        Assert.Equal(new Measure(1), connector.GeometryCarrierCount);
        Assert.Equal(new Measure(1), connector.InteriorCarrierCount);
        Assert.Equal(Measure.Zero, connector.EndpointCarrierCount);
        Assert.Equal(new Measure(1), connector.CenteredCarrierCount);
        Assert.Equal(Measure.Zero, connector.AsymmetricCarrierCount);
        Assert.Equal(new Ratio(5, 10), connector.SiteCarrierPosition);
        Assert.Equal(new Ratio(0, 10), connector.SiteCarrierBalance);
    }

    [Fact]
    public void TLikeConnectorReadsContinuationPlusOutgoingTerminal()
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Site.Open(x, new Measure(0));
        var center = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(10));

        var connector = ConnectedSiteTopology.Create(
            [center],
            new Relation(left, center),
            new Relation(center, right))
            .ConnectorAt(center);

        Assert.Equal(new Measure(2), connector.NeighborCount);
        Assert.Equal(new Measure(2), connector.Incidence.CarrierCount);
        Assert.Equal(new Measure(1), connector.Incidence.ContinuingCarrierCount);
        Assert.Equal(new Measure(1), connector.Incidence.TerminalCarrierCount);
        Assert.Equal(new Measure(1), connector.OutgoingTerminalCarrierCount);
        Assert.Equal(Measure.Zero, connector.IncomingTerminalCarrierCount);
        Assert.Equal(new Measure(1), connector.GeometryCarrierCount);
        Assert.Equal(new Measure(1), connector.InteriorCarrierCount);
        Assert.Equal(new Measure(1), connector.CenteredCarrierCount);
        Assert.Equal(new Ratio(5, 10), connector.SiteCarrierPosition);
    }

    [Fact]
    public void EndpointBridgeConnectorReadsTerminalSiteCarrierAndContinuingCrossbar()
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Site.Open(x, new Measure(0))
            .Request(
                new Relation(
                    Site.Open(y, new Measure(-4)),
                    Site.Open(y, new Measure(0))),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var right = Site.Open(x, new Measure(6));

        var connector = ConnectedSiteTopology.Create(
            [left],
            new Relation(left, right))
            .ConnectorAt(left);

        Assert.Equal(new Measure(1), connector.NeighborCount);
        Assert.Equal(new Measure(2), connector.Incidence.CarrierCount);
        Assert.Equal(new Measure(1), connector.Incidence.ContinuingCarrierCount);
        Assert.Equal(new Measure(1), connector.Incidence.TerminalCarrierCount);
        Assert.Equal(Measure.Zero, connector.IncomingTerminalCarrierCount);
        Assert.Equal(new Measure(1), connector.OutgoingTerminalCarrierCount);
        Assert.Equal(new Measure(1), connector.GeometryCarrierCount);
        Assert.Equal(Measure.Zero, connector.InteriorCarrierCount);
        Assert.Equal(new Measure(1), connector.EndpointCarrierCount);
        Assert.Equal(new Ratio(0, 6), connector.SiteCarrierPosition);
        Assert.Equal(new Ratio(6, 6), connector.SiteCarrierAsymmetry);
    }
}
