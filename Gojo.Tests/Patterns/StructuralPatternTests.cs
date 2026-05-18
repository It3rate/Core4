using Gojo.Core;
using Gojo.Patterns;

namespace Gojo.Tests.Patterns;

public sealed class StructuralPatternTests
{
    [Fact]
    public void BindingReplacesEveryMatchingPlaceholder()
    {
        var slot = new Placeholder();
        var carrier = new Carrier();
        var pattern = new Relation(slot, slot);

        var bound = StructuralPattern.Apply(pattern, StructuralPattern.Bind(slot, carrier));

        Assert.Equal(new Relation(carrier, carrier), bound);
    }

    [Fact]
    public void BindingCanReplaceAPlaceholderWithRelationStructure()
    {
        var sourceSlot = new Placeholder();
        var targetSlot = new Placeholder();
        var source = new Carrier();
        var target = new Carrier();
        var pattern = new Relation(sourceSlot, targetSlot);

        var bound = StructuralPattern.Apply(
            pattern,
            StructuralPattern.Bind(sourceSlot, Relation.Point(source, 10)),
            StructuralPattern.Bind(targetSlot, Relation.Point(target, 0)));

        Assert.Equal(
            new Relation(Relation.Point(source, 10), Relation.Point(target, 0)),
            bound);
    }

    [Fact]
    public void RepeatedPatternBuildsDecimalCarryChain()
    {
        var sourceSlot = new Placeholder();
        var targetSlot = new Placeholder();
        var ones = new Carrier();
        var tens = new Carrier();
        var hundreds = new Carrier();
        var carryPattern = new Relation(sourceSlot, targetSlot);

        var carries = StructuralPattern.Repeat(
            carryPattern,
            [
                StructuralPattern.Bind(sourceSlot, Relation.Point(ones, 10)),
                StructuralPattern.Bind(targetSlot, Relation.Point(tens, 0))
            ],
            [
                StructuralPattern.Bind(sourceSlot, Relation.Point(tens, 10)),
                StructuralPattern.Bind(targetSlot, Relation.Point(hundreds, 0))
            ]);

        foreach (var carry in carries.OfType<Relation>())
        {
            ones.Attach(carry);
        }

        ones.Add(375);

        Assert.Equal(5UL, ones.Value);
        Assert.Equal(7UL, tens.Value);
        Assert.Equal(3UL, hundreds.Value);
    }

    [Fact]
    public void RepeatedPatternCanPreserveSharedInputAsPowerLikeParticipation()
    {
        var slot = new Placeholder();
        var value = new Carrier();
        var copies = StructuralPattern.Repeat(
            slot,
            [StructuralPattern.Bind(slot, value)],
            [StructuralPattern.Bind(slot, value)],
            [StructuralPattern.Bind(slot, value)]);

        var chain = StructuralPattern.Chain(copies);

        Assert.Equal(new Relation(new Relation(value, value), value), chain);
    }
}
