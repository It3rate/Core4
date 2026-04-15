using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;
using Core4.Dynamics;

namespace Core4.Tests.Dynamics;

public sealed class CarrierCommunicationTests
{
    [Fact]
    public void CarrierPortsMoveResolvedPublicationToListeningSiteOnNextRound()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Publish(light, new Ratio(4, 4));
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(4)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();

        Assert.Single(afterOne.Transfers);
        Assert.Single(afterOne.Fields);
        Assert.Single(afterOne.PublicationTransfers);
        Assert.Single(afterOne.PublicationFields);
        Assert.Equal(light, afterOne.Transfers[0].Carrier);
        Assert.Equal(source.Placement, afterOne.Transfers[0].Source);
        Assert.Equal(receiver.Placement, afterOne.Transfers[0].Target);
        Assert.Equal(new Ratio(4, 4), afterOne.Transfers[0].Value);
        Assert.Equal(light, afterOne.Fields[0].Carrier);
        Assert.Equal(new Ratio(4, 4), afterOne.Fields[0].Published);
        Assert.Equal(new Ratio(4, 4), afterOne.Fields[0].Delivered);
        Assert.Equal(new Ratio(0, 0), afterOne.Fields[0].Retained);
        Assert.Equal(new Measure(4), afterOne.Fields[0].EmitterWeight);
        Assert.Equal(new Measure(4), afterOne.Fields[0].ListenerWeight);
        Assert.Equal(Measure.One, afterOne.Fields[0].TransferCount);
        Assert.Equal(new Ratio(0, 0), afterOne.At(receiver).Orthogonal.Residual);
        Assert.Equal(new Ratio(0, 0), afterOne.At(source).Orthogonal.Residual);
        Assert.Single(afterOne.At(receiver).Orthogonal.IncomingPublications());
        Assert.Equal(new Ratio(4, 4), afterOne.At(receiver).Orthogonal.IncomingPublications()[0].Value);

        var afterTwo = afterOne.Relax();

        Assert.Equal(new Ratio(0, 0), afterTwo.At(receiver).Orthogonal.Residual);
        Assert.Single(afterTwo.At(receiver).Orthogonal.IncomingPublications());
    }

    [Fact]
    public void CarrierPortsDoNotFeedSelfOutputBackAsExternalInput()
    {
        Carrier x = 1;
        Carrier light = 2;

        var site = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Publish(light, new Ratio(4, 4))
            .Request(light, Ratio.Tension(new Measure(4)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([site]));
        var next = communication.Seed().Relax();

        Assert.Empty(next.Transfers);
        Assert.Empty(next.PublicationTransfers);
        Assert.Single(next.Fields);
        Assert.Single(next.PublicationFields);
        Assert.Equal(new Ratio(4, 4), next.Fields[0].Published);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Delivered);
        Assert.Equal(new Ratio(4, 4), next.Fields[0].Retained);
        Assert.Equal(Measure.One, next.PublicationFields[0].PublishedCount);
        Assert.Equal(Measure.Zero, next.PublicationFields[0].DeliveredCount);
        Assert.Empty(next.At(site).Orthogonal.IncomingPublications());
        Assert.Equal(new Ratio(0, 0), next.At(site).Orthogonal.Residual);
    }

    [Fact]
    public void UnresolvedCarrierTensionDoesNotPublishAcrossSites()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Publish(light, Ratio.Tension(new Measure(7)));
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(7)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var next = communication.Seed().Relax();

        Assert.Single(next.Fields);
        Assert.Single(next.PublicationFields);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Published);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Delivered);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Retained);
        Assert.Equal(Measure.Zero, next.Fields[0].EmitterWeight);
        Assert.Equal(new Measure(7), next.Fields[0].ListenerWeight);
        Assert.Equal(Measure.Zero, next.PublicationFields[0].PublishedCount);
        Assert.Equal(Measure.Zero, next.PublicationFields[0].DeliveredCount);
        Assert.Equal(new Ratio(0, 0), next.At(source).State);
        Assert.Equal(new Ratio(0, 0), next.At(source).Orthogonal.Residual);
        Assert.Empty(next.Transfers);
        Assert.Empty(next.PublicationTransfers);
        Assert.Empty(next.At(receiver).Orthogonal.IncomingPublications());
    }

    [Fact]
    public void CarrierPortsCanDeliverReadOnlySitePublications()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Resolve(new Ratio(3, 1))
            .PublishReference(light);
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(4)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();

        Assert.Single(afterOne.PublicationTransfers);
        Assert.Single(afterOne.PublicationFields);
        Assert.Equal(light, afterOne.PublicationTransfers[0].Carrier);
        Assert.Equal(source.Placement, afterOne.PublicationTransfers[0].Source);
        Assert.Equal(receiver.Placement, afterOne.PublicationTransfers[0].Target);
        Assert.Equal(source.Placement, afterOne.PublicationTransfers[0].Publication.Site.Placement);
        Assert.Equal(source.State, afterOne.PublicationTransfers[0].Publication.Site.State);
        Assert.Equal(light, afterOne.PublicationFields[0].Carrier);
        Assert.Equal(Measure.One, afterOne.PublicationFields[0].PublishedCount);
        Assert.Equal(Measure.One, afterOne.PublicationFields[0].DeliveredCount);
        Assert.Equal(Measure.One, afterOne.PublicationFields[0].ListenerCount);
        Assert.Equal(Measure.One, afterOne.PublicationFields[0].TransferCount);
        Assert.Single(afterOne.At(receiver).Orthogonal.IncomingPublications());
        Assert.Equal(source.Placement, afterOne.At(receiver).Orthogonal.IncomingPublications()[0].Site.Placement);
        Assert.Equal(source.State, afterOne.At(receiver).Orthogonal.IncomingPublications()[0].Site.State);
    }

    [Fact]
    public void IncomingSitePublicationsDoNotSettleAsLocalTension()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Resolve(new Ratio(2, 1))
            .PublishReference(light);
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(4)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();
        var afterTwo = afterOne.Relax();

        Assert.Equal(new Ratio(0, 0), afterOne.At(receiver).Orthogonal.Residual);
        Assert.Equal(new Ratio(0, 0), afterTwo.At(receiver).Orthogonal.Residual);
        Assert.Equal(new Ratio(0, 0), afterOne.At(receiver).State);
        Assert.Equal(new Ratio(0, 0), afterTwo.At(receiver).State);
        Assert.Single(afterOne.At(receiver).Orthogonal.IncomingPublications());
        Assert.Single(afterTwo.At(receiver).Orthogonal.IncomingPublications());
    }
}
