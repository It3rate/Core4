using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class BasisEncounterAgreementTests
{
    [Fact]
    public void ComparePreferenceUsesResolutionAsFinalTieBreak()
    {
        var stronger = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, Measure.One),
            commitment: new Ratio(Measure.One, Measure.One),
            resolution: new Ratio(new Measure(4), new Measure(5)));
        var weaker = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, Measure.One),
            commitment: new Ratio(Measure.One, Measure.One),
            resolution: new Ratio(new Measure(2), new Measure(5)));

        Assert.True(stronger.ComparePreference(weaker) > 0);
        Assert.Equal(0, stronger.CompareTraversalPreference(weaker));
    }

    [Fact]
    public void CompareTraversalPreferencePrioritizesContinuationBeforeCapabilityOrCommitment()
    {
        var preserving = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: Ratio.Zero(Measure.One),
            commitment: Ratio.Zero(Measure.One),
            resolution: Ratio.Zero(Measure.One));
        var inverting = Agreement(
            continuation: new Ratio(-Measure.One, Measure.One),
            capability: new Ratio(Measure.One, Measure.One),
            commitment: new Ratio(Measure.One, Measure.One),
            resolution: new Ratio(Measure.One, Measure.One));

        Assert.True(preserving.CompareTraversalPreference(inverting) > 0);
    }

    [Fact]
    public void CompareTraversalPreferencePrioritizesCapabilityBeforeCommitment()
    {
        var bilateral = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, Measure.One),
            commitment: new Ratio(new Measure(1), new Measure(4)),
            resolution: Ratio.Zero(Measure.One));
        var oneSided = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, new Measure(2)),
            commitment: new Ratio(Measure.One, Measure.One),
            resolution: new Ratio(Measure.One, Measure.One));

        Assert.True(bilateral.CompareTraversalPreference(oneSided) > 0);
    }

    [Fact]
    public void CompareTraversalPreferenceUsesCommitmentAfterCapabilityTie()
    {
        var stronger = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, new Measure(2)),
            commitment: new Ratio(new Measure(3), new Measure(4)),
            resolution: Ratio.Zero(Measure.One));
        var weaker = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, new Measure(2)),
            commitment: new Ratio(new Measure(1), new Measure(4)),
            resolution: new Ratio(Measure.One, Measure.One));

        Assert.True(stronger.CompareTraversalPreference(weaker) > 0);
    }

    [Fact]
    public void AgreementExposesTransmissionAndRetentionFromCommitment()
    {
        var agreement = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, Measure.One),
            commitment: new Ratio(new Measure(3), new Measure(4)),
            resolution: Ratio.Zero(Measure.One));

        Assert.True(agreement.HasCommittedTransfer);
        Assert.Equal(new Ratio(new Measure(3), new Measure(4)), agreement.Transmission);
        Assert.Equal(new Ratio(new Measure(1), new Measure(4)), agreement.Retention);
    }

    [Fact]
    public void CompareTransferPreferenceUsesCommitmentMagnitude()
    {
        var stronger = Agreement(
            continuation: Ratio.Zero(Measure.One),
            capability: Ratio.Zero(Measure.One),
            commitment: new Ratio(new Measure(3), new Measure(4)),
            resolution: Ratio.Zero(Measure.One));
        var weaker = Agreement(
            continuation: new Ratio(Measure.One, Measure.One),
            capability: new Ratio(Measure.One, Measure.One),
            commitment: new Ratio(new Measure(1), new Measure(4)),
            resolution: new Ratio(Measure.One, Measure.One));

        Assert.True(stronger.CompareTransferPreference(weaker) > 0);
    }

    [Fact]
    public void AgreementCanIdentifyReflectivePinnedBoundaryCase()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(10));
        var encounter = new SiteBasisEncounter(
            site,
            x,
            Basis.Freeze(x, new Measure(10), new Measure(6)),
            Basis.Freeze(x, new Measure(10), new Measure(6)),
            new Measure(4));
        var signature = TraversalSignature.FromSite(
            site,
            -Measure.One,
            new Ratio(new Measure(10), Measure.One),
            Provenance.SelfDriven(new Measure(10)));
        var agreement = encounter.Agree(signature, -Measure.One);

        Assert.True(agreement.CanReflectAtLocalPinnedBoundary(hasClosureTarget: false));
        Assert.False(agreement.CanReflectAtLocalPinnedBoundary(hasClosureTarget: true));
    }

    private static BasisEncounterAgreement Agreement(
        Ratio continuation,
        Ratio capability,
        Ratio commitment,
        Ratio resolution) =>
        new(default, resolution, continuation, capability, commitment);
}
