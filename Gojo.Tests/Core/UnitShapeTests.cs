using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class UnitShapeTests
{
    [Fact]
    public void DefaultCarrierHasFreshLinearUnitShape()
    {
        var carrier = new Carrier();
        var other = new Carrier();

        Assert.True(carrier.UnitShape.CompatibleWith(UnitShape.Atom(carrier)));
        Assert.False(carrier.UnitShape.CompatibleWith(other.UnitShape));
    }

    [Fact]
    public void CarriersCanShareUnitShapeWithoutSharingIdentity()
    {
        var unitShape = new Carrier().UnitShape;
        var first = new Carrier(unitShape);
        var second = new Carrier(unitShape);

        Assert.True(first.UnitShape.CompatibleWith(second.UnitShape));
        Assert.False(first.SameIdentityAs(second));
    }

    [Fact]
    public void InverseCarrierSharesUnitShapeWithForwardCarrier()
    {
        var carrier = new Carrier();

        var inverse = carrier.Inverse();

        Assert.True(inverse.UnitShape.CompatibleWith(carrier.UnitShape));
    }

    [Fact]
    public void SameCarrierBasisReadsCarrierUnitShape()
    {
        var carrier = new Carrier();
        var basis = Segment(carrier, 0, 10);

        Assert.True(basis.TryReadUnitShape(out var unitShape));
        Assert.True(unitShape.CompatibleWith(carrier.UnitShape));
    }

    [Fact]
    public void TransferPortUsesConcreteTargetUnitShape()
    {
        var light = new Carrier();
        var heat = new Carrier();
        var port = new Relation(Point(light, 1), Point(heat, 1));

        Assert.True(port.TryReadUnitShape(out var unitShape));

        Assert.True(unitShape.CompatibleWith(heat.UnitShape));
        Assert.False(unitShape.CompatibleWith(light.UnitShape));
    }

    [Fact]
    public void UnknownOutputPortUsesSourceUnitShape()
    {
        var source = new Carrier();
        var port = new Relation(Segment(source, 0, 10), Point(Carrier.Unknown, 1));

        Assert.True(port.TryReadUnitShape(out var unitShape));

        Assert.True(unitShape.CompatibleWith(source.UnitShape));
    }

    [Fact]
    public void AdditiveOperationWithCompatibleUnitsCollapsesToThatUnitShape()
    {
        var lengthShape = new Carrier().UnitShape;
        var x = new Carrier(lengthShape);
        var y = new Carrier(lengthShape);
        var result = new Carrier();
        var xPort = new Relation(Point(x, 1), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Point(y, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));

        Assert.True(operation.TryReadUnitShape(out var unitShape));

        Assert.True(unitShape.CompatibleWith(lengthShape));
    }

    [Fact]
    public void AdditiveOperationWithIncompatibleUnitsPreservesUnreducedShape()
    {
        var length = new Carrier();
        var heat = new Carrier();
        var result = new Carrier();
        var lengthPort = new Relation(Point(length, 1), Point(Carrier.Unknown, 1));
        var heatPort = new Relation(Point(heat, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(lengthPort, heatPort), Point(result, 1));
        var expected = UnitShape.Additive(length.UnitShape, heat.UnitShape);

        Assert.True(operation.TryReadUnitShape(out var unitShape));

        Assert.True(unitShape.CompatibleWith(expected));
        Assert.False(unitShape.CompatibleWith(length.UnitShape));
        Assert.False(unitShape.CompatibleWith(heat.UnitShape));
    }

    [Fact]
    public void MultiplicativeOperationDerivesProductUnitShape()
    {
        var lengthShape = new Carrier().UnitShape;
        var width = new Carrier(lengthShape);
        var height = new Carrier(lengthShape);
        var result = new Carrier();
        var widthPort = new Relation(Point(width, 1), Point(Carrier.Unknown, 1));
        var heightPort = new Relation(Point(height, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(widthPort, heightPort), Segment(result, 0, 1));
        var expected = UnitShape.Product(lengthShape, lengthShape);

        Assert.True(operation.TryReadUnitShape(out var unitShape));

        Assert.True(unitShape.CompatibleWith(expected));
        Assert.Equal(2, unitShape.ProductParticipationOf(lengthShape));
    }

    [Fact]
    public void CrystallizedCarrierUsesDerivedUnitShapeWithoutProjectingValue()
    {
        var lengthShape = new Carrier().UnitShape;
        var width = new Carrier(lengthShape);
        var height = new Carrier(lengthShape);
        var result = new Carrier();
        var widthPort = new Relation(Point(width, 1), Point(Carrier.Unknown, 1));
        var heightPort = new Relation(Point(height, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(widthPort, heightPort), Segment(result, 0, 1));

        Assert.True(operation.TryCrystallize(out var crystal));

        Assert.True(crystal.UnitShape.CompatibleWith(UnitShape.Product(lengthShape, lengthShape)));
        Assert.Equal(0UL, crystal.Value);
        Assert.Equal(0, crystal.Tension);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));
}
