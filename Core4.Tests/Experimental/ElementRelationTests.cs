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
        var pointSignature = point.Signature();
        var fusedSignature = fused.Signature();

        Assert.True(point.IsFirstOrder());
        Assert.True(point.IsCarrierPointRelation());
        Assert.False(point.IsCarrierAlignment());
        Assert.True(alignedCarrier.IsCarrierAlignment());
        Assert.True(fused.IsSecondOrder());
        Assert.True(fused.IsRelationToRelation());
        Assert.True(fused.IsRepeated());
        Assert.True(point.SharesBase(alignedCarrier));
        Assert.Equal(new ElementOrderSignature(Measure.One, new Measure(2)), pointSignature.ElementOrder);
        Assert.Equal(Measure.One, pointSignature.FreezeOrder);
        Assert.Equal(Measure.Zero, pointSignature.CarrierPattern);
        Assert.Equal(Measure.Zero, pointSignature.BasePattern);
        Assert.Equal(Measure.One, pointSignature.ReadPattern);
        Assert.Equal(Measure.Zero, pointSignature.AlignmentPattern);
        Assert.Equal(Measure.Zero, pointSignature.FusionPattern);
        Assert.Equal(new Measure(3), fusedSignature.ElementOrder.BaseOrder);
        Assert.Equal(new Measure(3), fusedSignature.ElementOrder.AttachedOrder);
        Assert.Equal(new Measure(2), fusedSignature.FreezeOrder);
        Assert.Equal(Measure.Zero, fusedSignature.CarrierPattern);
        Assert.Equal(new Measure(2), fusedSignature.BasePattern);
        Assert.Equal(Measure.Zero, fusedSignature.ReadPattern);
        Assert.Equal(Measure.Zero, fusedSignature.AlignmentPattern);
        Assert.Equal(Measure.Zero, fusedSignature.FusionPattern);
        Assert.Equal(new Measure(3), fusedSignature.Multiplicity);
    }

    [Fact]
    public void ExperimentalRelationInspectionReadsSameCarrierPointPairAsUnitCandidate()
    {
        Carrier x = 1;
        var origin = new ExperimentalRelation(x, Measure.Zero, new Ratio(Measure.One, Measure.One));
        var unit = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));
        var unitCandidate = new ExperimentalRelation(origin, unit, new Ratio(Measure.One, Measure.One));
        var signature = unitCandidate.Signature();

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
        Assert.Equal(new ElementOrderSignature(new Measure(3), new Measure(3)), signature.ElementOrder);
        Assert.Equal(new Measure(2), signature.FreezeOrder);
        Assert.Equal(Measure.One, signature.CarrierPattern);
        Assert.Equal(new Measure(2), signature.BasePattern);
        Assert.Equal(new Measure(4), signature.ReadPattern);
        Assert.Equal(Measure.Zero, signature.AlignmentPattern);
        Assert.Equal(Measure.Zero, signature.FusionPattern);
        Assert.Equal(Measure.One, signature.Multiplicity);
    }

    [Fact]
    public void ExperimentalRelationInspectionReadsCrossCarrierPointPairAsJoinCandidate()
    {
        Carrier x = 1;
        Carrier y = 2;
        var xPoint = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));
        var yPoint = new ExperimentalRelation(y, new Measure(10), new Ratio(Measure.One, Measure.One));
        var joinCandidate = new ExperimentalRelation(xPoint, yPoint, new Ratio(Measure.One, Measure.One));
        var signature = joinCandidate.Signature();

        Assert.True(joinCandidate.IsPointPair());
        Assert.False(joinCandidate.IsSameCarrierPointPair());
        Assert.True(joinCandidate.IsCrossCarrierPointPair());
        Assert.False(joinCandidate.IsSameDomainCrossCarrierPointPair());
        Assert.False(joinCandidate.CanReadAsUnitCandidate());
        Assert.True(joinCandidate.CanReadAsJoinCandidate());
        Assert.Equal(Measure.Zero, joinCandidate.AttachedOffset());
        Assert.Equal(new Measure(2), signature.FreezeOrder);
        Assert.Equal(new Measure(3), signature.CarrierPattern);
        Assert.Equal(Measure.One, signature.BasePattern);
        Assert.Equal(new Measure(5), signature.ReadPattern);
        Assert.Equal(Measure.Zero, signature.AlignmentPattern);
        Assert.Equal(Measure.Zero, signature.FusionPattern);
    }

    [Fact]
    public void ExperimentalRelationInspectionReadsAlignmentPatternsForComparablePointPairs()
    {
        Carrier x = 1;
        Carrier y = new(2, x.Domain);

        var self = new ExperimentalRelation(
            new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One)),
            new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One)),
            new Ratio(Measure.One, Measure.One));
        var aligned = new ExperimentalRelation(
            new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One)),
            new ExperimentalRelation(x, new Measure(20), new Ratio(Measure.One, Measure.One)),
            new Ratio(Measure.One, Measure.One));
        var opposed = new ExperimentalRelation(
            new ExperimentalRelation(x, new Measure(-10), new Ratio(Measure.One, Measure.One)),
            new ExperimentalRelation(x, new Measure(20), new Ratio(Measure.One, Measure.One)),
            new Ratio(Measure.One, Measure.One));
        var crossDomainAligned = new ExperimentalRelation(
            new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One)),
            new ExperimentalRelation(y, new Measure(20), new Ratio(Measure.One, Measure.One)),
            new Ratio(Measure.One, Measure.One));

        Assert.True(self.CanReadAsSelfCandidate());
        Assert.False(self.CanReadAsAlignedCandidate());
        Assert.False(self.CanReadAsOpposedCandidate());
        Assert.Equal(Measure.One, self.AlignmentPattern());

        Assert.False(aligned.CanReadAsSelfCandidate());
        Assert.True(aligned.CanReadAsAlignedCandidate());
        Assert.False(aligned.CanReadAsOpposedCandidate());
        Assert.Equal(new Measure(2), aligned.AlignmentPattern());

        Assert.False(opposed.CanReadAsSelfCandidate());
        Assert.False(opposed.CanReadAsAlignedCandidate());
        Assert.True(opposed.CanReadAsOpposedCandidate());
        Assert.Equal(new Measure(3), opposed.AlignmentPattern());

        Assert.True(crossDomainAligned.IsSameDomainCrossCarrierPointPair());
        Assert.True(crossDomainAligned.CanReadAsAlignedCandidate());
        Assert.Equal(new Measure(2), crossDomainAligned.Signature().AlignmentPattern);
    }

    [Fact]
    public void ExperimentalRelationInspectionReadsFusionPatternsForUnitCandidatePairs()
    {
        Carrier x = 1;
        Carrier y = 2;

        var originX = new ExperimentalRelation(x, Measure.Zero, new Ratio(Measure.One, Measure.One));
        var leftX = new ExperimentalRelation(x, new Measure(-10), new Ratio(Measure.One, Measure.One));
        var rightX = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));
        var highX = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));

        var originY = new ExperimentalRelation(y, Measure.Zero, new Ratio(Measure.One, Measure.One));
        var upY = new ExperimentalRelation(y, new Measure(20), new Ratio(Measure.One, Measure.One));
        var downY = new ExperimentalRelation(y, new Measure(-20), new Ratio(Measure.One, Measure.One));

        var leftUnit = new ExperimentalRelation(originX, leftX, new Ratio(Measure.One, Measure.One));
        var rightUnit = new ExperimentalRelation(originX, rightX, new Ratio(Measure.One, Measure.One));
        var selfUnit = new ExperimentalRelation(originX, highX, new Ratio(Measure.One, Measure.One));
        var upUnit = new ExperimentalRelation(originY, upY, new Ratio(Measure.One, Measure.One));
        var downUnit = new ExperimentalRelation(originY, downY, new Ratio(Measure.One, Measure.One));

        var segment = new ExperimentalRelation(leftUnit, rightUnit, new Ratio(Measure.One, Measure.One));
        var self = new ExperimentalRelation(rightUnit, selfUnit, new Ratio(Measure.One, Measure.One));
        var area = new ExperimentalRelation(rightUnit, upUnit, new Ratio(Measure.One, Measure.One));
        var complex = new ExperimentalRelation(rightUnit, downUnit, new Ratio(Measure.One, Measure.One));

        Assert.True(segment.IsUnitCandidatePair());
        Assert.True(segment.IsSameCarrierUnitCandidatePair());
        Assert.True(segment.SharesUnitBase());
        Assert.True(segment.CanReadAsDirectedSegmentCandidate());
        Assert.Equal(new Measure(6), segment.Signature().ReadPattern);
        Assert.Equal(new Measure(2), segment.FusionPattern());
        Assert.Equal(new Measure(2), segment.Signature().FusionPattern);

        Assert.True(self.CanReadAsSelfInteractionCandidate());
        Assert.Equal(Measure.One, self.FusionPattern());

        Assert.True(area.IsUnitCandidatePair());
        Assert.True(area.IsCrossCarrierUnitCandidatePair());
        Assert.True(area.SharesUnitBase());
        Assert.True(area.CanReadAsAreaCandidate());
        Assert.Equal(new Measure(3), area.Signature().CarrierPattern);
        Assert.Equal(new Measure(3), area.FusionPattern());

        Assert.True(complex.CanReadAsComplexCandidate());
        Assert.Equal(new Measure(4), complex.FusionPattern());
    }
}
