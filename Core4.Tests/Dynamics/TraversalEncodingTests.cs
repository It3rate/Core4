using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Tests.Dynamics;

public sealed class TraversalEncodingTests
{
    [Fact]
    public void TraversalSignatureSeparatesProjectionFromEquivalentReentry()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(5));
        var forward = TraversalSignature.FromSite(
            site,
            Measure.One,
            new Ratio(new Measure(4), Measure.One),
            Provenance.SelfDriven(new Measure(4)));
        var reflected = TraversalSignature.FromSite(
            site,
            -Measure.One,
            new Ratio(new Measure(4), Measure.One),
            Provenance.OtherDriven(new Measure(4)));

        Assert.True(forward.SameProjection(reflected));
        Assert.False(forward.Equivalent(reflected));
        Assert.Equal(new Ratio(Measure.One, Measure.One), forward.ProjectionAgreement(reflected));
        Assert.Equal(new Ratio(-Measure.One, Measure.One), forward.PerspectiveAgreement(reflected));
        Assert.Equal(Ratio.Zero(Measure.One), forward.ReentryAgreement(reflected));
    }

    [Fact]
    public void TraversalDispositionEncodesContinuationPerspectiveAndReturnNumerically()
    {
        var disposition = TraversalDisposition.Create(
            continuesCarrier: true,
            direction: -Measure.One,
            returnsAlongIngress: true);

        Assert.True(disposition.ContinuesCarrier);
        Assert.True(disposition.IsIncoming);
        Assert.False(disposition.IsOutgoing);
        Assert.True(disposition.ReturnsAlongIngress);
        Assert.Equal(new Ratio(Measure.One, Measure.One), disposition.Continuity);
        Assert.Equal(new Ratio(-Measure.One, Measure.One), disposition.Perspective);
        Assert.Equal(new Ratio(-Measure.One, Measure.One), disposition.Reentry);
    }

    [Fact]
    public void TraversalObservationPrefersUniqueSameCarrierForwardContinuation()
    {
        Carrier x = 1;
        Carrier y = 2;

        var source = TraversalSignature.FromPlacement(
            Placement.Exact(x, new Measure(5)),
            Measure.One,
            new Ratio(new Measure(5), Measure.One),
            Provenance.SelfDriven(new Measure(5)));

        var sameCarrier = new TraversalContinuation(
            x,
            source,
            TraversalSignature.FromPlacement(
                Placement.Exact(x, new Measure(10)),
                Measure.One,
                new Ratio(Measure.One, Measure.One),
                Provenance.SelfDriven(Measure.One)),
            TraversalDisposition.Create(continuesCarrier: true, direction: Measure.One, returnsAlongIngress: false));

        var branch = new TraversalContinuation(
            y,
            source,
            TraversalSignature.FromPlacement(
                Placement.Exact(y, new Measure(4)),
                Measure.One,
                new Ratio(Measure.One, Measure.One),
                Provenance.SelfDriven(Measure.One)),
            TraversalDisposition.Create(continuesCarrier: false, direction: Measure.One, returnsAlongIngress: false));

        var observation = new TraversalObservation(source, [sameCarrier, branch]);

        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.Equal(x, preferred.Carrier);
        Assert.True(preferred.ContinuesCarrier);
    }
}
