using Core4.Inspection;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class PropagationWaveTests
{
    [Fact]
    public void WaveParksTerminiThenReturnsOnlyAfterOutboundQuiescence()
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

        var wave = PropagationWave.Start(topology, stemBase);

        var intoCenter = wave.Advance();
        Assert.True(intoCenter.HasMotion);
        Assert.True(intoCenter.After.IsOutbound);
        Assert.Single(intoCenter.After.Active);
        Assert.True(SameNode(intoCenter.After.Active[0].Site, center));

        var intoBranches = intoCenter.After.Advance();
        Assert.True(intoBranches.HasMotion);
        Assert.True(intoBranches.After.IsOutbound);
        Assert.Equal(2, intoBranches.After.Active.Length);
        Assert.Contains(intoBranches.After.Active, frontier => SameNode(frontier.Site, leftEnd));
        Assert.Contains(intoBranches.After.Active, frontier => SameNode(frontier.Site, rightEnd));

        var phaseFlip = intoBranches.After.Advance();
        Assert.False(phaseFlip.HasMotion);
        Assert.True(phaseFlip.After.IsReturning);
        Assert.Equal(2, phaseFlip.After.Active.Length);
        Assert.Empty(phaseFlip.After.Parked);

        var mergeReturn = phaseFlip.After.Advance();
        Assert.True(mergeReturn.HasMotion);
        Assert.True(mergeReturn.After.IsReturning);
        var merged = Assert.Single(mergeReturn.After.Active);
        Assert.True(SameNode(merged.Site, center));
        Assert.Equal(new Measure(2), merged.Multiplicity);

        var home = mergeReturn.After.Advance();
        Assert.True(home.HasMotion);
        var returned = Assert.Single(home.After.Active);
        Assert.True(SameNode(returned.Site, stemBase));
        Assert.Equal(new Measure(2), returned.Multiplicity);

        var complete = home.After.Advance();
        Assert.False(complete.HasMotion);
        Assert.True(complete.After.IsComplete);
        var parkedHome = Assert.Single(complete.After.Parked);
        Assert.True(SameNode(parkedHome.Site, stemBase));
        Assert.Equal(new Measure(2), parkedHome.Multiplicity);
    }

    [Fact]
    public void WaveSplitsAgainOnReverseFlowAfterDiamondMerge()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

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

        var topology = ConnectedSiteTopology.Create(
            [entry, merge],
            new Relation(left, entry));

        var wave = PropagationWave.Start(topology, left);

        wave = wave.Advance().After;
        wave = wave.Advance().After;
        var merged = wave.Advance();

        Assert.True(merged.HasMotion);
        var mergedFront = Assert.Single(merged.After.Active);
        Assert.True(SameNode(mergedFront.Site, merge));
        Assert.Equal(new Measure(2), mergedFront.Multiplicity);

        var flip = merged.After.Advance();
        Assert.False(flip.HasMotion);
        Assert.True(flip.After.IsReturning);

        var split = flip.After.Advance();
        Assert.True(split.HasMotion);
        Assert.Equal(2, split.After.Active.Length);
        Assert.Contains(split.After.Active, frontier => SameNode(frontier.Site, upperApex));
        Assert.Contains(split.After.Active, frontier => SameNode(frontier.Site, lowerApex));
        Assert.Contains(split.Moves, move => move.Relation.Carrier == y && SameNode(move.Target.Site, upperApex));
        Assert.Contains(split.Moves, move => move.Relation.Carrier == z && SameNode(move.Target.Site, lowerApex));
    }

    [Fact]
    public void WavePartiallyReflectsAtExplicitBoundaryAndRetainsResidualWeight()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(6)),
                new Ratio(new Measure(2), Measure.One),
                Provenance.Shared(new Measure(2)));

        var wave = PropagationWave.Start(
            ConnectedSiteTopology.FromRelations(new Relation(left, right)),
            left);

        var atBoundary = wave.Advance();
        Assert.True(atBoundary.HasMotion);
        var boundary = Assert.Single(atBoundary.After.Active);
        Assert.True(SameNode(boundary.Site, right));
        Assert.Equal(new Ratio(Measure.One, Measure.One), boundary.WeightRead.AsRatio());
        Assert.Equal(new Ratio(Measure.One, Measure.One), boundary.IntensityRead.AsRatio());
        Assert.Equal(new Ratio(Measure.One, Measure.One), boundary.Intensity);

        var split = atBoundary.After.Advance();
        Assert.True(split.HasMotion);
        var reflected = Assert.Single(split.After.Active);
        Assert.True(SameNode(reflected.Site, left));
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), reflected.WeightRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), reflected.IntensityRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), reflected.Intensity);

        var retained = Assert.Single(split.After.Parked);
        Assert.True(SameNode(retained.Site, right));
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), retained.WeightRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), retained.IntensityRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), retained.Intensity);

        var move = Assert.Single(split.Moves);
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), move.TransmissionRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), move.RetentionRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), move.Transmission);
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), move.Retention);
    }

    [Fact]
    public void WaveCompletesAsPeriodicAfterExactReentryOnSimpleClosureLoop()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(0)),
                Ratio.Tension(new Measure(10)),
                Provenance.Shared(new Measure(10)));

        var wave = PropagationWave.Start(
            ConnectedSiteTopology.Create(
                [right],
                new Relation(left, middle),
                new Relation(middle, right)),
            left);

        wave = wave.Advance().After;
        Assert.True(wave.IsOutbound);
        Assert.Single(wave.Active);
        Assert.True(SameNode(wave.Active[0].Site, middle));

        wave = wave.Advance().After;
        Assert.True(wave.IsOutbound);
        Assert.Single(wave.Active);
        Assert.True(SameNode(wave.Active[0].Site, right));

        wave = wave.Advance().After;
        Assert.True(wave.IsOutbound);
        Assert.Single(wave.Active);
        Assert.True(SameNode(wave.Active[0].Site, left));
        Assert.False(wave.IsPeriodic);

        var periodic = wave.Advance();
        Assert.True(periodic.HasMotion);
        Assert.True(periodic.After.IsComplete);
        Assert.True(periodic.After.IsPeriodic);
        Assert.Equal(new Measure(3), periodic.After.Period);
        var frontier = Assert.Single(periodic.After.Parked);
        Assert.True(SameNode(frontier.Site, middle));
        Assert.Equal(new Measure(3), frontier.ExactReentryPeriod);
    }

    [Fact]
    public void WaveAdvancesAcrossDistinctExactSitesThatShareWholeLocation()
    {
        Carrier x = 1;

        var left = Site.Open(x.At(new Ratio(new Measure(1), new Measure(4))));
        var right = Site.Open(x.At(new Ratio(new Measure(3), new Measure(4))));

        var wave = PropagationWave.Start(
            ConnectedSiteTopology.FromRelations(new Relation(left, right)),
            left);

        var step = wave.Advance();

        Assert.True(step.HasMotion);
        var frontier = Assert.Single(step.After.Active);
        Assert.True(SameNode(frontier.Site, right));
        Assert.Equal(new Ratio(new Measure(3), new Measure(4)), frontier.Site.On(x).Normalize());
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();
}
