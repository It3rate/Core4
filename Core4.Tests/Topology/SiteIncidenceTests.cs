using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class SiteIncidenceTests
{
    [Fact]
    public void LiveRelationActsAsTraversableSupport()
    {
        Carrier x = 1;
        var relation = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(10)));

        var midpoint = relation.Place(new Ratio(1, 2));
        var onCarrier = midpoint.On(x);

        Assert.Equal(x, midpoint.PrimaryCarrier);
        Assert.Equal(Measure.Zero, midpoint.SupportGrade);
        Assert.Equal(new Ratio(10, 2), onCarrier);
        Assert.Equal(new Measure(5), onCarrier.Whole);
    }

    [Fact]
    public void SiteIncidenceCapturesContinuationAndOutgoingBranchLeg()
    {
        Carrier x = 1;
        Carrier y = 2;

        var site = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var left = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(5)));
        var right = new Relation(
            Site.Open(x, new Measure(5)),
            Site.Open(x, new Measure(10)));

        var incidence = SiteIncidence.Create(site, left, right);

        Assert.Equal(new Measure(3), incidence.LegCount);
        Assert.Equal(new Measure(2), incidence.CarrierCount);
        Assert.Equal(new Measure(1), incidence.IncomingLegCount);
        Assert.Equal(new Measure(2), incidence.OutgoingLegCount);
        Assert.True(incidence.SiteCarrierContinues);
        Assert.True(incidence.Continues(x));
        Assert.False(incidence.Terminates(x));
        Assert.True(incidence.HasOutgoing(y));
        Assert.False(incidence.HasIncoming(y));
        Assert.True(incidence.Terminates(y));
        Assert.Equal(new Measure(1), incidence.ContinuingCarrierCount);
        Assert.Equal(new Measure(1), incidence.TerminalCarrierCount);
    }

    [Fact]
    public void SiteIncidenceCapturesContinuationAndIncomingJoinLeg()
    {
        Carrier x = 1;
        Carrier y = 2;

        var site = Site.Open(x, new Measure(5))
            .Request(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var left = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(5)));
        var right = new Relation(
            Site.Open(x, new Measure(5)),
            Site.Open(x, new Measure(10)));

        var incidence = SiteIncidence.Create(site, left, right);

        Assert.Equal(new Measure(3), incidence.LegCount);
        Assert.Equal(new Measure(2), incidence.IncomingLegCount);
        Assert.Equal(new Measure(1), incidence.OutgoingLegCount);
        Assert.True(incidence.Continues(x));
        Assert.True(incidence.HasIncoming(y));
        Assert.False(incidence.HasOutgoing(y));
        Assert.True(incidence.Terminates(y));
        Assert.Equal(new Measure(1), incidence.ContinuingCarrierCount);
        Assert.Equal(new Measure(1), incidence.TerminalCarrierCount);
    }

    [Fact]
    public void SiteIncidenceDelegatesCarrierEncounterAssemblyToEncounterLayer()
    {
        Carrier x = 1;
        Carrier y = 2;

        var site = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var left = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(5)));
        var right = new Relation(
            Site.Open(x, new Measure(5)),
            Site.Open(x, new Measure(10)));

        var incidence = SiteIncidence.Create(site, left, right);
        var xEncounters = incidence.BasisEncountersOn(x);
        var yEncounters = incidence.BasisEncountersOn(y);

        Assert.Contains(xEncounters, encounter => encounter.IsBilateral);

        var branch = Assert.Single(yEncounters);
        Assert.True(branch.IsAtomic);
        Assert.True(branch.IsOneSided);
        Assert.True(branch.HasDominantCalibration);
        Assert.False(branch.HasRecessiveCalibration);
    }
}
