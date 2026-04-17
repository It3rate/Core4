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
    public void ExperimentalRelationSignatureStaysOnPrimitiveParticipationAxes()
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

        Assert.Equal(Code(RelationPolarity.Dominant), pointSignature.Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), pointSignature.Polarity.Attached);
        Assert.Equal(Code(RelationMergeScope.Whole), pointSignature.MergeScope.Base);
        Assert.Equal(Code(RelationMergeScope.Node), pointSignature.MergeScope.Attached);
        Assert.Equal(Code(RelationNovelty.Unknown), pointSignature.CarrierRelation);
        Assert.Equal(Code(RelationNovelty.Unknown), pointSignature.DomainRelation);

        Assert.Equal(Code(RelationPolarity.Dominant), fusedSignature.Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), fusedSignature.Polarity.Attached);
        Assert.Equal(Code(RelationMergeScope.Node), fusedSignature.MergeScope.Base);
        Assert.Equal(Code(RelationMergeScope.Whole), fusedSignature.MergeScope.Attached);
        Assert.Equal(Code(RelationNovelty.Existing), fusedSignature.CarrierRelation);
        Assert.Equal(Code(RelationNovelty.Existing), fusedSignature.DomainRelation);
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
        Assert.Equal(Code(RelationPolarity.Dominant), signature.Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), signature.Polarity.Attached);
        Assert.Equal(Code(RelationMergeScope.Node), signature.MergeScope.Base);
        Assert.Equal(Code(RelationMergeScope.Node), signature.MergeScope.Attached);
        Assert.Equal(Code(RelationNovelty.Existing), signature.CarrierRelation);
        Assert.Equal(Code(RelationNovelty.Existing), signature.DomainRelation);
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
        Assert.Equal(Code(RelationPolarity.Dominant), signature.Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), signature.Polarity.Attached);
        Assert.Equal(Code(RelationMergeScope.Node), signature.MergeScope.Base);
        Assert.Equal(Code(RelationMergeScope.Node), signature.MergeScope.Attached);
        Assert.Equal(Code(RelationNovelty.New), signature.CarrierRelation);
        Assert.Equal(Code(RelationNovelty.New), signature.DomainRelation);
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
        var sameDomainAligned = new ExperimentalRelation(
            new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One)),
            new ExperimentalRelation(y, new Measure(20), new Ratio(Measure.One, Measure.One)),
            new Ratio(Measure.One, Measure.One));

        Assert.True(self.CanReadAsSelfCandidate());
        Assert.False(self.CanReadAsAlignedCandidate());
        Assert.False(self.CanReadAsOpposedCandidate());

        Assert.False(aligned.CanReadAsSelfCandidate());
        Assert.True(aligned.CanReadAsAlignedCandidate());
        Assert.False(aligned.CanReadAsOpposedCandidate());
        Assert.Equal(Code(RelationPolarity.Dominant), aligned.Signature().Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), aligned.Signature().Polarity.Attached);

        Assert.False(opposed.CanReadAsSelfCandidate());
        Assert.False(opposed.CanReadAsAlignedCandidate());
        Assert.True(opposed.CanReadAsOpposedCandidate());
        Assert.Equal(Code(RelationPolarity.Recessive), opposed.Signature().Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), opposed.Signature().Polarity.Attached);

        Assert.True(sameDomainAligned.IsSameDomainCrossCarrierPointPair());
        Assert.True(sameDomainAligned.CanReadAsAlignedCandidate());
        Assert.Equal(Code(RelationNovelty.New), sameDomainAligned.Signature().CarrierRelation);
        Assert.Equal(Code(RelationNovelty.Existing), sameDomainAligned.Signature().DomainRelation);
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
        Assert.Equal(Code(RelationPolarity.Recessive), segment.Signature().Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), segment.Signature().Polarity.Attached);
        Assert.Equal(Code(RelationMergeScope.Whole), segment.Signature().MergeScope.Base);
        Assert.Equal(Code(RelationMergeScope.Whole), segment.Signature().MergeScope.Attached);
        Assert.Equal(Code(RelationNovelty.Existing), segment.Signature().CarrierRelation);
        Assert.Equal(Code(RelationNovelty.Existing), segment.Signature().DomainRelation);

        Assert.True(self.CanReadAsSelfInteractionCandidate());
        Assert.Equal(Code(RelationPolarity.Dominant), self.Signature().Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), self.Signature().Polarity.Attached);

        Assert.True(area.IsUnitCandidatePair());
        Assert.True(area.IsCrossCarrierUnitCandidatePair());
        Assert.True(area.SharesUnitBase());
        Assert.True(area.CanReadAsAreaCandidate());
        Assert.Equal(Code(RelationPolarity.Dominant), area.Signature().Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), area.Signature().Polarity.Attached);
        Assert.Equal(Code(RelationNovelty.New), area.Signature().CarrierRelation);
        Assert.Equal(Code(RelationNovelty.New), area.Signature().DomainRelation);

        Assert.True(complex.CanReadAsComplexCandidate());
        Assert.Equal(Code(RelationPolarity.Dominant), complex.Signature().Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Recessive), complex.Signature().Polarity.Attached);
    }

    [Fact]
    public void ExperimentalRelationInspectionKeepsHigherOrderSignaturePrimitive()
    {
        Carrier x = 1;
        Carrier y = 2;

        var originX = new ExperimentalRelation(x, Measure.Zero, new Ratio(Measure.One, Measure.One));
        var leftX = new ExperimentalRelation(x, new Measure(-10), new Ratio(Measure.One, Measure.One));
        var rightX = new ExperimentalRelation(x, new Measure(10), new Ratio(Measure.One, Measure.One));

        var originY = new ExperimentalRelation(y, Measure.Zero, new Ratio(Measure.One, Measure.One));
        var upY = new ExperimentalRelation(y, new Measure(20), new Ratio(Measure.One, Measure.One));

        var leftUnit = new ExperimentalRelation(originX, leftX, new Ratio(Measure.One, Measure.One));
        var rightUnit = new ExperimentalRelation(originX, rightX, new Ratio(Measure.One, Measure.One));
        var upUnit = new ExperimentalRelation(originY, upY, new Ratio(Measure.One, Measure.One));

        var segment = new ExperimentalRelation(leftUnit, rightUnit, new Ratio(Measure.One, Measure.One));
        var area = new ExperimentalRelation(rightUnit, upUnit, new Ratio(Measure.One, Measure.One));
        var hybrid = new ExperimentalRelation(area, segment, new Ratio(Measure.One, Measure.One));
        var signature = hybrid.Signature();

        Assert.Equal(new Measure(4), hybrid.FreezeOrder());
        Assert.Equal(Code(RelationPolarity.Dominant), signature.Polarity.Base);
        Assert.Equal(Code(RelationPolarity.Dominant), signature.Polarity.Attached);
        Assert.Equal(Code(RelationMergeScope.Whole), signature.MergeScope.Base);
        Assert.Equal(Code(RelationMergeScope.Whole), signature.MergeScope.Attached);
        Assert.Equal(Code(RelationNovelty.Existing), signature.CarrierRelation);
        Assert.Equal(Code(RelationNovelty.Existing), signature.DomainRelation);
        Assert.False(hybrid.CanReadAsAreaCandidate());
        Assert.False(hybrid.CanReadAsDirectedSegmentCandidate());
    }

    private static RelationCode Code(RelationPolarity value) => RelationCode.Of(value);
    private static RelationCode Code(RelationMergeScope value) => RelationCode.Of(value);
    private static RelationCode Code(RelationNovelty value) => RelationCode.Of(value);
}
