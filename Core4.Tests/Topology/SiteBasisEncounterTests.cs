using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class SiteBasisEncounterTests
{
    [Fact]
    public void InteriorSiteExposesBilateralAndOneSidedCarrierEncounters()
    {
        Carrier x = 1;
        var left = Site.Open(x, new Measure(0));
        var middle = Site.Open(x, new Measure(5));
        var right = Site.Open(x, new Measure(10));

        var encounters = ConnectedSiteTopology.FromRelations(
            new Relation(left, middle),
            new Relation(middle, right))
            .BasisEncountersAt(middle, x);

        Assert.Equal(3, encounters.Length);

        var bilateral = encounters[0];
        Assert.True(bilateral.IsBilateral);
        Assert.True(bilateral.IsLocalToSite);
        Assert.True(bilateral.SupportsDirection(-Measure.One));
        Assert.True(bilateral.SupportsDirection(Measure.One));

        Assert.Contains(encounters, encounter => encounter.IsAtomic && encounter.HasRecessiveCalibration);
        Assert.Contains(encounters, encounter => encounter.IsAtomic && encounter.HasDominantCalibration);
    }

    [Fact]
    public void TerminalBranchStartExposesOneSidedOutgoingEncounter()
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

        var branchEncounters = topology.BasisEncountersAt(center, y);
        var chainEncounters = topology.BasisEncountersAt(center, x);

        var branch = Assert.Single(branchEncounters);
        Assert.True(branch.IsAtomic);
        Assert.True(branch.IsOneSided);
        Assert.True(branch.HasDominantCalibration);
        Assert.False(branch.HasRecessiveCalibration);
        Assert.True(branch.SupportsDirection(Measure.One));
        Assert.False(branch.SupportsDirection(-Measure.One));

        Assert.Contains(chainEncounters, encounter => encounter.IsBilateral);
    }

    [Fact]
    public void ExplicitSameCarrierPinnedBasisIsSurfacedAtSite()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(6)),
                Ratio.Tension(new Measure(4)),
                Provenance.Shared(new Measure(4)));

        var encounters = ConnectedSiteTopology.FromRelations(new Relation(left, right))
            .BasisEncountersAt(right, x);

        var explicitBasis = Assert.Single(encounters, encounter => encounter.IsExplicitPinnedBasis);
        Assert.True(explicitBasis.IsAtomic);
        Assert.True(explicitBasis.IsLocalToSite);
        Assert.True(explicitBasis.HasRecessiveCalibration);
        Assert.False(explicitBasis.HasDominantCalibration);
        Assert.Equal(explicitBasis.Basis.Fold, explicitBasis.Fold);
        Assert.Equal(explicitBasis.Basis.Fold, explicitBasis.CalibrationRead.Fold);
        Assert.Equal(explicitBasis.Basis.Potential, explicitBasis.CalibrationRead.Potential);
        Assert.Equal(new Ratio(Measure.One, Measure.One), explicitBasis.Commitment);
    }

    [Fact]
    public void ExplicitPinnedBasisCanBePartiallyCommittedByAttachedValue()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(6)),
                new Ratio(new Measure(2), Measure.One),
                Provenance.Shared(new Measure(2)));

        var encounters = ConnectedSiteTopology.FromRelations(new Relation(left, right))
            .BasisEncountersAt(right, x);

        var explicitBasis = Assert.Single(encounters, encounter => encounter.IsExplicitPinnedBasis);
        Assert.Equal(new Ratio(new Measure(1), new Measure(2)), explicitBasis.Commitment);
    }

    [Fact]
    public void EncounterAgreementCarriesEncounteredSupportFoldRead()
    {
        Carrier x = 1;

        var left = Site.Open(x, new Measure(0));
        var right = Site.Open(x, new Measure(10))
            .Exchange(
                Basis.Freeze(x, new Measure(10), new Measure(6)),
                Ratio.Tension(new Measure(4)),
                Provenance.Shared(new Measure(4)));

        var encounter = Assert.Single(
            ConnectedSiteTopology.FromRelations(new Relation(left, right))
                .BasisEncountersAt(right, x),
            candidate => candidate.IsExplicitPinnedBasis);
        var signature = TraversalSignature.FromSite(
            right,
            Measure.One,
            new Ratio(new Measure(10), Measure.One),
            Provenance.SelfDriven(new Measure(10)));

        var agreement = encounter.Agree(signature, Measure.One);

        Assert.Equal(encounter.Fold, agreement.Calibration);
        Assert.Equal(encounter.Potential, agreement.Potential);
        Assert.Equal(encounter.Fold, agreement.CalibrationRead.Fold);
        Assert.Equal(encounter.Potential, agreement.CalibrationRead.Potential);
        Assert.Equal(new Ratio(new Measure(4), new Measure(10)), agreement.Resolution);
    }
}
