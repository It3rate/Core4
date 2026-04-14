using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;
using SettlementPhase = Core4.Dynamics.SiteSettlement.SettlementPhase;

namespace Core4.Tests.Dynamics;

public sealed class SiteAndSettlementTests
{
    [Fact]
    public void OpenSiteStartsOpenAndUnresolved()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(0));

        Assert.True(site.IsOpen);
        Assert.False(site.IsTension);
        Assert.Equal(new Ratio(0, 0), site.State);
        Assert.Equal(new Ratio(0, 1), site.Stiffness);
        Assert.Equal(new Ratio(1, 1), site.RoutingGain);
        Assert.Equal(new Ratio(1, 1), site.DominantBias);
        Assert.Equal(Measure.Zero, site.Potential);
    }

    [Fact]
    public void FocusedSiteUsesZeroDominantTensionState()
    {
        Carrier x = 1;
        var site = Site.Tension(x, new Measure(0), new Measure(5));

        Assert.False(site.IsOpen);
        Assert.True(site.IsTension);
        Assert.Equal(new Ratio(5, 0), site.State);
        Assert.Equal(new Measure(5), site.Potential);
    }

    [Fact]
    public void OrthogonalReservoirGroupsFlowsByProvenanceDirection()
    {
        Carrier x = 1;
        Carrier incomingCarrier = 2;
        Carrier outgoingCarrier = 3;
        Carrier sharedCarrier = 4;

        var site = Site.Open(x, new Measure(0))
            .WithResidual(Ratio.Tension(new Measure(3)))
            .Request(incomingCarrier, Ratio.Tension(new Measure(4)))
            .Publish(outgoingCarrier, Ratio.Tension(new Measure(6)))
            .Exchange(sharedCarrier, Ratio.Tension(new Measure(2)), Provenance.Shared(new Measure(2)));

        Assert.Equal(new Ratio(4, 0), site.Orthogonal.Incoming());
        Assert.Equal(new Ratio(6, 0), site.Orthogonal.Outgoing());
        Assert.Equal(new Ratio(2, 0), site.Orthogonal.Oscillating());
        Assert.Equal(new Measure(3), site.Orthogonal.ConnectionCount);
        Assert.Equal(new Measure(15), site.Potential);
    }

    [Fact]
    public void SettleMatchesCurrentSingleSiteNegotiationPass()
    {
        Carrier x = 1;
        Carrier lightSensor = 2;
        Carrier switchState = 3;
        Carrier navigation = 4;

        var site = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Harden(new Ratio(3, 4))
            .WithResidual(Ratio.Tension(new Measure(3)))
            .Request(lightSensor, Ratio.Tension(new Measure(4)))
            .Request(switchState, Ratio.Tension(new Measure(6)))
            .Exchange(navigation, Ratio.Tension(new Measure(2)), Provenance.Shared(new Measure(2)));

        var settlement = site.Settle();

        Assert.Equal(new Ratio(0, 1), settlement.Anchor);
        Assert.Equal(new Ratio(13, 0), settlement.Incoming);
        Assert.Equal(new Ratio(11, 0), settlement.Provisional);
        Assert.Equal(new Ratio(2, 0), settlement.Published);
        Assert.Equal(new Ratio(2, 0), settlement.Feedback);
        Assert.Equal(new Ratio(2, 0), settlement.Settled);
        Assert.Equal(new Ratio(9, 0), settlement.Remainder);
        Assert.Equal(new Ratio(11, 0), settlement.At(SettlementPhase.Provisional));
        Assert.Equal(new Ratio(2, 0), settlement.At(SettlementPhase.Settled));
        Assert.Equal(new Ratio(0, 0), settlement.At(SettlementPhase.None));
        Assert.True(settlement.Phases.MoveTo(SettlementPhase.Settled));
        Assert.Equal(new Measure(6), settlement.Phases.CurrentIndex);
        Assert.Equal(new Ratio(2, 0), settlement.Current);
        Assert.Equal(new Measure(3), settlement.Phases.Relate(SettlementPhase.Provisional, SettlementPhase.Settled).Potential);
        Assert.Equal(new Ratio(2, 0), settlement.Next.State);
        Assert.Equal(new Ratio(3, 4), settlement.Next.Stiffness);
        Assert.Equal(new Ratio(9, 0), settlement.Next.Orthogonal.Residual);
    }

    [Fact]
    public void HardeningSitePinsAlignedClaimWithoutChangingState()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Resolve(new Ratio(5, 10))
            .Harden(new Ratio(3, 4));

        Assert.Equal(new Ratio(0, 1), site.Kernel.Recessive);
        Assert.Equal(new Ratio(5, 10), site.State);
        Assert.Equal(new Ratio(3, 4), site.Stiffness);
    }

    [Fact]
    public void HardeningPreservesNonReducedConfidenceScale()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(0)).Harden(new Ratio(900, 1000));

        Assert.Equal(new Ratio(900, 1000), site.Stiffness);
    }

    [Fact]
    public void RoutingGainPartitionsSoftenedShareBeforeAlignedAbsorption()
    {
        Carrier x = 1;
        Carrier input = 2;

        var site = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Harden(new Ratio(1, 2))
            .Route(new Ratio(1, 2))
            .Request(input, Ratio.Tension(new Measure(8)));

        var settlement = site.Settle();

        Assert.Equal(new Ratio(8, 0), settlement.Incoming);
        Assert.Equal(new Ratio(8, 0), settlement.Provisional);
        Assert.Equal(new Ratio(2, 0), settlement.Published);
        Assert.Equal(new Ratio(2, 0), settlement.Settled);
        Assert.Equal(new Ratio(6, 0), settlement.Remainder);
        Assert.Equal(new Ratio(1, 2), settlement.Next.Stiffness);
        Assert.Equal(new Ratio(1, 2), settlement.Next.RoutingGain);
        Assert.Equal(new Ratio(1, 1), settlement.Next.DominantBias);
    }

    [Fact]
    public void RoutingPreservesNonReducedConfidenceScale()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(0)).Route(new Ratio(900, 1000));

        Assert.Equal(new Ratio(900, 1000), site.RoutingGain);
    }

    [Fact]
    public void DominantBiasPartitionsRoutedShareBetweenSupportAndClaim()
    {
        Carrier x = 1;
        Carrier input = 2;

        var site = Site.Open(x, new Measure(0))
            .Anchor(new Ratio(0, 1))
            .Harden(new Ratio(1, 2))
            .Route(new Ratio(1, 1))
            .BiasToDominant(new Ratio(1, 4))
            .Request(input, Ratio.Tension(new Measure(8)));

        var settlement = site.Settle();

        Assert.Equal(new Ratio(8, 0), settlement.Incoming);
        Assert.Equal(new Ratio(8, 0), settlement.Provisional);
        Assert.Equal(new Ratio(4, 0), settlement.Published);
        Assert.Equal(new Ratio(4, 0), settlement.Settled);
        Assert.Equal(new Ratio(4, 0), settlement.Remainder);
        Assert.Equal(new Ratio(3, 0), settlement.Next.Kernel.Recessive);
        Assert.Equal(new Ratio(1, 0), settlement.Next.State);
        Assert.Equal(new Ratio(1, 4), settlement.Next.DominantBias);
    }

    [Fact]
    public void DominantBiasPreservesNonReducedConfidenceScale()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(0)).BiasToDominant(new Ratio(900, 1000));

        Assert.Equal(new Ratio(900, 1000), site.DominantBias);
    }
}
