using Core4.Experimental;
using ExperimentalElement = Core4.Experimental.IElement;
using ExperimentalCarrier = Core4.Experimental.Carrier;
using ExperimentalMeasure = Core4.Experimental.Measure;
using ExperimentalRelation = Core4.Experimental.Relation;

namespace Core4.Tests.Experimental;

public sealed class ElementRelationTests
{
    [Fact]
    public void ExperimentalPrimitivesImplementExperimentalElement()
    {
        ExperimentalElement measure = new ExperimentalMeasure(5);
        ExperimentalElement carrier = new ExperimentalCarrier(1);
        ExperimentalElement relation = new ExperimentalRelation(new ExperimentalCarrier(1), MergeScope.Whole);

        Assert.IsAssignableFrom<ExperimentalElement>(measure);
        Assert.IsAssignableFrom<ExperimentalElement>(carrier);
        Assert.IsAssignableFrom<ExperimentalElement>(relation);
        Assert.Equal(ExperimentalMeasure.Zero, measure.Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, carrier.Polarity);
    }

    [Fact]
    public void RelationStartsFromOneAttachedElementAndCalculatesSignatureImmediately()
    {
        var carrier = new ExperimentalCarrier(7, Domain: 3);
        var relation = new ExperimentalRelation(carrier, MergeScope.Whole);

        Assert.Single(relation.Entries);
        Assert.Single(relation.Signature.Entries);

        var entry = relation.Entries[0];
        var signature = relation.Signature.Entries[0];

        Assert.Equal(carrier, entry.Attached);
        Assert.Equal(MergeScope.Whole, entry.MergeScope);
        Assert.Equal(ExperimentalRelation.DefaultCommitment, entry.Commitment);
        Assert.Null(entry.Transform);
        Assert.Null(entry.Aspect);

        Assert.Equal(ExperimentalMeasure.Zero, signature.Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, signature.Transform);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), signature.MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), signature.CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), signature.DomainNovelty);
    }

    [Fact]
    public void RelationAddExpandsMultiplicityIntoRepeatedEntriesWithoutMutatingExistingRelation()
    {
        var baseRelation = new ExperimentalRelation(new ExperimentalCarrier(1, Domain: 10), MergeScope.Whole);
        var nextRelation = baseRelation.Add(
            new ExperimentalMeasure(12),
            MergeScope.Node,
            commitment: new ExperimentalMeasure(5),
            multiplicity: new ExperimentalMeasure(3));

        Assert.Single(baseRelation.Entries);
        Assert.Equal(4, nextRelation.Entries.Length);
        Assert.Single(baseRelation.Signature.Entries);
        Assert.Equal(4, nextRelation.Signature.Entries.Length);

        foreach (var index in Enumerable.Range(1, 3))
        {
            var appended = nextRelation.Entries[index];
            var appendedSignature = nextRelation.Signature.Entries[index];

            Assert.Equal(new ExperimentalMeasure(12), appended.Attached);
            Assert.Equal(MergeScope.Node, appended.MergeScope);
            Assert.Equal(new ExperimentalMeasure(5), appended.Commitment);
            Assert.Null(appended.Transform);
            Assert.Null(appended.Aspect);

            Assert.Equal(ExperimentalMeasure.Zero, appendedSignature.Polarity);
            Assert.Equal(ExperimentalMeasure.Zero, appendedSignature.Transform);
            Assert.Equal(new ExperimentalMeasure((long)MergeScope.Node), appendedSignature.MergeScope);
            Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), appendedSignature.CarrierNovelty);
            Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), appendedSignature.DomainNovelty);
        }
    }

    [Fact]
    public void RelationTracksCarrierAndDomainNoveltyAcrossAddedElements()
    {
        var lineA = new ExperimentalCarrier(1, Domain: 100);
        var lineB = new ExperimentalCarrier(2, Domain: 100);
        var lineC = new ExperimentalCarrier(9, Domain: 300);

        var relation = new ExperimentalRelation(lineA, MergeScope.Whole)
            .Add(lineA, MergeScope.Node)
            .Add(lineB, MergeScope.Whole)
            .Add(lineC, MergeScope.Whole);

        Assert.Equal(4, relation.Signature.Entries.Length);

        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[0].DomainNovelty);

        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[1].DomainNovelty);

        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[2].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[2].DomainNovelty);

        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[3].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[3].DomainNovelty);
    }

    [Fact]
    public void RelationCanAttachAnotherRelationAsOneElement()
    {
        var left = new ExperimentalRelation(new ExperimentalCarrier(1, Domain: 10), MergeScope.Whole)
            .Add(new ExperimentalMeasure(4), MergeScope.Node)
            .Add(new ExperimentalMeasure(1), MergeScope.Node);
        var right = new ExperimentalRelation(new ExperimentalCarrier(2, Domain: 10), MergeScope.Whole)
            .Add(new ExperimentalMeasure(8), MergeScope.Node)
            .Add(new ExperimentalMeasure(20), MergeScope.Node);
        var fused = new ExperimentalRelation(left, MergeScope.Node)
            .Add(right, MergeScope.Whole, multiplicity: new ExperimentalMeasure(2));

        Assert.Equal(3, fused.Entries.Length);
        Assert.Equal(3, fused.Signature.Entries.Length);

        Assert.Equal(new ExperimentalMeasure(-1), fused.Signature.Entries[0].Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, fused.Signature.Entries[0].Transform);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Node), fused.Signature.Entries[0].MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), fused.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), fused.Signature.Entries[0].DomainNovelty);

        Assert.Equal(new ExperimentalMeasure(1), fused.Signature.Entries[1].Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, fused.Signature.Entries[1].Transform);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), fused.Signature.Entries[1].MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), fused.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), fused.Signature.Entries[1].DomainNovelty);

        Assert.Equal(new ExperimentalMeasure(1), fused.Signature.Entries[2].Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, fused.Signature.Entries[2].Transform);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), fused.Signature.Entries[2].MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), fused.Signature.Entries[2].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), fused.Signature.Entries[2].DomainNovelty);
    }

    [Fact]
    public void RelationCanReuseElementUnderExplicitTransform()
    {
        var carrier = new ExperimentalCarrier(1, Domain: 10);
        var unit = Unit(carrier, new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var transformed = new ExperimentalRelation(unit, MergeScope.Whole)
            .Add(unit, MergeScope.Whole, transform: new ExperimentalMeasure(-1));

        Assert.Equal(2, transformed.Entries.Length);
        Assert.Null(transformed.Entries[0].Transform);
        Assert.Null(transformed.Entries[0].Aspect);
        Assert.Equal(new ExperimentalMeasure(-1), transformed.Entries[1].Transform);
        Assert.Null(transformed.Entries[1].Aspect);

        Assert.Equal(new ExperimentalMeasure(1), transformed.Signature.Entries[0].Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, transformed.Signature.Entries[0].Transform);
        Assert.Equal(new ExperimentalMeasure(-1), transformed.Signature.Entries[1].Polarity);
        Assert.Equal(new ExperimentalMeasure(-1), transformed.Signature.Entries[1].Transform);
    }

    [Fact]
    public void RelationConvenienceConstructorBuildsWholeScopedChain()
    {
        var relation = new ExperimentalRelation(
            new ExperimentalCarrier(1, Domain: 20),
            new ExperimentalCarrier(2, Domain: 20),
            new ExperimentalMeasure(9));

        Assert.Equal(3, relation.Entries.Length);
        Assert.All(relation.Entries, entry =>
        {
            Assert.Equal(ExperimentalRelation.DefaultMergeScope, entry.MergeScope);
            Assert.Equal(ExperimentalRelation.DefaultCommitment, entry.Commitment);
            Assert.Null(entry.Transform);
            Assert.Null(entry.Aspect);
        });
        Assert.Equal(ExperimentalMeasure.Zero, relation.Signature.Entries[0].Transform);
        Assert.Equal(ExperimentalMeasure.Zero, relation.Signature.Entries[1].Transform);
        Assert.Equal(ExperimentalMeasure.Zero, relation.Signature.Entries[2].Transform);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[2].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[2].DomainNovelty);
    }

    [Fact]
    public void LandmarkBuildsFromCarrierAndPinnedMeasureWithoutBecomingUnit()
    {
        var carrier = new ExperimentalCarrier(1, Domain: 10);
        var landmark = new ExperimentalRelation(carrier, new ExperimentalMeasure(100));

        Assert.Equal(2, landmark.Entries.Length);
        Assert.Equal(ExperimentalMeasure.Zero, landmark.Polarity);
        Assert.Equal(ExperimentalRelation.DefaultMergeScope, landmark.Entries[0].MergeScope);
        Assert.Equal(ExperimentalRelation.DefaultMergeScope, landmark.Entries[1].MergeScope);
        Assert.Equal(ExperimentalRelation.DefaultCommitment, landmark.Entries[0].Commitment);
        Assert.Equal(ExperimentalRelation.DefaultCommitment, landmark.Entries[1].Commitment);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), landmark.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), landmark.Signature.Entries[0].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), landmark.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), landmark.Signature.Entries[1].DomainNovelty);
    }

    [Fact]
    public void DominantAndRecessiveUnitsCanBeBuiltWithoutUsingZeroAsOrigin()
    {
        var carrier = new ExperimentalCarrier(1, Domain: 10);
        var dominantUnit = Unit(carrier, new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var recessiveUnit = Unit(carrier, new ExperimentalMeasure(200), new ExperimentalMeasure(100));

        Assert.Equal(3, dominantUnit.Entries.Length);
        Assert.Equal(new ExperimentalMeasure(1), dominantUnit.Polarity);
        Assert.Equal(new ExperimentalMeasure(-1), recessiveUnit.Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, dominantUnit.Signature.Entries[1].Polarity);
        Assert.Equal(ExperimentalMeasure.Zero, dominantUnit.Signature.Entries[2].Polarity);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), dominantUnit.Signature.Entries[2].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), recessiveUnit.Signature.Entries[2].CarrierNovelty);
    }

    [Fact]
    public void OpposedAndRepeatedWholeUnitMergesPreservePolarityWithoutSharedOriginClaims()
    {
        var carrier = new ExperimentalCarrier(1, Domain: 10);
        var dominantUnit = Unit(carrier, new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var recessiveUnit = Unit(carrier, new ExperimentalMeasure(200), new ExperimentalMeasure(100));

        var opposedMerge = new ExperimentalRelation(recessiveUnit, MergeScope.Whole)
            .Add(dominantUnit, MergeScope.Whole);
        var selfInteraction = new ExperimentalRelation(dominantUnit, MergeScope.Whole)
            .Add(dominantUnit, MergeScope.Whole);

        Assert.Equal(new ExperimentalMeasure(-1), opposedMerge.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(1), opposedMerge.Signature.Entries[1].Polarity);
        Assert.All(opposedMerge.Signature.Entries, entry => Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), entry.MergeScope));
        Assert.Equal(new ExperimentalMeasure(1), selfInteraction.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(1), selfInteraction.Signature.Entries[1].Polarity);
        Assert.All(selfInteraction.Signature.Entries, entry => Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), entry.MergeScope));
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), opposedMerge.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), selfInteraction.Signature.Entries[1].CarrierNovelty);
    }

    [Fact]
    public void RepeatedSameTimeCarrierParticipationReadsAsOneCarrierLineage()
    {
        var distanceUnit = Unit(new ExperimentalCarrier(1, Domain: 10), new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var timeUnit = Unit(new ExperimentalCarrier(2, Domain: 20), new ExperimentalMeasure(1), new ExperimentalMeasure(2));

        var accelerationLike = new ExperimentalRelation(distanceUnit, MergeScope.Whole)
            .Add(timeUnit, MergeScope.Whole)
            .Add(timeUnit, MergeScope.Whole);

        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), accelerationLike.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), accelerationLike.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), accelerationLike.Signature.Entries[2].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), accelerationLike.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), accelerationLike.Signature.Entries[2].DomainNovelty);
    }

    [Fact]
    public void StandardObjectRecipesBuildDirectedAndSelfStructuresByReusingOneUnit()
    {
        var carrier = new ExperimentalCarrier(1, Domain: 10);
        var unit = Unit(carrier, new ExperimentalMeasure(100), new ExperimentalMeasure(200));

        var directed = DirectedFrom(unit);
        var self = SelfFrom(unit);

        Assert.Equal(2, directed.Entries.Length);
        Assert.Same(unit, directed.Entries[0].Attached);
        Assert.Same(unit, directed.Entries[1].Attached);
        Assert.Null(directed.Entries[0].Transform);
        Assert.Equal(new ExperimentalMeasure(-1), directed.Entries[1].Transform);
        Assert.Equal(new ExperimentalMeasure(1), directed.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(-1), directed.Signature.Entries[1].Polarity);

        Assert.Equal(2, self.Entries.Length);
        Assert.Same(unit, self.Entries[0].Attached);
        Assert.Same(unit, self.Entries[1].Attached);
        Assert.Null(self.Entries[0].Transform);
        Assert.Null(self.Entries[1].Transform);
        Assert.Equal(new ExperimentalMeasure(1), self.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(1), self.Signature.Entries[1].Polarity);
    }

    [Fact]
    public void StandardObjectRecipesBuildAreaAndComplexFromReusedCarrierLocalUnits()
    {
        var widthUnit = Unit(new ExperimentalCarrier(1, Domain: 10), new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var heightUnit = Unit(new ExperimentalCarrier(2, Domain: 10), new ExperimentalMeasure(50), new ExperimentalMeasure(80));

        var area = AreaFrom(widthUnit, heightUnit);
        var complex = ComplexFrom(widthUnit, heightUnit);

        Assert.Equal(2, area.Entries.Length);
        Assert.Same(widthUnit, area.Entries[0].Attached);
        Assert.Same(heightUnit, area.Entries[1].Attached);
        Assert.Null(area.Entries[0].Transform);
        Assert.Null(area.Entries[1].Transform);
        Assert.Equal(new ExperimentalMeasure(1), area.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(1), area.Signature.Entries[1].Polarity);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), area.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), area.Signature.Entries[1].DomainNovelty);

        Assert.Equal(2, complex.Entries.Length);
        Assert.Same(widthUnit, complex.Entries[0].Attached);
        Assert.Same(heightUnit, complex.Entries[1].Attached);
        Assert.Null(complex.Entries[0].Transform);
        Assert.Equal(new ExperimentalMeasure(-1), complex.Entries[1].Transform);
        Assert.Equal(new ExperimentalMeasure(1), complex.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(-1), complex.Signature.Entries[1].Polarity);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), complex.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), complex.Signature.Entries[1].DomainNovelty);
    }

    [Fact]
    public void ComplexLikeStructuresDifferentiateSharedUnitOrthogonalsFromIndependentUnits()
    {
        var distanceUnit = Unit(new ExperimentalCarrier(1, Domain: 10), new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var directionCarrier = new ExperimentalCarrier(2, Domain: 20);
        var horizontal = Landmark(directionCarrier, new ExperimentalMeasure(0));
        var vertical = Landmark(directionCarrier, new ExperimentalMeasure(90));

        var sharedUnitComplex = new ExperimentalRelation(distanceUnit, MergeScope.Whole, aspect: horizontal)
            .Add(distanceUnit, MergeScope.Whole, transform: new ExperimentalMeasure(-1), aspect: vertical);

        var orthogonalIndependentComplex = new ExperimentalRelation(
            Unit(new ExperimentalCarrier(3, Domain: 10), new ExperimentalMeasure(100), new ExperimentalMeasure(200)),
            MergeScope.Whole)
            .Add(
                Unit(new ExperimentalCarrier(4, Domain: 10), new ExperimentalMeasure(100), new ExperimentalMeasure(200)),
                MergeScope.Whole,
                transform: new ExperimentalMeasure(-1));

        var coincidentOriginByValueComplex = new ExperimentalRelation(
            Unit(new ExperimentalCarrier(5, Domain: 10), new ExperimentalMeasure(0), new ExperimentalMeasure(10)),
            MergeScope.Whole)
            .Add(
                Unit(new ExperimentalCarrier(6, Domain: 10), new ExperimentalMeasure(0), new ExperimentalMeasure(10)),
                MergeScope.Whole,
                transform: new ExperimentalMeasure(-1));

        Assert.Same(distanceUnit, sharedUnitComplex.Entries[0].Attached);
        Assert.Same(distanceUnit, sharedUnitComplex.Entries[1].Attached);
        Assert.Same(horizontal, sharedUnitComplex.Entries[0].Aspect);
        Assert.Same(vertical, sharedUnitComplex.Entries[1].Aspect);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), sharedUnitComplex.Signature.Entries[1].CarrierNovelty);

        Assert.NotSame(orthogonalIndependentComplex.Entries[0].Attached, orthogonalIndependentComplex.Entries[1].Attached);
        Assert.Null(orthogonalIndependentComplex.Entries[0].Aspect);
        Assert.Null(orthogonalIndependentComplex.Entries[1].Aspect);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), orthogonalIndependentComplex.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), orthogonalIndependentComplex.Signature.Entries[1].DomainNovelty);

        Assert.NotSame(coincidentOriginByValueComplex.Entries[0].Attached, coincidentOriginByValueComplex.Entries[1].Attached);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), coincidentOriginByValueComplex.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), coincidentOriginByValueComplex.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure(-1), coincidentOriginByValueComplex.Signature.Entries[1].Transform);
    }

    [Fact]
    public void StandardObjectRecipesCanReuseOneDistanceUnitUnderSnappedDirectionalAspects()
    {
        var distanceUnit = Unit(new ExperimentalCarrier(1, Domain: 10), new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var directionCarrier = new ExperimentalCarrier(2, Domain: 20);
        var horizontal = Landmark(directionCarrier, new ExperimentalMeasure(0));
        var vertical = Landmark(directionCarrier, new ExperimentalMeasure(90));

        var area = new ExperimentalRelation(distanceUnit, MergeScope.Whole, aspect: horizontal)
            .Add(distanceUnit, MergeScope.Whole, aspect: vertical);
        var complex = new ExperimentalRelation(distanceUnit, MergeScope.Whole, aspect: horizontal)
            .Add(distanceUnit, MergeScope.Whole, transform: new ExperimentalMeasure(-1), aspect: vertical);

        Assert.Same(distanceUnit, area.Entries[0].Attached);
        Assert.Same(distanceUnit, area.Entries[1].Attached);
        Assert.Same(horizontal, area.Entries[0].Aspect);
        Assert.Same(vertical, area.Entries[1].Aspect);
        Assert.Equal(new ExperimentalMeasure(1), area.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(1), area.Signature.Entries[1].Polarity);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), area.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), area.Signature.Entries[1].CarrierNovelty);

        Assert.Same(distanceUnit, complex.Entries[0].Attached);
        Assert.Same(distanceUnit, complex.Entries[1].Attached);
        Assert.Same(horizontal, complex.Entries[0].Aspect);
        Assert.Same(vertical, complex.Entries[1].Aspect);
        Assert.Equal(new ExperimentalMeasure(1), complex.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(-1), complex.Signature.Entries[1].Polarity);
        Assert.Equal(new ExperimentalMeasure(-1), complex.Signature.Entries[1].Transform);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), complex.Signature.Entries[1].CarrierNovelty);
    }

    [Fact]
    public void TwoCarrierWholeMergesCoverAreaComplexAntiComplexAndAntiArea()
    {
        var x = new ExperimentalCarrier(1, Domain: 10);
        var y = new ExperimentalCarrier(2, Domain: 20);

        var xDominant = Unit(x, new ExperimentalMeasure(100), new ExperimentalMeasure(200));
        var xRecessive = Unit(x, new ExperimentalMeasure(200), new ExperimentalMeasure(100));
        var yDominant = Unit(y, new ExperimentalMeasure(50), new ExperimentalMeasure(80));
        var yRecessive = Unit(y, new ExperimentalMeasure(80), new ExperimentalMeasure(50));

        var area = new ExperimentalRelation(xDominant, MergeScope.Whole).Add(yDominant, MergeScope.Whole);
        var complex = new ExperimentalRelation(xDominant, MergeScope.Whole).Add(yRecessive, MergeScope.Whole);
        var antiComplex = new ExperimentalRelation(xRecessive, MergeScope.Whole).Add(yDominant, MergeScope.Whole);
        var antiArea = new ExperimentalRelation(xRecessive, MergeScope.Whole).Add(yRecessive, MergeScope.Whole);

        AssertPairPolarity(area, 1, 1);
        AssertPairPolarity(complex, 1, -1);
        AssertPairPolarity(antiComplex, -1, 1);
        AssertPairPolarity(antiArea, -1, -1);

        Assert.All(area.Signature.Entries, entry => Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), entry.MergeScope));
        Assert.All(complex.Signature.Entries, entry => Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), entry.MergeScope));
        Assert.All(antiComplex.Signature.Entries, entry => Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), entry.MergeScope));
        Assert.All(antiArea.Signature.Entries, entry => Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), entry.MergeScope));

        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), area.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), area.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), complex.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), complex.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), antiComplex.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), antiComplex.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), antiArea.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), antiArea.Signature.Entries[1].DomainNovelty);
    }

    private static ExperimentalRelation Unit(
        ExperimentalCarrier carrier,
        ExperimentalMeasure originMeasure,
        ExperimentalMeasure unitMeasure) =>
        new ExperimentalRelation(carrier, originMeasure, unitMeasure);

    private static ExperimentalRelation Landmark(
        ExperimentalCarrier carrier,
        ExperimentalMeasure location) =>
        new ExperimentalRelation(carrier, location);

    private static ExperimentalRelation DirectedFrom(ExperimentalRelation unit) =>
        new ExperimentalRelation(unit, MergeScope.Whole).Add(unit, MergeScope.Whole, transform: new ExperimentalMeasure(-1));

    private static ExperimentalRelation SelfFrom(ExperimentalRelation unit) =>
        new ExperimentalRelation(unit, MergeScope.Whole).Add(unit, MergeScope.Whole);

    private static ExperimentalRelation AreaFrom(ExperimentalRelation widthUnit, ExperimentalRelation heightUnit) =>
        new ExperimentalRelation(widthUnit, MergeScope.Whole).Add(heightUnit, MergeScope.Whole);

    private static ExperimentalRelation ComplexFrom(ExperimentalRelation widthUnit, ExperimentalRelation heightUnit) =>
        new ExperimentalRelation(widthUnit, MergeScope.Whole).Add(heightUnit, MergeScope.Whole, transform: new ExperimentalMeasure(-1));

    private static void AssertPairPolarity(ExperimentalRelation relation, long @base, long attached)
    {
        Assert.Equal(new ExperimentalMeasure(@base), relation.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure(attached), relation.Signature.Entries[1].Polarity);
    }
}
