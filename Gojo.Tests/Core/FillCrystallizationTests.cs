using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class FillCrystallizationTests
{
    [Fact]
    public void SegmentCrystallizesAsFilledIntervalCarrier()
    {
        var line = new Carrier();
        var segment = Relation.Segment(line, 2, 7);

        Assert.True(segment.TryCrystallizeFill(out var fill));

        Assert.Equal(5UL, fill.Value);
        Assert.False(fill.IsInverse);
        Assert.False(fill.SameIdentityAs(line));
        Assert.True(fill.UnitShape.CompatibleWith(line.UnitShape));
    }

    [Fact]
    public void ReversedSegmentCrystallizesAsInverseFilledIntervalCarrier()
    {
        var line = new Carrier();
        var segment = Relation.Segment(line, 7, 2);

        Assert.True(segment.TryCrystallizeFill(out var fill));

        Assert.Equal(5UL, fill.Value);
        Assert.True(fill.IsInverse);
        Assert.False(fill.SameIdentityAs(line));
        Assert.True(fill.UnitShape.CompatibleWith(line.UnitShape));
    }

    [Fact]
    public void ZeroLengthFrameDoesNotHardenAsFill()
    {
        var line = new Carrier();
        var segment = Relation.Segment(line, 4, 4);

        Assert.False(segment.TryCrystallizeFill(out _));
    }

    [Fact]
    public void PointFrameDoesNotHardenAsFill()
    {
        var line = new Carrier();
        var point = Relation.Point(line, 4);

        Assert.False(point.TryCrystallizeFill(out _));
    }

    [Fact]
    public void UnboundFrameDoesNotHardenAsFill()
    {
        var unbound = Relation.Segment(Carrier.Unknown, 0, 8);

        Assert.False(unbound.TryCrystallizeFill(out _));
    }
}
