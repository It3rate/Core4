using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class CarrierLawTests
{
    [Theory]
    [InlineData(SignLawKind.Positive, 0)]
    [InlineData(SignLawKind.Negative, 1)]
    [InlineData(SignLawKind.BothSigns, 2)]
    [InlineData(SignLawKind.Segment, 3)]
    public void SignLawMarkerUsesUnknownMinusThreeAndKindTick(SignLawKind kind, long kindTick)
    {
        var holder = new Carrier();
        var law = CarrierLaw.SignLaw(holder, kind);

        AssertLawSurface(law, holder, CarrierLaw.SignLawMarkerTicks, kindTick);
    }

    [Theory]
    [InlineData(FlowLawKind.Forward, 0)]
    [InlineData(FlowLawKind.Inverse, 1)]
    [InlineData(FlowLawKind.BothFaces, 2)]
    [InlineData(FlowLawKind.Neither, 3)]
    public void FlowLawMarkerUsesUnknownMinusFourAndKindTick(FlowLawKind kind, long kindTick)
    {
        var holder = new Carrier();
        var law = CarrierLaw.FlowLaw(holder, kind);

        AssertLawSurface(law, holder, CarrierLaw.FlowLawMarkerTicks, kindTick);
    }

    [Fact]
    public void BareCarrierEffectiveLawsAreBothSignsAndForward()
    {
        var holder = new Carrier();

        Assert.False(CarrierLaw.TryReadSignLaw(holder, out _));
        Assert.False(CarrierLaw.TryReadFlowLaw(holder, out _));
        Assert.Equal(SignLawKind.BothSigns, CarrierLaw.ReadEffectiveSignLaw(holder));
        Assert.Equal(FlowLawKind.Forward, CarrierLaw.ReadEffectiveFlowLaw(holder));
    }

    [Theory]
    [InlineData(SignLawKind.Positive)]
    [InlineData(SignLawKind.Negative)]
    [InlineData(SignLawKind.BothSigns)]
    [InlineData(SignLawKind.Segment)]
    public void AttachSignLawRoundTrips(SignLawKind kind)
    {
        var holder = new Carrier();

        CarrierLaw.AttachSignLaw(holder, kind);

        Assert.True(CarrierLaw.TryReadSignLaw(holder, out var read));
        Assert.Equal(kind, read);
        Assert.Equal(kind, CarrierLaw.ReadEffectiveSignLaw(holder));
    }

    [Theory]
    [InlineData(FlowLawKind.Forward)]
    [InlineData(FlowLawKind.Inverse)]
    [InlineData(FlowLawKind.BothFaces)]
    [InlineData(FlowLawKind.Neither)]
    public void AttachFlowLawRoundTrips(FlowLawKind kind)
    {
        var holder = new Carrier();

        CarrierLaw.AttachFlowLaw(holder, kind);

        Assert.True(CarrierLaw.TryReadFlowLaw(holder, out var read));
        Assert.Equal(kind, read);
        Assert.Equal(kind, CarrierLaw.ReadEffectiveFlowLaw(holder));
    }

    [Fact]
    public void AttachSignAndFlowLawsIndependently()
    {
        var holder = new Carrier();

        CarrierLaw.AttachSignLaw(holder, SignLawKind.Segment);
        CarrierLaw.AttachFlowLaw(holder, FlowLawKind.Neither);

        Assert.Equal(SignLawKind.Segment, CarrierLaw.ReadEffectiveSignLaw(holder));
        Assert.Equal(FlowLawKind.Neither, CarrierLaw.ReadEffectiveFlowLaw(holder));
    }

    [Fact]
    public void SegmentBoundsAttachAndReadForSegmentSignLaw()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignLaw(holder, SignLawKind.Segment);
        CarrierLaw.AttachSegmentBounds(holder, -10, 40);

        Assert.True(CarrierLaw.TryReadSegmentBounds(holder, out var anchorA, out var anchorB));
        Assert.Equal(-10, anchorA);
        Assert.Equal(40, anchorB);
    }

    [Fact]
    public void SegmentBoundsDoNotReadWithoutSegmentSignLaw()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignLaw(holder, SignLawKind.BothSigns);
        CarrierLaw.AttachSegmentBounds(holder, 0, 100);

        Assert.False(CarrierLaw.TryReadSegmentBounds(holder, out _, out _));
    }

    [Fact]
    public void SegmentBoundsDoNotReadWhenSegmentLawMissing()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSegmentBounds(holder, 0, 100);

        Assert.False(CarrierLaw.TryReadSegmentBounds(holder, out _, out _));
    }

    [Fact]
    public void LawsOnOneHolderDoNotLeakToAnother()
    {
        var first = new Carrier();
        var second = new Carrier();

        CarrierLaw.AttachSignLaw(first, SignLawKind.Positive);
        CarrierLaw.AttachFlowLaw(first, FlowLawKind.Inverse);

        Assert.Equal(SignLawKind.BothSigns, CarrierLaw.ReadEffectiveSignLaw(second));
        Assert.Equal(FlowLawKind.Forward, CarrierLaw.ReadEffectiveFlowLaw(second));
        Assert.False(CarrierLaw.TryReadSignLaw(second, out _));
        Assert.False(CarrierLaw.TryReadFlowLaw(second, out _));
    }

    [Fact]
    public void AttachReturnsAttachedLawRelations()
    {
        var holder = new Carrier();
        var sign = CarrierLaw.AttachSignLaw(holder, SignLawKind.Negative);
        var flow = CarrierLaw.AttachFlowLaw(holder, FlowLawKind.BothFaces);

        Assert.Same(holder, sign.Right is Relation signKind ? signKind.Right : null);
        Assert.Same(holder, flow.Right is Relation flowKind ? flowKind.Right : null);
        Assert.True(CarrierLaw.TryReadSignLaw(holder, out _));
        Assert.True(CarrierLaw.TryReadFlowLaw(holder, out _));
    }

    [Fact]
    public void InvalidSignLawTickDoesNotMap()
    {
        var holder = new Carrier();
        holder.Attach(new Relation(
            Relation.Point(Carrier.Unknown, CarrierLaw.SignLawMarkerTicks),
            Relation.Point(holder, 9)));

        Assert.False(CarrierLaw.TryReadSignLaw(holder, out _));
        Assert.Equal(SignLawKind.BothSigns, CarrierLaw.ReadEffectiveSignLaw(holder));
    }

    [Fact]
    public void InvalidFlowLawTickDoesNotMap()
    {
        var holder = new Carrier();
        holder.Attach(new Relation(
            Relation.Point(Carrier.Unknown, CarrierLaw.FlowLawMarkerTicks),
            Relation.Point(holder, -2)));

        Assert.False(CarrierLaw.TryReadFlowLaw(holder, out _));
        Assert.Equal(FlowLawKind.Forward, CarrierLaw.ReadEffectiveFlowLaw(holder));
    }

    [Fact]
    public void WrongMarkerTickIsIgnoredForSignLaw()
    {
        var holder = new Carrier();
        holder.Attach(new Relation(
            Relation.Point(Carrier.Unknown, -99),
            Relation.Point(holder, 0)));

        Assert.False(CarrierLaw.TryReadSignLaw(holder, out _));
    }

    [Fact]
    public void SignLawOnHolderDoesNotSatisfyFlowRead()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignLaw(holder, SignLawKind.Positive);

        Assert.False(CarrierLaw.TryReadFlowLaw(holder, out _));
        Assert.Equal(FlowLawKind.Forward, CarrierLaw.ReadEffectiveFlowLaw(holder));
    }

    [Fact]
    public void FlowLawOnHolderDoesNotSatisfySignRead()
    {
        var holder = new Carrier();
        CarrierLaw.AttachFlowLaw(holder, FlowLawKind.Inverse);

        Assert.False(CarrierLaw.TryReadSignLaw(holder, out _));
        Assert.Equal(SignLawKind.BothSigns, CarrierLaw.ReadEffectiveSignLaw(holder));
    }

    [Fact]
    public void TryReadSignLawRejectsNullHolder()
    {
        Assert.Throws<ArgumentNullException>(() => CarrierLaw.TryReadSignLaw(null!, out _));
    }

    [Fact]
    public void TryReadFlowLawRejectsNullHolder()
    {
        Assert.Throws<ArgumentNullException>(() => CarrierLaw.TryReadFlowLaw(null!, out _));
    }

    [Fact]
    public void SignLawFactoryRejectsNullHolder()
    {
        Assert.Throws<ArgumentNullException>(() => CarrierLaw.SignLaw(null!, SignLawKind.Positive));
    }

    [Fact]
    public void FlowLawFactoryRejectsNullHolder()
    {
        Assert.Throws<ArgumentNullException>(() => CarrierLaw.FlowLaw(null!, FlowLawKind.Forward));
    }

    [Fact]
    public void SegmentBoundsFactoryRejectsNullHolder()
    {
        Assert.Throws<ArgumentNullException>(() => CarrierLaw.SegmentBounds(null!, 0, 10));
    }

    [Fact]
    public void FirstAttachedSignLawWinsWhenMultiplePresent()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignLaw(holder, SignLawKind.Positive);
        CarrierLaw.AttachSignLaw(holder, SignLawKind.Negative);

        Assert.True(CarrierLaw.TryReadSignLaw(holder, out var kind));
        Assert.Equal(SignLawKind.Positive, kind);
    }

    [Fact]
    public void FirstAttachedFlowLawWinsWhenMultiplePresent()
    {
        var holder = new Carrier();
        CarrierLaw.AttachFlowLaw(holder, FlowLawKind.Forward);
        CarrierLaw.AttachFlowLaw(holder, FlowLawKind.Inverse);

        Assert.True(CarrierLaw.TryReadFlowLaw(holder, out var kind));
        Assert.Equal(FlowLawKind.Forward, kind);
    }

    [Theory]
    [InlineData(SignLawKind.Positive, FlowLawKind.Forward)]
    [InlineData(SignLawKind.Negative, FlowLawKind.Inverse)]
    [InlineData(SignLawKind.BothSigns, FlowLawKind.BothFaces)]
    [InlineData(SignLawKind.Segment, FlowLawKind.Neither)]
    public void SixteenCellSamplesComposeIndependently(SignLawKind sign, FlowLawKind flow)
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignLaw(holder, sign);
        CarrierLaw.AttachFlowLaw(holder, flow);

        Assert.Equal(sign, CarrierLaw.ReadEffectiveSignLaw(holder));
        Assert.Equal(flow, CarrierLaw.ReadEffectiveFlowLaw(holder));
    }

    private static void AssertLawSurface(Relation law, Carrier holder, long markerTicks, long kindTicks)
    {
        var markerPoint = Assert.IsType<Relation>(law.Left);
        var kindPoint = Assert.IsType<Relation>(law.Right);
        Assert.Equal(new CarrierLocation(Carrier.Unknown, markerTicks), markerPoint.Left);
        Assert.Equal(Carrier.Unknown, markerPoint.Right);
        Assert.Equal(new CarrierLocation(holder, kindTicks), kindPoint.Left);
        Assert.Same(holder, kindPoint.Right);
    }
}
