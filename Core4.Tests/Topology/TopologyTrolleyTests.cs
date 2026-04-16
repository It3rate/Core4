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
        Assert.True(SameNode(trolley.Site, center));

        var observation = trolley.Observe();

        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.True(SameNode(preferred.To, right));

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
    public void TrolleyReflectsAtTerminalWhenExplicitPinnedBasisPreservesIngressDirection()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(6)),
                Ratio.Tension(new Measure(4)),
                Provenance.Shared(new Measure(4)));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.FromRelations(new Relation(left, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, right));

        var reflection = Assert.Single(trolley.Observe().ForwardContinuations);
        Assert.True(reflection.ReturnsAlongIngress);
        Assert.Contains(
            reflection.BasisAgreements,
            agreement => agreement.Encounter.IsExplicitPinnedBasis && agreement.PreservesDirection);
        Assert.True(trolley.TryContinuePreferred(out var returned));
        Assert.True(SameNode(returned.Site, left));
    }

    [Fact]
    public void TrolleyDoesNotReflectWhenExplicitPinnedBasisOpposesIngressDirection()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(14)),
                Ratio.Tension(new Measure(4)),
                Provenance.Shared(new Measure(4)));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.FromRelations(new Relation(left, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, right));

        Assert.Empty(trolley.Observe().ForwardContinuations);
        Assert.False(trolley.TryContinuePreferred(out _));
    }

    [Fact]
    public void TrolleyCarriesAttenuatedCalibrationAfterPartialReflection()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(6)),
                new Ratio(new Measure(2), Measure.One),
                Provenance.Shared(new Measure(2)));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.FromRelations(new Relation(left, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, right));

        var reflection = Assert.Single(trolley.Observe().ForwardContinuations);
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), reflection.Transmission);
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), reflection.Retention);
        Assert.Equal(new Ratio(new Measure(5), Measure.One), reflection.TransmittedCalibration);
        Assert.Equal(new Ratio(new Measure(5), Measure.One), reflection.RetainedCalibration);
        Assert.True(reflection.HasRetention);

        Assert.True(trolley.TryContinuePreferred(out var returned));
        Assert.True(SameNode(returned.Site, left));
        Assert.Equal(new Ratio(new Measure(5), Measure.One), returned.Signature.Calibration);
    }

    [Fact]
    public void TrolleyWrapsToKnownClosureTargetInsteadOfReflectingAtTerminal()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(0)),
                Ratio.Tension(new Measure(10)),
                Provenance.Shared(new Measure(10)));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.Create(
                [right],
                new Relation(left, middle),
                new Relation(middle, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, middle));
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, right));

        var closure = Assert.Single(trolley.Observe().ForwardContinuations);
        Assert.False(closure.ReturnsAlongIngress);
        Assert.True(SameNode(closure.To, left));
        Assert.Contains(
            closure.BasisAgreements,
            agreement => agreement.Encounter.IsExplicitPinnedBasis && agreement.InvertsDirection);

        Assert.True(trolley.TryContinuePreferred(out var wrapped));
        Assert.True(SameNode(wrapped.Site, left));
        Assert.True(wrapped.TryContinuePreferred(out var continued));
        Assert.True(SameNode(continued.Site, middle));
    }

    [Fact]
    public void TrolleyCanCloseToKnownTargetWithInvertedPerspective()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(14)),
                Ratio.Tension(new Measure(4)),
                Provenance.Shared(new Measure(4)));
        var wrapTarget = Site.Open(x, new Measure(14));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.Create(
                [right, wrapTarget],
                new Relation(left, middle),
                new Relation(middle, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, right));

        var closure = Assert.Single(trolley.Observe().ForwardContinuations);
        Assert.True(SameNode(closure.To, wrapTarget));
        Assert.Equal(-Measure.One, closure.Direction);
        Assert.True(closure.BestBasisAgreement.PreservesDirection);

        Assert.True(trolley.TryContinuePreferred(out var wrapped));
        Assert.True(SameNode(wrapped.Site, wrapTarget));
        Assert.Equal(-Measure.One, wrapped.Direction);
        Assert.Equal(-Measure.One, wrapped.Signature.Perspective);
        Assert.Equal(-Measure.One, wrapped.Signature.EffectiveDirection);
    }

    [Fact]
    public void TrolleyPrefersMoreCommittedClosureWhenMultipleRemapsExist()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(0)),
                Ratio.Tension(new Measure(10)),
                Provenance.Shared(new Measure(10)))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(5)),
                new Ratio(new Measure(2), Measure.One),
                Provenance.Shared(new Measure(2)));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.Create(
                [right],
                new Relation(left, middle),
                new Relation(middle, right)),
            left);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(SameNode(trolley.Site, right));

        var closureObservation = trolley.Observe();
        Assert.Equal(2, closureObservation.ForwardContinuations.Length);
        Assert.True(closureObservation.TryPreferred(out var preferred));
        Assert.True(SameNode(preferred.To, left));
        Assert.Equal(new Ratio(Measure.One, Measure.One), preferred.BestBasisAgreement.Commitment);
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
    public void TrolleyPrefersBranchThatPreservesLocalBasisDirectionAtYJunction()
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
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.True(SameNode(preferred.To, rightEnd));

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
            continuation => SameNode(continuation.To, stemEnd));

        Assert.True(branch.HasBasisAgreement);
        Assert.True(branch.BestBasisAgreement.PreservesDirection);
        Assert.True(branch.BestBasisAgreement.IsOneSided);
        Assert.True(branch.BestBasisAgreement.Encounter.HasDominantCalibration);
    }

    [Fact]
    public void TrolleyUsesBasisAgreementToBreakOtherwiseEqualBranchTie()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var left = Site.Open(x, new Measure(0));
        var yStart = Site.Open(y, new Measure(0));
        var yMiddle = Site.Open(y, new Measure(4));
        var yEnd = Site.Open(y, new Measure(8));
        var zStart = Site.Open(z, new Measure(0));
        var zEnd = Site.Open(z, new Measure(4));
        var center = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(yStart, yMiddle),
                Ratio.Tension(new Measure(4)))
            .Request(
                new Relation(yMiddle, yEnd),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(zStart, zEnd),
                Ratio.Tension(new Measure(4)));

        var topology = ConnectedSiteTopology.Create(
            [center],
            new Relation(left, center));

        var trolley = TopologyTrolley.Start(topology, left);
        Assert.True(trolley.TryContinuePreferred(out trolley));

        var observation = trolley.Observe();
        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.Equal(y, preferred.Carrier);
        Assert.True(preferred.BestBasisAgreement.IsBilateral);
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
        var upper = Assert.Single(branch.Where(continuation => SameNode(continuation.To, upperApex)));
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
        Assert.Contains(crossObservation.ForwardContinuations, continuation => SameNode(continuation.To, verticalHigh));
        Assert.DoesNotContain(crossObservation.ForwardContinuations, continuation => SameNode(continuation.To, verticalLow));
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
        var upper = Assert.Single(trolley.Observe().ForwardContinuations, continuation => SameNode(continuation.To, upperApex));
        trolley = trolley.Move(upper);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));
        var upperVertical = Assert.Single(trolley.Observe().ForwardContinuations, continuation => SameNode(continuation.To, verticalHigh));
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
        var upper = Assert.Single(trolley.Observe().ForwardContinuations, continuation => SameNode(continuation.To, upperApex));
        trolley = trolley.Move(upper);
        Assert.True(trolley.TryContinuePreferred(out trolley));
        Assert.True(trolley.TryContinuePreferred(out trolley));

        Assert.True(SameNode(trolley.Site, cross));
        var backToMerge = trolley.Move(Assert.Single(
            trolley.Observe(-Measure.One).ForwardContinuationsIn(-Measure.One),
            continuation => SameNode(continuation.To, merge)));

        var reverseObservation = backToMerge.Observe(-Measure.One);
        Assert.Equal(2, reverseObservation.ForwardContinuationsIn(-Measure.One).Length);
        Assert.Contains(reverseObservation.ForwardContinuationsIn(-Measure.One), continuation => SameNode(continuation.To, upperApex));
        Assert.Contains(reverseObservation.ForwardContinuationsIn(-Measure.One), continuation => SameNode(continuation.To, lowerApex));
        Assert.DoesNotContain(reverseObservation.ForwardContinuationsIn(-Measure.One), continuation => SameNode(continuation.To, cross));
    }

    [Fact]
    public void TrolleyContinuesAcrossDistinctExactSitesThatShareWholeLocation()
    {
        Carrier x = 1;

        var left = Site.Open(x.At(new Ratio(new Measure(1), new Measure(4))));
        var right = Site.Open(x.At(new Ratio(new Measure(3), new Measure(4))));

        var trolley = TopologyTrolley.Start(
            ConnectedSiteTopology.FromRelations(new Relation(left, right)),
            left);

        Assert.True(trolley.TryContinuePreferred(out var advanced));
        Assert.True(SameNode(advanced.Site, right));
        Assert.Equal(new Ratio(new Measure(3), new Measure(4)), advanced.Site.On(x).Normalize());
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();
}
