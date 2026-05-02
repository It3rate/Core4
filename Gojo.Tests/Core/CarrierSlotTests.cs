using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class CarrierSlotTests
{
    [Fact]
    public void SetFeedsDifferenceFromCurrentVisibleValue()
    {
        var carrier = new Carrier();
        carrier.Add(4);

        carrier.Set(10);

        Assert.Equal(10UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void SetCanMoveVisibleValueDownByFeedingNegativeTension()
    {
        var carrier = new Carrier();
        carrier.Add(10);

        carrier.Set(4);

        Assert.Equal(4UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void SetBelowOriginLeavesUnaccountedNegativeTension()
    {
        var carrier = new Carrier();
        carrier.Add(4);

        carrier.Set(-3);

        Assert.Equal(0UL, carrier.Value);
        Assert.Equal(-3, carrier.Tension);
    }

    [Fact]
    public void SetSettlesThroughAttachedStructureInsteadOfForcingValue()
    {
        var carrier = new Carrier();
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.Set(23);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(20, carrier.Tension);
    }

    [Fact]
    public void SetChangesVisibleResidueWithoutDiscardingExistingTension()
    {
        var carrier = new Carrier();
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));
        carrier.Add(37);

        carrier.Set(8);

        Assert.Equal(8UL, carrier.Value);
        Assert.Equal(30, carrier.Tension);
    }

    [Fact]
    public void ValueSlotReadsSettledVisibleCarrierValue()
    {
        var carrier = new Carrier();

        carrier.Add(17);

        Assert.Equal(new CarrierLocation(carrier, 17), carrier.ValueSlot);
    }

    [Fact]
    public void ValueSlotReadsSettledResidueWithoutImplicitTensionInspection()
    {
        var carrier = new Carrier();
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.Add(23);

        Assert.Equal(new CarrierLocation(carrier, 3), carrier.ValueSlot);
    }

    [Fact]
    public void TensionSlotDeliberatelyReadsUnresolvedCarrierTension()
    {
        var carrier = new Carrier();
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.Add(23);

        Assert.Equal(new CarrierLocation(carrier, 20), carrier.TensionSlot);
    }

    [Fact]
    public void TensionSlotCanReadBelowOriginParticipation()
    {
        var carrier = new Carrier();

        carrier.Remove(7);

        Assert.Equal(new CarrierLocation(carrier, 0), carrier.ValueSlot);
        Assert.Equal(new CarrierLocation(carrier, -7), carrier.TensionSlot);
    }

    [Fact]
    public void ValueSlotCanBeReadThroughExplicitBasisPort()
    {
        var carrier = new Carrier();
        var unit = new Carrier();
        var outputPort = new Relation(Segment(carrier, 0, 10), Point(unit, 1));

        carrier.Set(27);
        var read = outputPort.Project(carrier.ValueSlot, unit);

        Assert.Equal(
            new Relation(
                new CarrierLocation(unit, 2),
                new CarrierLocation(carrier, 7)),
            read);
    }

    [Fact]
    public void InputPortMapsPortValueBackToCarrierTicks()
    {
        var carrier = new Carrier();
        var unit = new Carrier();
        var inputPort = new Relation(Point(unit, 1), Segment(carrier, 0, 10));

        Assert.True(carrier.TrySetFrom(inputPort, new CarrierLocation(unit, 2)));

        Assert.Equal(new CarrierLocation(carrier, 20), carrier.ValueSlot);
    }

    [Fact]
    public void TickInputPortMapsOnePortUnitToOneCarrierTick()
    {
        var carrier = new Carrier();
        var unit = new Carrier();
        var inputPort = new Relation(Point(unit, 1), Point(carrier, 1));

        Assert.True(carrier.TrySetFrom(inputPort, new CarrierLocation(unit, 17)));

        Assert.Equal(new CarrierLocation(carrier, 17), carrier.ValueSlot);
    }

    [Fact]
    public void InputPortCarriesUnrepresentedResidueAsTension()
    {
        var carrier = new Carrier();
        var unit = new Carrier();
        var inputPort = new Relation(Point(unit, 3), Segment(carrier, 0, 10));

        Assert.True(carrier.TrySetFrom(inputPort, new CarrierLocation(unit, 1)));

        Assert.Equal(new CarrierLocation(carrier, 3), carrier.ValueSlot);
        Assert.Equal(new CarrierLocation(unit, 1), unit.TensionSlot);
    }

    [Fact]
    public void CarrierCanImportAnotherCarrierValueSlotThroughInputPort()
    {
        var source = new Carrier();
        var target = new Carrier();
        var inputPort = new Relation(Point(source, 1), Segment(target, 0, 4));
        source.Set(3);

        Assert.True(target.TrySetFrom(inputPort, source));

        Assert.Equal(new CarrierLocation(source, 3), source.ValueSlot);
        Assert.Equal(new CarrierLocation(target, 12), target.ValueSlot);
    }

    [Fact]
    public void CarrierImportPreservesUnrepresentedSourceResidueAsTension()
    {
        var source = new Carrier();
        var target = new Carrier();
        var inputPort = new Relation(Point(source, 3), Segment(target, 0, 10));
        source.Set(1);

        Assert.True(target.TrySetFrom(inputPort, source));

        Assert.Equal(new CarrierLocation(source, 1), source.ValueSlot);
        Assert.Equal(new CarrierLocation(source, 1), source.TensionSlot);
        Assert.Equal(new CarrierLocation(target, 3), target.ValueSlot);
    }

    [Fact]
    public void MultiSlotPortImportsMatchingRelationShapedValues()
    {
        var sourceX = new Carrier();
        var sourceY = new Carrier();
        var targetX = new Carrier();
        var targetY = new Carrier();
        var sourceSurface = new Relation(Point(sourceX, 1), Point(sourceY, 1));
        var targetSurface = new Relation(Segment(targetX, 0, 10), Segment(targetY, 0, 4));
        var port = new Relation(sourceSurface, targetSurface);
        sourceX.Set(3);
        sourceY.Set(2);

        Assert.True(port.TrySetFrom(new Relation(sourceX.ValueSlot, sourceY.ValueSlot)));

        Assert.Equal(new CarrierLocation(targetX, 30), targetX.ValueSlot);
        Assert.Equal(new CarrierLocation(targetY, 8), targetY.ValueSlot);
    }

    [Fact]
    public void MultiSlotPortPreservesResiduePerImportedSlot()
    {
        var sourceX = new Carrier();
        var sourceY = new Carrier();
        var targetX = new Carrier();
        var targetY = new Carrier();
        var sourceSurface = new Relation(Point(sourceX, 3), Point(sourceY, 1));
        var targetSurface = new Relation(Segment(targetX, 0, 10), Point(targetY, 1));
        var port = new Relation(sourceSurface, targetSurface);
        sourceX.Set(1);
        sourceY.Set(2);

        Assert.True(port.TrySetFrom(new Relation(sourceX.ValueSlot, sourceY.ValueSlot)));

        Assert.Equal(new CarrierLocation(targetX, 3), targetX.ValueSlot);
        Assert.Equal(new CarrierLocation(sourceX, 1), sourceX.TensionSlot);
        Assert.Equal(new CarrierLocation(targetY, 2), targetY.ValueSlot);
        Assert.Equal(new CarrierLocation(sourceY, 0), sourceY.TensionSlot);
    }

    [Fact]
    public void MultiSlotPortRejectsShapeMismatchWithoutPartialCommit()
    {
        var sourceX = new Carrier();
        var sourceY = new Carrier();
        var other = new Carrier();
        var targetX = new Carrier();
        var targetY = new Carrier();
        var sourceSurface = new Relation(Point(sourceX, 1), Point(sourceY, 1));
        var targetSurface = new Relation(Point(targetX, 1), Point(targetY, 1));
        var port = new Relation(sourceSurface, targetSurface);
        sourceX.Set(3);
        other.Set(4);

        Assert.False(port.TrySetFrom(new Relation(sourceX.ValueSlot, other.ValueSlot)));

        Assert.Equal(new CarrierLocation(targetX, 0), targetX.ValueSlot);
        Assert.Equal(new CarrierLocation(targetY, 0), targetY.ValueSlot);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));
}
