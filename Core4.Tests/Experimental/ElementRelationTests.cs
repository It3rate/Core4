using Core4.Aliases;
using Core4.Encoding;
using Core4.Experimental;
using AliasRelation = Core4.Aliases.Relation;
using ExperimentalRelation = Core4.Experimental.Relation;

namespace Core4.Tests.Experimental;

public sealed class ElementRelationTests
{
    [Fact]
    public void CoreFreezeCandidatesImplementIElement()
    {
        Carrier x = 1;
        Measure value = 3;
        var site = Site.Open(x, new Measure(5));
        var relation = new AliasRelation(
            Site.Open(x, Measure.Zero),
            Site.Open(x, Measure.One));
        var basis = relation.Freeze();
        var samples = SampleSet.Collect(site);
        var attachment = basis.Attach(site);

        Assert.IsAssignableFrom<IElement>(value);
        Assert.IsAssignableFrom<IElement>(x);
        Assert.IsAssignableFrom<IElement>(site);
        Assert.IsAssignableFrom<IElement>(relation);
        Assert.IsAssignableFrom<IElement>(basis);
        Assert.IsAssignableFrom<IElement>(samples);
        Assert.IsAssignableFrom<IElement>(attachment);
        Assert.IsAssignableFrom<IElement>(site.Placement);
    }

    [Fact]
    public void ExperimentalRelationCanRelateElementsAndRelationsWithOneShape()
    {
        Carrier temperature = 1;
        Carrier fahrenheit = new(2, temperature.Domain);
        Measure minusForty = -40;

        IElement carrierAlignment = new ExperimentalRelation(temperature, fahrenheit);
        var landmark = new ExperimentalRelation(temperature, minusForty, new Ratio(Measure.One, Measure.One));
        var calibration = new ExperimentalRelation(
            landmark,
            carrierAlignment,
            new Ratio(Measure.One, Measure.One),
            new Measure(2));

        Assert.IsAssignableFrom<IElement>(carrierAlignment);
        Assert.IsAssignableFrom<IElement>(landmark);
        Assert.IsAssignableFrom<IElement>(calibration);

        Assert.Equal(temperature, landmark.Base);
        Assert.Equal(minusForty, landmark.Attached);
        Assert.Equal(carrierAlignment, calibration.Attached);
        Assert.Equal(new Measure(2), calibration.Multiplicity);
    }

    [Fact]
    public void ExperimentalRelationInspectionDistinguishesCarrierPointAndSecondOrderPatterns()
    {
        Carrier x = 1;
        Carrier y = 2;
        var point = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));
        var alignedCarrier = new ExperimentalRelation(x, y);
        var fused = new ExperimentalRelation(point, alignedCarrier, new Ratio(Measure.One, Measure.One), new Measure(3));

        Assert.True(point.IsFirstOrder());
        Assert.True(point.IsCarrierPointRelation());
        Assert.False(point.IsCarrierAlignment());
        Assert.True(alignedCarrier.IsCarrierAlignment());
        Assert.True(fused.IsSecondOrder());
        Assert.True(fused.IsRelationToRelation());
        Assert.True(fused.IsRepeated());
        Assert.True(point.SharesBase(alignedCarrier));
    }

    [Fact]
    public void ExperimentalRelationInspectionReadsSameCarrierPointPairAsUnitCandidate()
    {
        Carrier x = 1;
        var origin = new ExperimentalRelation(x, Measure.Zero, new Ratio(Measure.One, Measure.One));
        var unit = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));
        var unitCandidate = new ExperimentalRelation(origin, unit, new Ratio(Measure.One, Measure.One));

        Assert.True(unitCandidate.IsPointPair());
        Assert.True(unitCandidate.IsSameCarrierPointPair());
        Assert.False(unitCandidate.IsCrossCarrierPointPair());
        Assert.True(unitCandidate.CanReadAsUnitCandidate());
        Assert.False(unitCandidate.CanReadAsJoinCandidate());
        Assert.Equal(new Measure(10), unitCandidate.AttachedOffset());
        Assert.True(unitCandidate.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation));
        Assert.Equal(x, baseCarrier);
        Assert.Equal(x, attachedCarrier);
        Assert.Equal(Measure.Zero, baseLocation);
        Assert.Equal(new Measure(10), attachedLocation);
    }

    [Fact]
    public void ExperimentalRelationInspectionReadsCrossCarrierPointPairAsJoinCandidate()
    {
        Carrier x = 1;
        Carrier y = 2;
        var xPoint = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));
        var yPoint = new ExperimentalRelation(y, new Measure(10), new Ratio(Measure.One, Measure.One));
        var joinCandidate = new ExperimentalRelation(xPoint, yPoint, new Ratio(Measure.One, Measure.One));

        Assert.True(joinCandidate.IsPointPair());
        Assert.False(joinCandidate.IsSameCarrierPointPair());
        Assert.True(joinCandidate.IsCrossCarrierPointPair());
        Assert.False(joinCandidate.CanReadAsUnitCandidate());
        Assert.True(joinCandidate.CanReadAsJoinCandidate());
        Assert.Equal(Measure.Zero, joinCandidate.AttachedOffset());
    }
}
