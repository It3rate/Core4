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

    [Fact]
    public void UnitShapeSurfaceReadIsSnapshotNotMutableBasis()
    {
        var lengthShape = new Carrier().UnitShape;
        var areaShape = UnitShape.Product(lengthShape, lengthShape);
        var surface = (Relation)areaShape.Surface;

        Assert.True(surface.Perturb(new CarrierLocation(Carrier.Unknown, 10)));

        Assert.Equal(2, areaShape.ProductParticipationOf(lengthShape));
    }

    [Fact]
    public void CrystallizedCarrierKeepsBasisWhenReadSurfaceIsPerturbed()
    {
        var lengthShape = new Carrier().UnitShape;
        var areaShape = UnitShape.Product(lengthShape, lengthShape);
        var crystal = new Carrier(areaShape);
        var surface = (Relation)crystal.UnitShape.Surface;

        Assert.True(surface.Perturb(new CarrierLocation(Carrier.Unknown, 10)));

        Assert.True(crystal.UnitShape.CompatibleWith(areaShape));
        Assert.Equal(2, crystal.UnitShape.ProductParticipationOf(lengthShape));
    }

    [Fact]
    public void CrystallizedCarriersSharingUnitShapeKeepIndependentValues()
    {
        var lengthShape = new Carrier().UnitShape;
        var areaShape = UnitShape.Product(lengthShape, lengthShape);
        var first = new Carrier(areaShape);
        var second = new Carrier(areaShape);

        first.Set(12);
        second.Set(3);
        first.Remove(20);

        Assert.True(first.UnitShape.CompatibleWith(second.UnitShape));
        Assert.False(first.SameIdentityAs(second));
        Assert.Equal(0UL, first.Value);
        Assert.Equal(-8, first.Tension);
        Assert.Equal(3UL, second.Value);
        Assert.Equal(0, second.Tension);
    }

    [Fact]
    public void CrystallizeFromProjectsOperationValueOntoNewCarrier()
    {
        var lengthShape = new Carrier().UnitShape;
        var width = new Carrier(lengthShape);
        var height = new Carrier(lengthShape);
        var result = new Carrier();
        var widthPort = new Relation(Point(width, 1), Point(Carrier.Unknown, 1));
        var heightPort = new Relation(Point(height, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(widthPort, heightPort), Segment(result, 0, 1));
        width.Set(3);
        height.Set(4);

        Assert.True(operation.TryCrystallize(
            new Relation(width.ValueSlot, height.ValueSlot),
            out var crystal,
            out var read));

        Assert.True(crystal.UnitShape.CompatibleWith(UnitShape.Product(lengthShape, lengthShape)));
        Assert.Equal(12UL, crystal.Value);
        Assert.Equal(0, crystal.Tension);
        Assert.Equal(
            new Relation(
                new CarrierLocation(crystal, 12),
                new Relation(new CarrierLocation(width, 0), new CarrierLocation(height, 0))),
            read);
    }

    [Fact]
    public void CrystallizedProjectionPreservesOperationOutputBasis()
    {
        var lengthShape = new Carrier().UnitShape;
        var width = new Carrier(lengthShape);
        var height = new Carrier(lengthShape);
        var result = new Carrier();
        var widthPort = new Relation(Point(width, 1), Point(Carrier.Unknown, 1));
        var heightPort = new Relation(Point(height, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(widthPort, heightPort), Segment(result, 0, 10));
        width.Set(3);
        height.Set(4);

        Assert.True(operation.TryCrystallize(
            new Relation(width.ValueSlot, height.ValueSlot),
            out var crystal,
            out var read));

        Assert.Equal(120UL, crystal.Value);
        Assert.Equal(
            new Relation(
                new CarrierLocation(crystal, 120),
                new Relation(new CarrierLocation(width, 0), new CarrierLocation(height, 0))),
            read);
    }

    [Fact]
    public void CrystallizedProjectionKeepsOperandResidueVisibleWithoutMutatingSources()
    {
        var lengthShape = new Carrier().UnitShape;
        var x = new Carrier(lengthShape);
        var y = new Carrier(lengthShape);
        var result = new Carrier();
        var xPort = new Relation(Segment(x, 0, 10), Point(Carrier.Unknown, 1));
        var yPort = new Relation(Segment(y, 0, 10), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(xPort, yPort), Point(result, 1));
        x.Set(17);
        y.Set(28);

        Assert.True(operation.TryCrystallize(
            new Relation(x.ValueSlot, y.ValueSlot),
            out var crystal,
            out var read));

        Assert.Equal(3UL, crystal.Value);
        Assert.Equal(0, x.Tension);
        Assert.Equal(0, y.Tension);
        Assert.Equal(
            new Relation(
                new CarrierLocation(crystal, 3),
                new Relation(new CarrierLocation(x, 7), new CarrierLocation(y, 8))),
            read);
    }

    [Fact]
    public void ExistingCrystalProjectionUsesDeltaSettlement()
    {
        var lengthShape = new Carrier().UnitShape;
        var width = new Carrier(lengthShape);
        var height = new Carrier(lengthShape);
        var result = new Carrier();
        var widthPort = new Relation(Point(width, 1), Point(Carrier.Unknown, 1));
        var heightPort = new Relation(Point(height, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(widthPort, heightPort), Segment(result, 0, 1));
        Assert.True(operation.TryCrystallize(out var crystal));
        width.Set(3);
        height.Set(4);
        Assert.True(operation.TryProjectCrystallizedValueFrom(
            new Relation(width.ValueSlot, height.ValueSlot),
            crystal,
            out _));
        width.Set(5);

        Assert.True(operation.TryProjectCrystallizedValueFrom(
            new Relation(width.ValueSlot, height.ValueSlot),
            crystal,
            out var read));

        Assert.Equal(20UL, crystal.Value);
        Assert.Equal(
            new Relation(
                new CarrierLocation(crystal, 20),
                new Relation(new CarrierLocation(width, 0), new CarrierLocation(height, 0))),
            read);
    }

    [Fact]
    public void IncompatibleAdditiveCrystalDoesNotProjectAsSingleScalarValue()
    {
        var length = new Carrier();
        var heat = new Carrier();
        var result = new Carrier();
        var lengthPort = new Relation(Point(length, 1), Point(Carrier.Unknown, 1));
        var heatPort = new Relation(Point(heat, 1), Point(Carrier.Unknown, 1));
        var operation = new Relation(new Relation(lengthPort, heatPort), Point(result, 1));
        length.Set(3);
        heat.Set(4);

        Assert.True(operation.TryCrystallize(out var shapeOnlyCrystal));
        Assert.False(operation.TryCrystallize(
            new Relation(length.ValueSlot, heat.ValueSlot),
            out _,
            out _));
        Assert.False(operation.TryProjectCrystallizedValueFrom(
            new Relation(length.ValueSlot, heat.ValueSlot),
            shapeOnlyCrystal,
            out _));
        Assert.Equal(0UL, shapeOnlyCrystal.Value);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));
}
