using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class OperationSurfaceTests
{
    [Fact]
    public void PointOutputOperationSurfaceAddsProjectedPortGroupValues()
    {
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));
        x.Set(3);
        y.Set(4);

        Assert.True(operation.TryReadOperationFrom(new Relation(x.ValueSlot, y.ValueSlot), result, out var read));

        Assert.Equal(
            new Relation(
                new CarrierLocation(result, 7),
                new Relation(new CarrierLocation(x, 0), new CarrierLocation(y, 0))),
            read);
    }

    [Fact]
    public void SegmentOutputOperationSurfaceMultipliesProjectedPortGroupValues()
    {
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Segment(result, 0, 1));
        x.Set(3);
        y.Set(4);

        Assert.True(operation.TryReadOperationFrom(new Relation(x.ValueSlot, y.ValueSlot), result, out var read));

        Assert.Equal(
            new Relation(
                new CarrierLocation(result, 12),
                new Relation(new CarrierLocation(x, 0), new CarrierLocation(y, 0))),
            read);
    }

    [Fact]
    public void SegmentOutputOperationWithOneInverseOperandWritesInverseOutputFace()
    {
        var x = new Carrier();
        var inverseX = x.Inverse();
        var y = new Carrier();
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Segment(result, 0, 1));

        Assert.True(operation.TryReadOperationFrom(
            new Relation(new CarrierLocation(inverseX, 2), new CarrierLocation(y, 3)),
            result,
            out var read));

        var target = Assert.IsType<CarrierLocation>(read.Left);
        Assert.True(target.Carrier.SameIdentityAs(result.Inverse()));
        Assert.Equal(6, target.Ticks);
    }

    [Fact]
    public void SegmentOutputOperationWithTwoInverseOperandsWritesForwardOutputFace()
    {
        var x = new Carrier();
        var inverseX = x.Inverse();
        var y = new Carrier();
        var inverseY = y.Inverse();
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Segment(result, 0, 1));

        Assert.True(operation.TryReadOperationFrom(
            new Relation(new CarrierLocation(inverseX, 2), new CarrierLocation(inverseY, 3)),
            result,
            out var read));

        var target = Assert.IsType<CarrierLocation>(read.Left);
        Assert.True(target.Carrier.SameIdentityAs(result));
        Assert.Equal(-6, target.Ticks);
    }

    [Fact]
    public void SegmentOutputOperationPreservesInverseMultiplicationCycle()
    {
        var real = new Carrier();
        var inverse = real.Inverse();
        var result = new Carrier(real.UnitShape);
        var leftPort = new Relation(Point(real, 1), Point(Carrier.Unknown, 1));
        var rightPort = new Relation(Point(real, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(leftPort, rightPort), Segment(result, 0, 1));

        AssertRead(new CarrierLocation(inverse, 1), new CarrierLocation(real, 1), result.Inverse(), 1);
        AssertRead(new CarrierLocation(inverse, 1), new CarrierLocation(inverse, 1), result, -1);
        AssertRead(new CarrierLocation(inverse, 1), new CarrierLocation(real, -1), result.Inverse(), -1);
        AssertRead(new CarrierLocation(inverse, 1), new CarrierLocation(inverse, -1), result, 1);

        void AssertRead(CarrierLocation left, CarrierLocation right, Carrier expectedCarrier, long expectedTicks)
        {
            Assert.True(operation.TryReadOperationFrom(new Relation(left, right), result, out var read));
            var target = Assert.IsType<CarrierLocation>(read.Left);
            Assert.True(target.Carrier.SameIdentityAs(expectedCarrier));
            Assert.Equal(expectedTicks, target.Ticks);
        }
    }

    [Fact]
    public void OperationSurfaceReadsThroughExposedPortBasisAndPreservesResidue()
    {
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var xPort = new Relation(Segment(x, 0, 10), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Segment(y, 0, 10), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));
        x.Set(17);
        y.Set(28);

        Assert.True(operation.TryReadOperationFrom(new Relation(x.ValueSlot, y.ValueSlot), result, out var read));

        Assert.Equal(
            new Relation(
                new CarrierLocation(result, 3),
                new Relation(new CarrierLocation(x, 7), new CarrierLocation(y, 8))),
            read);
    }

    [Fact]
    public void OperationSurfaceCanReadAtomicBasisInputsThroughPortGroup()
    {
        var q4 = new Carrier();
        var q5 = new Carrier();
        var result = new Carrier();
        var q4Port = new Relation(Segment(q4, 0, 4), Point(Carrier.Unknown, 1));
        var q5Port = new Relation(Segment(q5, 0, 5), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(q4Port, q5Port), Point(result, 1));

        Assert.True(operation.TryReadOperationFrom(
            new Relation(Segment(q4, 0, 7), Segment(q5, 0, 14)),
            result,
            out var read));

        Assert.Equal(
            new Relation(
                new CarrierLocation(result, 3),
                new Relation(new CarrierLocation(q4, 3), new CarrierLocation(q5, 4))),
            read);
    }

    [Fact]
    public void OperationSurfaceRejectsMismatchedPortGroupWithoutFallbackRead()
    {
        var x = new Carrier();
        var y = new Carrier();
        var other = new Carrier();
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));
        x.Set(3);
        other.Set(4);

        Assert.False(operation.TryReadOperationFrom(new Relation(x.ValueSlot, other.ValueSlot), result, out _));
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));
}
