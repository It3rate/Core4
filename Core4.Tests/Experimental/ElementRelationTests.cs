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
        var carrier = new ExperimentalCarrier(7, Domain: 3, PolarityRaw: 1);
        var relation = new ExperimentalRelation(carrier, MergeScope.Whole);

        Assert.Single(relation.Entries);
        Assert.Single(relation.Signature.Entries);

        var entry = relation.Entries[0];
        var signature = relation.Signature.Entries[0];

        Assert.Equal(carrier, entry.Attached);
        Assert.Equal(MergeScope.Whole, entry.MergeScope);
        Assert.Equal(ExperimentalMeasure.Zero, entry.Commitment);
        Assert.Equal(ExperimentalMeasure.One, entry.Multiplicity);

        Assert.Equal(new ExperimentalMeasure(1), signature.Polarity);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), signature.MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), signature.CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), signature.DomainNovelty);
        Assert.Equal(ExperimentalMeasure.One, signature.Multiplicity);
    }

    [Fact]
    public void RelationAddAppendsEntriesWithoutMutatingExistingRelation()
    {
        var baseRelation = new ExperimentalRelation(new ExperimentalCarrier(1, Domain: 10), MergeScope.Whole);
        var nextRelation = baseRelation.Add(
            new ExperimentalMeasure(12, PolarityRaw: -1),
            MergeScope.Node,
            commitment: new ExperimentalMeasure(5),
            multiplicity: new ExperimentalMeasure(3));

        Assert.Single(baseRelation.Entries);
        Assert.Equal(2, nextRelation.Entries.Length);
        Assert.Single(baseRelation.Signature.Entries);
        Assert.Equal(2, nextRelation.Signature.Entries.Length);

        var appended = nextRelation.Entries[1];
        var appendedSignature = nextRelation.Signature.Entries[1];

        Assert.Equal(new ExperimentalMeasure(12, -1), appended.Attached);
        Assert.Equal(MergeScope.Node, appended.MergeScope);
        Assert.Equal(new ExperimentalMeasure(5), appended.Commitment);
        Assert.Equal(new ExperimentalMeasure(3), appended.Multiplicity);

        Assert.Equal(new ExperimentalMeasure(-1), appendedSignature.Polarity);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Node), appendedSignature.MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), appendedSignature.CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), appendedSignature.DomainNovelty);
        Assert.Equal(new ExperimentalMeasure(3), appendedSignature.Multiplicity);
    }

    [Fact]
    public void RelationTracksCarrierAndDomainNoveltyAcrossAddedElements()
    {
        var lineA = new ExperimentalCarrier(1, Domain: 100, PolarityRaw: 1);
        var lineB = new ExperimentalCarrier(2, Domain: 100, PolarityRaw: -1);
        var lineC = new ExperimentalCarrier(9, Domain: 300, PolarityRaw: 1);

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
            .Add(new ExperimentalMeasure(4, PolarityRaw: -1), MergeScope.Node);
        var right = new ExperimentalRelation(new ExperimentalCarrier(2, Domain: 10), MergeScope.Whole)
            .Add(new ExperimentalMeasure(8, PolarityRaw: 1), MergeScope.Node);
        var fused = new ExperimentalRelation(left, MergeScope.Node)
            .Add(right, MergeScope.Whole, multiplicity: new ExperimentalMeasure(2));

        Assert.Equal(2, fused.Entries.Length);
        Assert.Equal(2, fused.Signature.Entries.Length);

        Assert.Equal(new ExperimentalMeasure(-1), fused.Signature.Entries[0].Polarity);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Node), fused.Signature.Entries[0].MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), fused.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), fused.Signature.Entries[0].DomainNovelty);

        Assert.Equal(new ExperimentalMeasure(1), fused.Signature.Entries[1].Polarity);
        Assert.Equal(new ExperimentalMeasure((long)MergeScope.Whole), fused.Signature.Entries[1].MergeScope);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), fused.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), fused.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure(2), fused.Signature.Entries[1].Multiplicity);
    }

    [Fact]
    public void RelationConvenienceConstructorBuildsWholeScopedChain()
    {
        var relation = new ExperimentalRelation(
            new ExperimentalCarrier(1, Domain: 20),
            new ExperimentalCarrier(2, Domain: 20),
            new ExperimentalMeasure(9));

        Assert.Equal(3, relation.Entries.Length);
        Assert.All(relation.Entries, entry => Assert.Equal(MergeScope.Whole, entry.MergeScope));
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[0].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.New), relation.Signature.Entries[1].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[1].DomainNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[2].CarrierNovelty);
        Assert.Equal(new ExperimentalMeasure((long)Novelty.Existing), relation.Signature.Entries[2].DomainNovelty);
    }
}
