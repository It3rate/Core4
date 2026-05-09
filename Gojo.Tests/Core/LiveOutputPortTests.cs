using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class LiveOutputPortTests
{
    [Fact]
    public void InputToProjectionPortContinuouslyFeedsConcreteOutputCarrier()
    {
        var source = new Carrier();
        var target = new Carrier();
        var port = new Relation(Segment(source, 0, 2), Segment(target, 0, 1));
        source.Attach(new Relation(source.ValueSlot, port));

        source.Set(8);

        Assert.Equal(8UL, source.Value);
        Assert.Equal(4UL, target.Value);

        source.Set(10);

        Assert.Equal(10UL, source.Value);
        Assert.Equal(5UL, target.Value);
    }

    [Fact]
    public void InputGroupToOperationPortContinuouslyFeedsConcreteOutputCarrier()
    {
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Segment(result, 0, 1));
        x.Attach(new Relation(new Relation(x.ValueSlot, y.ValueSlot), operation));

        x.Set(3);
        y.Set(4);

        Assert.Equal(12UL, result.Value);

        x.Set(5);

        Assert.Equal(20UL, result.Value);
    }

    [Fact]
    public void LiveProjectionOutputDoesNotAccumulateReadResidueAsRepeatedTension()
    {
        var source = new Carrier();
        var target = new Carrier();
        var port = new Relation(Segment(source, 0, 3), Segment(target, 0, 1));
        source.Attach(new Relation(source.ValueSlot, port));

        source.Set(5);
        target.Add(0);

        Assert.Equal(1UL, target.Value);
        Assert.Equal(0, source.Tension);
    }

    [Fact]
    public void ProjectionResidueCanBePersistedThroughExplicitResiduePort()
    {
        var source = new Carrier();
        var target = new Carrier();
        var residue = new Carrier();
        var valuePort = new Relation(Segment(source, 0, 3), Segment(target, 0, 1));
        var residuePort = new Relation(Point(Carrier.Unknown, 1), Point(residue, 1));
        source.Attach(new Relation(source.ValueSlot, new Relation(valuePort, residuePort)));

        source.Set(5);

        Assert.Equal(1UL, target.Value);
        Assert.Equal(2UL, residue.Value);
        Assert.Equal(0, source.Tension);

        source.Set(7);

        Assert.Equal(2UL, target.Value);
        Assert.Equal(1UL, residue.Value);
        Assert.Equal(0, source.Tension);
    }

    [Fact]
    public void PersistedProjectionResidueDoesNotAccumulateAcrossSettlePasses()
    {
        var source = new Carrier();
        var target = new Carrier();
        var residue = new Carrier();
        var valuePort = new Relation(Segment(source, 0, 3), Segment(target, 0, 1));
        var residuePort = new Relation(Point(Carrier.Unknown, 1), Point(residue, 1));
        source.Attach(new Relation(source.ValueSlot, new Relation(valuePort, residuePort)));

        source.Set(5);
        target.Add(0);

        Assert.Equal(1UL, target.Value);
        Assert.Equal(2UL, residue.Value);
        Assert.Equal(0, source.Tension);
    }

    [Fact]
    public void OperationResidueShapeCanBePersistedThroughRelationShapedResiduePorts()
    {
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var residueX = new Carrier();
        var residueY = new Carrier();
        var xPort = new Relation(Segment(x, 0, 10), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Segment(y, 0, 10), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));
        var residueOutput = new Relation(
            new Relation(Point(Carrier.Unknown, 1), Point(residueX, 1)),
            new Relation(Point(Carrier.Unknown, 1), Point(residueY, 1)));
        x.Attach(new Relation(
            new Relation(x.ValueSlot, y.ValueSlot),
            new Relation(operation, residueOutput)));

        x.Set(17);
        y.Set(28);

        Assert.Equal(3UL, result.Value);
        Assert.Equal(7UL, residueX.Value);
        Assert.Equal(8UL, residueY.Value);

        x.Set(30);
        y.Set(41);

        Assert.Equal(7UL, result.Value);
        Assert.Equal(0UL, residueX.Value);
        Assert.Equal(1UL, residueY.Value);
    }

    [Fact]
    public void OperationResidueChainCanBeAggregatedThroughSingleResiduePort()
    {
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var residue = new Carrier();
        var xPort = new Relation(Segment(x, 0, 10), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Segment(y, 0, 10), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));
        var residueOutput = new Relation(Point(Carrier.Unknown, 1), Point(residue, 1));
        x.Attach(new Relation(
            new Relation(x.ValueSlot, y.ValueSlot),
            new Relation(operation, residueOutput)));

        x.Set(17);
        y.Set(28);

        Assert.Equal(3UL, result.Value);
        Assert.Equal(15UL, residue.Value);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));
}
