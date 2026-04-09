using System.Linq;
using Core4.Inspection;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class TopologyTrolleyTests
{
    [Fact]
    public void TrolleyPrefersSameCarrierContinuationThroughTJunction()
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

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.Equal(center.Carrier, trolley.Site.Carrier);
        Assert.Equal(center.Location, trolley.Site.Location);

        var observation = trolley.Observe();

        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.Equal(right.Carrier, preferred.To.Carrier);
        Assert.Equal(right.Location, preferred.To.Location);

        var forks = trolley.ForkAll();

        Assert.Equal(2, forks.Length);
        Assert.Contains(forks, fork => SameNode(fork.Site, right));
        Assert.Contains(forks, fork => SameNode(fork.Site, stemEnd));
    }

    [Fact]
    public void TrolleyCanContinueFromOppositePerspectiveWithoutTrailReplay()
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

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));

        Assert.True(SameNode(trolley.Site, top));
        Assert.False(trolley.TryContinuePreferred(out _));
        Assert.True(trolley.TryContinuePreferred(-Measure.One, out var reversed));
        Assert.True(SameNode(reversed.Site, corner));
        Assert.Equal(4, reversed.TrailSites.Length);

        Assert.True(reversed.TryContinuePreferred(-Measure.One, out var returned));
        Assert.True(SameNode(returned.Site, left));
    }

    [Fact]
    public void TrolleyContinuationCarriesBestLocalBasisAgreementOnPassThroughCarrier()
    {
        Carrier x = 1;
        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.FromRelations(
                new Relation(left, middle),
                new Relation(middle, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));

        var continuation = Assert.Single(
            trolley.Observe().ForwardContinuations,
            candidate => SameNode(candidate.To, right));

        Assert.True(continuation.HasBasisAgreement);
        Assert.True(continuation.BestBasisAgreement.Encounter.IsBilateral);
        Assert.True(continuation.BestBasisAgreement.PreservesDirection);
        Assert.True(continuation.BestBasisAgreement.IsBilateral);
    }

    [Fact]
    public void TrolleySurfacesBranchChoicesAtYJunction()
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

        var trolley = TopologyTrolley.Start(topology, stemBase);
        Assert.True(trolley.TryContinuePreferred(out trolley));

        var observation = trolley.Observe();

        Assert.True(observation.IsDecisionPoint);
        Assert.False(observation.TryPreferred(out _));

        var forks = trolley.ForkAll();

        Assert.Equal(2, forks.Length);
        Assert.Contains(forks, fork => SameNode(fork.Site, leftEnd));
        Assert.Contains(forks, fork => SameNode(fork.Site, rightEnd));
    }

    [Fact]
    public void TrolleyContinuationReadsOneSidedOutgoingBasisOnBranchStart()
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

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));

        var branch = Assert.Single(
            trolley.Observe().ForwardContinuations,
            continuation => continuation.To.Carrier == y && continuation.To.Location == new Measure(4));

        Assert.True(branch.HasBasisAgreement);
        Assert.True(branch.BestBasisAgreement.PreservesDirection);
        Assert.True(branch.BestBasisAgreement.IsOneSided);
        Assert.True(branch.BestBasisAgreement.Encounter.HasDominantCalibration);
    }

    [Fact]
    public void TrolleyBlocksAgainstFlowBranchOnForwardPass()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;
        Carrier w = 4;

        var left = Site.Open(x, new Measure(0));
        var entry = Site.Open(x, new Measure(4))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(
                    Site.Open(z, new Measure(0)),
                    Site.Open(z, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var upperApex = Site.Open(y, new Measure(4));
        var upperExit = Site.Open(y, new Measure(8));
        var lowerApex = Site.Open(z, new Measure(4));
        var lowerExit = Site.Open(z, new Measure(8));
        var merge = Site.Open(x, new Measure(12))
            .Request(
                new Relation(y, upperApex, upperExit),
                Ratio.Tension(new Measure(4)))
            .Request(
                new Relation(z, lowerApex, lowerExit),
                Ratio.Tension(new Measure(4)));

        var verticalLow = Site.Open(w, new Measure(-4));
        var verticalMid = Site.Open(w, new Measure(0));
        var verticalHigh = Site.Open(w, new Measure(4));
        var cross = Site.Open(x, new Measure(18))
            .Request(
                new Relation(verticalLow, verticalMid),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(verticalMid, verticalHigh),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(22));

        var topology = ConnectedSiteTopology.Create(
            [entry, merge, cross],
            new Relation(left, entry),
            new Relation(merge, cross),
            new Relation(cross, right));

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));

        var branch = trolley.Observe().ForwardContinuations;
        var upper = Assert.Single(branch.Where(continuation => continuation.To.Carrier == y && continuation.To.Location == new Measure(4)));
        trolley = trolley.Move(upper);

        var apexObservation = trolley.Observe();
        var mergeContinuation = Assert.Single(apexObservation.ForwardContinuations);
        trolley = trolley.Move(mergeContinuation);
        Assert.True(SameNode(trolley.Site, merge));

        var mergeObservation = trolley.Observe();
        var crossContinuation = Assert.Single(mergeObservation.ForwardContinuations);
        trolley = trolley.Move(crossContinuation);
        Assert.True(SameNode(trolley.Site, cross));

        var crossObservation = trolley.Observe();
        Assert.Equal(2, crossObservation.ForwardContinuations.Length);
        Assert.Contains(crossObservation.ForwardContinuations, continuation => SameNode(continuation.To, right));
        Assert.Contains(crossObservation.ForwardContinuations, continuation => continuation.To.Carrier == w && continuation.To.Location == new Measure(4));
        Assert.DoesNotContain(crossObservation.ForwardContinuations, continuation => continuation.To.Carrier == w && continuation.To.Location == new Measure(-4));
    }

    [Fact]
    public void TrolleyCanReturnFromTerminalIngressUnderOppositePerspective()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;
        Carrier w = 4;

        var left = Site.Open(x, new Measure(0));
        var entry = Site.Open(x, new Measure(4))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(
                    Site.Open(z, new Measure(0)),
                    Site.Open(z, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var upperApex = Site.Open(y, new Measure(4));
        var upperExit = Site.Open(y, new Measure(8));
        var lowerApex = Site.Open(z, new Measure(4));
        var lowerExit = Site.Open(z, new Measure(8));
        var merge = Site.Open(x, new Measure(12))
            .Request(
                new Relation(y, upperApex, upperExit),
                Ratio.Tension(new Measure(4)))
            .Request(
                new Relation(z, lowerApex, lowerExit),
                Ratio.Tension(new Measure(4)));

        var verticalLow = Site.Open(w, new Measure(-4));
        var verticalMid = Site.Open(w, new Measure(0));
        var verticalHigh = Site.Open(w, new Measure(4));
        var cross = Site.Open(x, new Measure(18))
            .Request(
                new Relation(verticalLow, verticalMid),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(verticalMid, verticalHigh),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(22));

        var topology = ConnectedSiteTopology.Create(
            [entry, merge, cross],
            new Relation(left, entry),
            new Relation(merge, cross),
            new Relation(cross, right));

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        var upper = Assert.Single(trolley.Observe().ForwardContinuations, continuation => continuation.To.Carrier == y && continuation.To.Location == new Measure(4));
        trolley = trolley.Move(upper);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));
        var upperVertical = Assert.Single(trolley.Observe().ForwardContinuations, continuation => continuation.To.Carrier == w && continuation.To.Location == new Measure(4));
        trolley = trolley.Move(upperVertical);

        Assert.True(trolley.TryContinuePreferred(-Measure.One, out var returned));
        Assert.True(SameNode(returned.Site, cross));
    }

    [Fact]
    public void TrolleyReverseAtDiamondMergeDoesNotReemitTowardCrossWhenBranchesExist()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;
        Carrier w = 4;

        var left = Site.Open(x, new Measure(0));
        var entry = Site.Open(x, new Measure(4))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(
                    Site.Open(z, new Measure(0)),
                    Site.Open(z, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var upperApex = Site.Open(y, new Measure(4));
        var upperExit = Site.Open(y, new Measure(8));
        var lowerApex = Site.Open(z, new Measure(4));
        var lowerExit = Site.Open(z, new Measure(8));
        var merge = Site.Open(x, new Measure(12))
            .Request(
                new Relation(y, upperApex, upperExit),
                Ratio.Tension(new Measure(4)))
            .Request(
                new Relation(z, lowerApex, lowerExit),
                Ratio.Tension(new Measure(4)));

        var verticalLow = Site.Open(w, new Measure(-4));
        var verticalMid = Site.Open(w, new Measure(0));
        var verticalHigh = Site.Open(w, new Measure(4));
        var cross = Site.Open(x, new Measure(18))
            .Request(
                new Relation(verticalLow, verticalMid),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(verticalMid, verticalHigh),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(22));

        var topology = ConnectedSiteTopology.Create(
            [entry, merge, cross],
            new Relation(left, entry),
            new Relation(merge, cross),
            new Relation(cross, right));

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        var upper = Assert.Single(trolley.Observe().ForwardContinuations, continuation => continuation.To.Carrier == y && continuation.To.Location == new Measure(4));
        trolley = trolley.Move(upper);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));

        Assert.True(SameNode(trolley.Site, cross));
        var backToMerge = trolley.Move(Assert.Single(
            trolley.Observe(-Measure.One).ForwardContinuationsIn(-Measure.One),
            continuation => SameNode(continuation.To, merge)));

        var reverseObservation = backToMerge.Observe(-Measure.One);
        Assert.Equal(2, reverseObservation.ForwardContinuationsIn(-Measure.One).Length);
        Assert.Contains(reverseObservation.ForwardContinuationsIn(-Measure.One), continuation => continuation.To.Carrier == y && continuation.To.Location == new Measure(4));
        Assert.Contains(reverseObservation.ForwardContinuationsIn(-Measure.One), continuation => continuation.To.Carrier == z && continuation.To.Location == new Measure(4));
        Assert.DoesNotContain(reverseObservation.ForwardContinuationsIn(-Measure.One), continuation => SameNode(continuation.To, cross));
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}
