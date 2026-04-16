using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Dynamics;

public sealed class TraversalEncodingTests
{
    [Fact]
    public void TraversalSignatureExposesPlacementProjectionAndPositionAlias()
    {
        Carrier x = 1;
        var placement = new Placement(x, new Ratio(3, 10));
        var signature = TraversalSignature.FromPlacement(
            placement,
            Measure.One,
            new Ratio(Measure.One, Measure.One),
            Provenance.SelfDriven(Measure.One));

        Assert.Equal(placement.ProjectionRead.AsRatio(), signature.ProjectionRead.AsRatio());
        Assert.Equal(new Ratio(Measure.One, Measure.One), signature.CalibrationRead.AsRatio());
        Assert.Equal(placement.Projection, signature.Projection);
        Assert.Equal(placement.Projection, signature.Position);
        Assert.Equal(new Ratio(Measure.One, Measure.One), signature.Calibration);
        Assert.Equal(new Ratio(Measure.One, Measure.One), signature.Basis);
    }

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
    public void TraversalSignatureKeepsCalibrationDistinctFromTraversalModifier()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(5));

        var adopted = TraversalSignature.FromSite(
            site,
            new Ratio(Measure.One, Measure.One),
            TraversalModifier.Identity,
            Provenance.SelfDriven(Measure.One));
        var invertedState = TraversalSignature.FromSite(
            site,
            new Ratio(-Measure.One, Measure.One),
            TraversalModifier.FromDirection(-Measure.One),
            Provenance.SelfDriven(Measure.One));

        Assert.True(adopted.SameProjection(invertedState));
        Assert.False(adopted.Equivalent(invertedState));
        Assert.Equal(new Ratio(-Measure.One, Measure.One), adopted.CalibrationAgreement(invertedState));
        Assert.Equal(new Ratio(-Measure.One, Measure.One), adopted.ModifierAgreement(invertedState));
        Assert.Equal(new Ratio(-Measure.One, Measure.One), adopted.PerspectiveAgreement(invertedState));
        Assert.Equal(new Ratio(Measure.One, Measure.One), adopted.EffectiveAgreement(invertedState));
    }

    [Fact]
    public void TraversalSignatureCanCarryCustomCalibrationRead()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(5));
        ISupportProjection calibration = new ProxyProjection(new Ratio(new Measure(2), new Measure(5)));
        var signature = TraversalSignature.FromSite(
            site,
            calibration,
            TraversalModifier.Identity,
            Provenance.SelfDriven(Measure.One));

        Assert.Equal(new Ratio(new Measure(2), new Measure(5)), signature.CalibrationRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(2), new Measure(5)), signature.Calibration);
        Assert.Equal(new Measure(8), signature.Potential);
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

        var sameCarrierTarget = TraversalSignature.FromPlacement(
            Placement.Exact(x, new Measure(10)),
            Measure.One,
            new Ratio(Measure.One, Measure.One),
            Provenance.SelfDriven(Measure.One));
        var sameCarrier = new TraversalContinuation(
            x,
            source,
            sameCarrierTarget,
            TraversalDisposition.Create(continuesCarrier: true, direction: Measure.One, returnsAlongIngress: false));

        var branchTarget = TraversalSignature.FromPlacement(
            Placement.Exact(y, new Measure(4)),
            Measure.One,
            new Ratio(Measure.One, Measure.One),
            Provenance.SelfDriven(Measure.One));
        var branch = new TraversalContinuation(
            y,
            source,
            branchTarget,
            TraversalDisposition.Create(continuesCarrier: false, direction: Measure.One, returnsAlongIngress: false));

        var observation = new TraversalObservation(source, [sameCarrier, branch]);

        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.Equal(x, preferred.Carrier);
        Assert.True(preferred.ContinuesCarrier);
    }

    [Fact]
    public void TraversalObservationFlipsForwardReadingUnderOppositePerspective()
    {
        Carrier x = 1;
        var source = TraversalSignature.FromPlacement(
            Placement.Exact(x, new Measure(5)),
            Measure.One,
            new Ratio(new Measure(4), Measure.One),
            Provenance.SelfDriven(new Measure(4)));
        var ingress = new TraversalContinuation(
            x,
            source,
            TraversalSignature.FromPlacement(
                Placement.Exact(x, new Measure(0)),
                -Measure.One,
                new Ratio(Measure.One, Measure.One),
                Provenance.OtherDriven(Measure.One)),
            TraversalDisposition.Create(continuesCarrier: true, direction: -Measure.One, returnsAlongIngress: true));
        var onward = new TraversalContinuation(
            x,
            source,
            TraversalSignature.FromPlacement(
                Placement.Exact(x, new Measure(10)),
                Measure.One,
                new Ratio(Measure.One, Measure.One),
                Provenance.SelfDriven(Measure.One)),
            TraversalDisposition.Create(continuesCarrier: true, direction: Measure.One, returnsAlongIngress: false));
        var observation = new TraversalObservation(source, [ingress, onward]);

        Assert.Single(observation.ForwardContinuationsIn(Measure.One));
        Assert.Equal(new Measure(10), observation.ForwardContinuationsIn(Measure.One)[0].Target.Position.Whole);
        Assert.Single(observation.ForwardContinuationsIn(-Measure.One));
        Assert.True(observation.TryPreferred(-Measure.One, out var preferred));
        Assert.Equal(new Measure(0), preferred.Target.Position.Whole);
    }

    private readonly record struct ProxyProjection(Ratio Value) : ISupportProjection
    {
        public Measure Potential => Value.Potential;

        public Ratio AsRatio() => Value;
    }
}
