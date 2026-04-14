using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;
using Core4.Types;

namespace Core4.Tests.Types;

public sealed class CarrierCommunicationTests
{
    [Fact]
    public void CarrierPortsMoveResidualToListeningSiteOnNextRound()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Harden(new Ratio(1, 2))
            .WithResidual(Ratio.Tension(new Measure(8)))
            .Publish(light, Ratio.Tension(new Measure(4)));
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(4)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();

        Assert.Single(afterOne.Transfers);
        Assert.Equal(light, afterOne.Transfers[0].Carrier);
        Assert.Equal(source.Placement, afterOne.Transfers[0].Source);
        Assert.Equal(receiver.Placement, afterOne.Transfers[0].Target);
        Assert.Equal(new Ratio(4, 0), afterOne.Transfers[0].Value);
        Assert.Equal(new Ratio(4, 0), afterOne.At(receiver).Orthogonal.Residual);
        Assert.Equal(new Ratio(4, 0), afterOne.At(source).State);
        Assert.Equal(new Ratio(0, 0), afterOne.At(source).Orthogonal.Residual);

        var afterTwo = afterOne.Relax();

        Assert.Equal(new Ratio(4, 0), afterTwo.At(receiver).State);
        Assert.Equal(new Ratio(0, 0), afterTwo.At(receiver).Orthogonal.Residual);
    }

    [Fact]
    public void CarrierPortsDoNotFeedSelfOutputBackAsExternalInput()
    {
        Carrier x = 1;
        Carrier light = 2;

        var site = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Harden(new Ratio(1, 2))
            .WithResidual(Ratio.Tension(new Measure(8)))
            .Publish(light, Ratio.Tension(new Measure(4)))
            .Request(light, Ratio.Tension(new Measure(4)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([site]));
        var next = communication.Seed().Relax();

        Assert.Empty(next.Transfers);
        Assert.Equal(new Ratio(4, 0), next.At(site).State);
        Assert.Equal(new Ratio(4, 0), next.At(site).Orthogonal.Residual);
    }

    [Fact]
    public void CarrierPortsAreNotCountedAsSamePassLocalSettlementFlows()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Publish(light, Ratio.Tension(new Measure(7)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source]));
        var next = communication.Seed().Relax();

        Assert.Equal(new Ratio(0, 0), next.At(source).State);
        Assert.Equal(new Ratio(0, 0), next.At(source).Orthogonal.Residual);
        Assert.Empty(next.Transfers);
    }
}
