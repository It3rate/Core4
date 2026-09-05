using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class CarrierLawTests
{
    [Fact]
    public void BareCarrierEffectivePolesDefaultToPnAndForward()
    {
        var holder = new Carrier();

        Assert.False(CarrierLaw.TryReadExplicitSignPoles(holder, out _, out _));
        Assert.False(CarrierLaw.TryReadExplicitFlowPoles(holder, out _, out _));
        Assert.Equal(CarrierLaw.DefaultSignPositivePole, CarrierLaw.ReadEffectiveSignPositive(holder));
        Assert.Equal(CarrierLaw.DefaultSignNegativePole, CarrierLaw.ReadEffectiveSignNegative(holder));
        Assert.Equal(CarrierLaw.DefaultFlowPositivePole, CarrierLaw.ReadEffectiveFlowPositive(holder));
        Assert.Equal(CarrierLaw.DefaultFlowNegativePole, CarrierLaw.ReadEffectiveFlowNegative(holder));
    }

    [Theory]
    [InlineData(1L, 0L)]
    [InlineData(0L, -1L)]
    [InlineData(1L, -1L)]
    [InlineData(0L, 0L)]
    public void SignPoleSurfacesUseMinusThreeAndMinusFour(long positive, long negative)
    {
        var holder = new Carrier();
        var pos = CarrierLaw.SignPositivePole(holder, positive);
        var neg = CarrierLaw.SignNegativePole(holder, negative);

        AssertPoleSurface(pos, holder, CarrierLaw.SignPositiveSurfaceTicks, positive);
        AssertPoleSurface(neg, holder, CarrierLaw.SignNegativeSurfaceTicks, negative);
    }

    [Theory]
    [InlineData(1L, 0L)]
    [InlineData(0L, -1L)]
    [InlineData(1L, -1L)]
    [InlineData(0L, 0L)]
    public void FlowPoleSurfacesUseMinusFiveAndMinusSix(long positive, long negative)
    {
        var holder = new Carrier();
        var pos = CarrierLaw.FlowPositivePole(holder, positive);
        var neg = CarrierLaw.FlowNegativePole(holder, negative);

        AssertPoleSurface(pos, holder, CarrierLaw.FlowPositiveSurfaceTicks, positive);
        AssertPoleSurface(neg, holder, CarrierLaw.FlowNegativeSurfaceTicks, negative);
    }

    [Fact]
    public void AttachSignPolesRoundTrips()
    {
        var holder = new Carrier();
        var (positive, negative) = SignLawPoles.BothSigns;

        CarrierLaw.AttachSignPoles(holder, positive, negative);

        Assert.True(CarrierLaw.TryReadExplicitSignPoles(holder, out var readPositive, out var readNegative));
        Assert.Equal(positive, readPositive);
        Assert.Equal(negative, readNegative);
    }

    [Fact]
    public void AttachFlowPolesRoundTrips()
    {
        var holder = new Carrier();
        var (positive, negative) = FlowLawPoles.Inverse;

        CarrierLaw.AttachFlowPoles(holder, positive, negative);

        Assert.True(CarrierLaw.TryReadExplicitFlowPoles(holder, out var readPositive, out var readNegative));
        Assert.Equal(positive, readPositive);
        Assert.Equal(negative, readNegative);
    }

    [Fact]
    public void SignAndFlowPolesAttachIndependently()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignPoles(holder, SignLawPoles.Segment.Positive, SignLawPoles.Segment.Negative);
        CarrierLaw.AttachFlowPoles(holder, FlowLawPoles.Neither.Positive, FlowLawPoles.Neither.Negative);

        Assert.True(CarrierLaw.TryReadExplicitSignPoles(holder, out var signPositive, out var signNegative));
        Assert.Equal(0, signPositive);
        Assert.Equal(0, signNegative);
        Assert.True(CarrierLaw.TryReadExplicitFlowPoles(holder, out var flowPositive, out var flowNegative));
        Assert.Equal(0, flowPositive);
        Assert.Equal(0, flowNegative);
    }

    [Fact]
    public void SegmentBoundsReadWhenEffectiveSignIsSegment()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignPoles(holder, 0, 0);
        CarrierLaw.AttachSegmentBounds(holder, -10, 40);

        Assert.True(CarrierLaw.IsEffectiveSegmentSign(holder));
        Assert.True(CarrierLaw.TryReadSegmentBounds(holder, out var anchorA, out var anchorB));
        Assert.Equal(-10, anchorA);
        Assert.Equal(40, anchorB);
    }

    [Fact]
    public void SegmentBoundsDoNotReadForPnSign()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignPoles(holder, SignLawPoles.BothSigns.Positive, SignLawPoles.BothSigns.Negative);
        CarrierLaw.AttachSegmentBounds(holder, 0, 100);

        Assert.False(CarrierLaw.TryReadSegmentBounds(holder, out _, out _));
    }

    [Fact]
    public void LawsOnOneHolderDoNotLeakToAnother()
    {
        var first = new Carrier();
        var second = new Carrier();
        CarrierLaw.AttachSignPoles(first, SignLawPoles.Positive, SignLawPoles.PositiveOff);
        CarrierLaw.AttachFlowPoles(first, FlowLawPoles.Inverse.Positive, FlowLawPoles.Inverse.Negative);

        Assert.False(CarrierLaw.TryReadExplicitSignPoles(second, out _, out _));
        Assert.Equal(CarrierLaw.DefaultSignPositivePole, CarrierLaw.ReadEffectiveSignPositive(second));
    }

    [Fact]
    public void InvalidSignPositiveValueDoesNotRead()
    {
        var holder = new Carrier();
        holder.Attach(CarrierLaw.SignPositivePole(holder, 5));

        Assert.False(CarrierLaw.TryReadSignPositivePole(holder, out _));
        Assert.Equal(CarrierLaw.DefaultSignPositivePole, CarrierLaw.ReadEffectiveSignPositive(holder));
    }

    [Fact]
    public void InvalidFlowNegativeValueDoesNotRead()
    {
        var holder = new Carrier();
        holder.Attach(CarrierLaw.FlowNegativePole(holder, -2));

        Assert.False(CarrierLaw.TryReadFlowNegativePole(holder, out _));
        Assert.Equal(CarrierLaw.DefaultFlowNegativePole, CarrierLaw.ReadEffectiveFlowNegative(holder));
    }

    [Fact]
    public void WrongSurfaceTickIsIgnored()
    {
        var holder = new Carrier();
        holder.Attach(new Relation(
            Relation.Point(Carrier.Unknown, -99),
            Relation.Point(holder, 1)));

        Assert.False(CarrierLaw.TryReadSignPositivePole(holder, out _));
    }

    [Fact]
    public void OnlyOneSignPoleAttachedIsNotExplicitPair()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignPositivePole(holder, 1);

        Assert.True(CarrierLaw.TryReadSignPositivePole(holder, out _));
        Assert.False(CarrierLaw.TryReadExplicitSignPoles(holder, out _, out _));
        Assert.Equal(1, CarrierLaw.ReadEffectiveSignPositive(holder));
        Assert.Equal(CarrierLaw.DefaultSignNegativePole, CarrierLaw.ReadEffectiveSignNegative(holder));
    }

    [Fact]
    public void FirstAttachedSignPositiveWinsWhenDuplicated()
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignPositivePole(holder, 1);
        CarrierLaw.AttachSignPositivePole(holder, 0);

        Assert.True(CarrierLaw.TryReadSignPositivePole(holder, out var value));
        Assert.Equal(1, value);
    }

    [Fact]
    public void NullHolderAttachReturnsStructureWithoutThrowing()
    {
        Carrier? holder = null;
        var relation = CarrierLaw.AttachSignPoles(holder, 1, 0);

        Assert.NotNull(relation);
        Assert.False(CarrierLaw.TryReadSignPositivePole(holder, out _));
    }

    [Fact]
    public void UnknownHolderAttachDoesNotThrow()
    {
        var relation = CarrierLaw.AttachFlowPoles(Carrier.Unknown, 1, 0);

        Assert.NotNull(relation);
        Assert.False(CarrierLaw.TryReadFlowPositivePole(Carrier.Unknown, out _));
    }

    [Theory]
    [InlineData(1L, 0L, 1L, 0L)]
    [InlineData(0L, -1L, 0L, -1L)]
    [InlineData(1L, -1L, 1L, -1L)]
    [InlineData(0L, 0L, 0L, 0L)]
    public void NamedPolePairsMatchSixteenCellSamples(
        long signPositive,
        long signNegative,
        long flowPositive,
        long flowNegative)
    {
        var holder = new Carrier();
        CarrierLaw.AttachSignPoles(holder, signPositive, signNegative);
        CarrierLaw.AttachFlowPoles(holder, flowPositive, flowNegative);

        Assert.True(CarrierLaw.TryReadExplicitSignPoles(holder, out var sp, out var sn));
        Assert.True(CarrierLaw.TryReadExplicitFlowPoles(holder, out var fp, out var fn));
        Assert.Equal(signPositive, sp);
        Assert.Equal(signNegative, sn);
        Assert.Equal(flowPositive, fp);
        Assert.Equal(flowNegative, fn);
    }

    [Fact]
    public void SignLawPolesNamedConstantsMatchEncoding()
    {
        Assert.Equal((1, 0), SignLawPoles.P);
        Assert.Equal((0, -1), SignLawPoles.N);
        Assert.Equal((1, -1), SignLawPoles.BothSigns);
        Assert.Equal((0, 0), SignLawPoles.Segment);
    }

    [Fact]
    public void FlowLawPolesNamedConstantsMatchEncoding()
    {
        Assert.Equal((1, 0), FlowLawPoles.Forward);
        Assert.Equal((0, -1), FlowLawPoles.Inverse);
        Assert.Equal((1, -1), FlowLawPoles.BothFaces);
        Assert.Equal((0, 0), FlowLawPoles.Neither);
    }

    private static void AssertPoleSurface(Relation law, Carrier holder, long surfaceTicks, long poleValue)
    {
        var surfacePoint = Assert.IsType<Relation>(law.Left);
        var polePoint = Assert.IsType<Relation>(law.Right);
        Assert.Equal(new CarrierLocation(Carrier.Unknown, surfaceTicks), surfacePoint.Left);
        Assert.Equal(Carrier.Unknown, surfacePoint.Right);
        Assert.Equal(new CarrierLocation(holder, poleValue), polePoint.Left);
        Assert.Same(holder, polePoint.Right);
    }
}
