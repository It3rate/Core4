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
        Assert.Single(afterOne.FeedbackFields);
        Assert.Empty(afterOne.FeedbackTransfers);
        Assert.Empty(afterOne.FeedbackSummaries);
        Assert.Equal(light, afterOne.Transfers[0].Carrier);
        Assert.Equal(source.Placement, afterOne.Transfers[0].Source);
        Assert.Equal(receiver.Placement, afterOne.Transfers[0].Target);
        Assert.Equal(new Ratio(4, 4), afterOne.Transfers[0].Value);
        Assert.Equal(new Ratio(4, 4), afterOne.Transfers[0].ValueRead.AsRatio());
        Assert.Equal(light, afterOne.Fields[0].Carrier);
        Assert.Equal(new Ratio(4, 4), afterOne.Fields[0].Published);
        Assert.Equal(new Ratio(4, 4), afterOne.Fields[0].PublishedRead.AsRatio());
        Assert.Equal(new Ratio(4, 4), afterOne.Fields[0].Delivered);
        Assert.Equal(new Ratio(0, 0), afterOne.Fields[0].Retained);
        Assert.Equal(new Measure(4), afterOne.Fields[0].EmitterWeight);
        Assert.Equal(new Measure(4), afterOne.Fields[0].ListenerWeight);
        Assert.Equal(Measure.One, afterOne.Fields[0].TransferCount);
        Assert.Equal(new Ratio(0, 0), afterOne.FeedbackFields[0].Requested);
        Assert.Equal(new Ratio(0, 0), afterOne.FeedbackFields[0].Delivered);
        Assert.Equal(new Ratio(0, 0), afterOne.FeedbackFields[0].Retained);
        Assert.Equal(new Ratio(0, 0), afterOne.At(receiver).Orthogonal.Residual);
        Assert.Equal(new Ratio(0, 0), afterOne.At(source).Orthogonal.Residual);
        Assert.Single(afterOne.At(receiver).Orthogonal.IncomingPublications());
        Assert.Equal(new Ratio(4, 4), afterOne.At(receiver).Orthogonal.IncomingPublications()[0].Value);
        Assert.Empty(afterOne.At(source).Orthogonal.IncomingFeedback());

        var afterTwo = afterOne.Relax();

        Assert.Equal(new Ratio(0, 0), afterTwo.At(receiver).Orthogonal.Residual);
        Assert.Single(afterTwo.At(receiver).Orthogonal.IncomingPublications());
        Assert.Empty(afterTwo.At(source).Orthogonal.IncomingFeedback());
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
        Assert.Empty(next.FeedbackTransfers);
        Assert.Single(next.Fields);
        Assert.Single(next.PublicationFields);
        Assert.Single(next.FeedbackFields);
        Assert.Equal(new Ratio(4, 4), next.Fields[0].Published);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Delivered);
        Assert.Equal(new Ratio(4, 4), next.Fields[0].Retained);
        Assert.Equal(Measure.One, next.PublicationFields[0].PublishedCount);
        Assert.Equal(Measure.Zero, next.PublicationFields[0].DeliveredCount);
        Assert.Equal(new Ratio(0, 0), next.FeedbackFields[0].Requested);
        Assert.Empty(next.At(site).Orthogonal.IncomingPublications());
        Assert.Empty(next.At(site).Orthogonal.IncomingFeedback());
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
        Assert.Single(next.FeedbackFields);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Published);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Delivered);
        Assert.Equal(new Ratio(0, 0), next.Fields[0].Retained);
        Assert.Equal(Measure.Zero, next.Fields[0].EmitterWeight);
        Assert.Equal(new Measure(7), next.Fields[0].ListenerWeight);
        Assert.Equal(Measure.Zero, next.PublicationFields[0].PublishedCount);
        Assert.Equal(Measure.Zero, next.PublicationFields[0].DeliveredCount);
        Assert.Equal(new Ratio(0, 0), next.FeedbackFields[0].Requested);
        Assert.Equal(new Ratio(0, 0), next.At(source).State);
        Assert.Equal(new Ratio(0, 0), next.At(source).Orthogonal.Residual);
        Assert.Empty(next.Transfers);
        Assert.Empty(next.PublicationTransfers);
        Assert.Empty(next.FeedbackTransfers);
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
        Assert.Empty(afterOne.At(source).Orthogonal.IncomingFeedback());
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

    [Fact]
    public void ResolvedCarrierRequestsReturnReadOnlyFeedbackToPublishingSite()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Resolve(new Ratio(4, 1))
            .Publish(light, new Ratio(4, 1));
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, new Ratio(51, 10));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();

        Assert.Single(afterOne.FeedbackTransfers);
        Assert.Single(afterOne.FeedbackFields);
        Assert.Single(afterOne.FeedbackSummaries);
        Assert.Equal(light, afterOne.FeedbackTransfers[0].Carrier);
        Assert.Equal(receiver.Placement, afterOne.FeedbackTransfers[0].Source);
        Assert.Equal(source.Placement, afterOne.FeedbackTransfers[0].Target);
        Assert.Equal(new Ratio(51, 10), afterOne.FeedbackTransfers[0].Feedback.Desired);
        Assert.Equal(new Ratio(4, 1), afterOne.FeedbackTransfers[0].Feedback.Published);
        Assert.Single(afterOne.At(source).Orthogonal.IncomingFeedback());
        Assert.Equal(new Ratio(51, 10), afterOne.At(source).Orthogonal.IncomingFeedback()[0].Desired);
        Assert.Equal(new Ratio(4, 1), afterOne.At(source).Orthogonal.IncomingFeedback()[0].Published);
        Assert.Equal(new Ratio(51, 10), afterOne.FeedbackFields[0].Requested);
        Assert.Equal(new Ratio(51, 10), afterOne.FeedbackFields[0].Delivered);
        Assert.Equal(new Ratio(0, 0), afterOne.FeedbackFields[0].Retained);
        Assert.Equal(new Ratio(51, 10), afterOne.FeedbackSummaries[0].Preferred);
        Assert.Equal(new Ratio(51, 10), afterOne.FeedbackSummaries[0].PreferredRead.AsRatio());
        Assert.Equal(new Ratio(10, 1), afterOne.FeedbackSummaries[0].DesiredResolution);
        Assert.Equal(new Ratio(11, 10), afterOne.FeedbackSummaries[0].ValueDelta);
        Assert.Equal(new Ratio(9, 1), afterOne.FeedbackSummaries[0].ResolutionDelta);
    }

    [Fact]
    public void FeedbackSummariesRetuneNextRoundCarrierPublication()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Resolve(new Ratio(4, 1))
            .Publish(light, new Ratio(4, 1));
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, new Ratio(6, 1));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();
        var afterTwo = afterOne.Relax();

        Assert.Single(afterOne.FeedbackSummaries);
        Assert.Equal(new Ratio(40, 8), afterOne.At(source).Orthogonal.Flows[0].Value);
        Assert.Single(afterTwo.Transfers);
        Assert.Equal(new Ratio(40, 8), afterTwo.Transfers[0].Value);
        Assert.Single(afterTwo.PublicationTransfers);
        Assert.Equal(new Ratio(40, 8), afterTwo.PublicationTransfers[0].Publication.Value);
    }

    [Fact]
    public void FeedbackSpreadDampensAdaptivePublicationUpdate()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Resolve(new Ratio(4, 1))
            .Publish(light, new Ratio(4, 1));
        var left = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, new Ratio(6, 1));
        var right = Site.Open(x, new Measure(20))
            .Anchor(new Ratio(0, 1))
            .Request(light, new Ratio(10, 1));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, left, right]));
        var afterOne = communication.Seed().Relax();

        Assert.Single(afterOne.FeedbackSummaries);
        Assert.Equal(new Ratio(16, 2), afterOne.FeedbackSummaries[0].Preferred);
        Assert.Equal(new Ratio(4, 1), afterOne.FeedbackSummaries[0].ValueSpread);
        Assert.Equal(new Ratio(288, 56), afterOne.At(source).Orthogonal.Flows[0].Value);
    }

    [Fact]
    public void TensionOnlyCarrierRequestsRegisterListeningWithoutSendingFeedback()
    {
        Carrier x = 1;
        Carrier light = 2;

        var source = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Publish(light, new Ratio(4, 1));
        var receiver = Site.Open(x, new Measure(10))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(8)));

        var communication = CarrierCommunication.Create(ConnectedSiteTopology.Create([source, receiver]));
        var afterOne = communication.Seed().Relax();

        Assert.Single(afterOne.Transfers);
        Assert.Single(afterOne.PublicationTransfers);
        Assert.Single(afterOne.FeedbackFields);
        Assert.Empty(afterOne.FeedbackTransfers);
        Assert.Empty(afterOne.FeedbackSummaries);
        Assert.Empty(afterOne.At(source).Orthogonal.IncomingFeedback());
        Assert.Equal(new Ratio(0, 0), afterOne.FeedbackFields[0].Requested);
    }

    [Fact]
    public void CarrierCommunicationStateDistinguishesExactSitesThatShareWholeLocation()
    {
        Carrier x = 1;
        Carrier light = 2;

        var left = Site.Open(x.At(new Ratio(new Measure(1), new Measure(4))))
            .Anchor(new Ratio(0, 1))
            .Publish(light, new Ratio(4, 4));
        var right = Site.Open(x.At(new Ratio(new Measure(3), new Measure(4))))
            .Anchor(new Ratio(0, 1))
            .Request(light, Ratio.Tension(new Measure(4)));

        var state = CarrierCommunication.Create(ConnectedSiteTopology.Create([left, right]))
            .Seed()
            .Relax();

        Assert.Equal(left.Placement, state.At(left).Placement);
        Assert.Equal(right.Placement, state.At(right).Placement);
        Assert.NotEqual(state.At(left).Placement, state.At(right).Placement);
        Assert.Single(state.Transfers);
        Assert.Equal(left.Placement, state.Transfers[0].Source);
        Assert.Equal(right.Placement, state.Transfers[0].Target);
    }
}
