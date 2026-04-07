using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Tests.Topology;

public sealed class AlignedSiteSpanTests
{
    [Fact]
    public void GeometryOnCarrierRecognizesCenteredInteriorPin()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(5));
        var left = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(5)));
        var right = new Relation(
            Site.Open(x, new Measure(5)),
            Site.Open(x, new Measure(10)));

        var geometry = SiteIncidence.Create(site, left, right).GeometryOn(x);

        Assert.True(geometry.HasGeometry);
        Assert.True(geometry.IsInterior);
        Assert.False(geometry.IsStartEndpoint);
        Assert.False(geometry.IsEndEndpoint);
        Assert.True(geometry.IsCentered);
        Assert.Equal(new Measure(5), geometry.InboundSpan);
        Assert.Equal(new Measure(5), geometry.OutboundSpan);
        Assert.Equal(new Measure(10), geometry.Span);
        Assert.Equal(new Ratio(5, 10), geometry.Position);
        Assert.Equal(new Ratio(0, 10), geometry.Balance);
        Assert.Equal(new Ratio(0, 10), geometry.Asymmetry);
    }

    [Fact]
    public void GeometryOnCarrierRecognizesAsymmetricInteriorPin()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(3));
        var left = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(3)));
        var right = new Relation(
            Site.Open(x, new Measure(3)),
            Site.Open(x, new Measure(10)));

        var geometry = SiteIncidence.Create(site, left, right).GeometryOn(x);

        Assert.True(geometry.HasGeometry);
        Assert.True(geometry.IsInterior);
        Assert.False(geometry.IsCentered);
        Assert.Equal(new Measure(3), geometry.InboundSpan);
        Assert.Equal(new Measure(7), geometry.OutboundSpan);
        Assert.Equal(new Measure(10), geometry.Span);
        Assert.Equal(new Ratio(3, 10), geometry.Position);
        Assert.Equal(new Ratio(4, 10), geometry.Balance);
        Assert.Equal(new Ratio(4, 10), geometry.Asymmetry);
    }

    [Fact]
    public void GeometryOnCarrierRecognizesTerminalStartPin()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(0));
        var relation = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(10)));

        var geometry = SiteIncidence.Create(site, relation).GeometryOn(x);

        Assert.True(geometry.HasGeometry);
        Assert.False(geometry.IsInterior);
        Assert.True(geometry.IsStartEndpoint);
        Assert.False(geometry.IsEndEndpoint);
        Assert.False(geometry.IsCentered);
        Assert.Equal(Measure.Zero, geometry.InboundSpan);
        Assert.Equal(new Measure(10), geometry.OutboundSpan);
        Assert.Equal(new Measure(10), geometry.Span);
        Assert.Equal(new Ratio(0, 10), geometry.Position);
        Assert.Equal(new Ratio(10, 10), geometry.Balance);
        Assert.Equal(new Ratio(10, 10), geometry.Asymmetry);
    }

    [Fact]
    public void GeometryOnCarrierRecognizesTerminalEndPin()
    {
        Carrier x = 1;
        var site = Site.Open(x, new Measure(10));
        var relation = new Relation(
            Site.Open(x, new Measure(0)),
            Site.Open(x, new Measure(10)));

        var geometry = SiteIncidence.Create(site, relation).GeometryOn(x);

        Assert.True(geometry.HasGeometry);
        Assert.False(geometry.IsInterior);
        Assert.False(geometry.IsStartEndpoint);
        Assert.True(geometry.IsEndEndpoint);
        Assert.False(geometry.IsCentered);
        Assert.Equal(new Measure(10), geometry.InboundSpan);
        Assert.Equal(Measure.Zero, geometry.OutboundSpan);
        Assert.Equal(new Measure(10), geometry.Span);
        Assert.Equal(new Ratio(10, 10), geometry.Position);
        Assert.Equal(new Ratio(-10, 10), geometry.Balance);
        Assert.Equal(new Ratio(10, 10), geometry.Asymmetry);
    }

    [Fact]
    public void GeometryOnCarrierRecognizesOrthogonalOutgoingTerminalSpan()
    {
        Carrier x = 1;
        Carrier y = 2;
        var start = Site.Open(y, new Measure(0));
        var end = Site.Open(y, new Measure(4));
        var site = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(start, end),
                Ratio.Tension(new Measure(4)));

        var geometry = SiteIncidence.Create(site).GeometryOn(y);

        Assert.True(geometry.HasGeometry);
        Assert.False(geometry.IsInterior);
        Assert.True(geometry.IsStartEndpoint);
        Assert.False(geometry.IsEndEndpoint);
        Assert.False(geometry.IsCentered);
        Assert.Equal(Measure.Zero, geometry.InboundSpan);
        Assert.Equal(new Measure(4), geometry.OutboundSpan);
        Assert.Equal(new Measure(4), geometry.Span);
        Assert.Equal(new Ratio(0, 4), geometry.Position);
        Assert.Equal(new Ratio(4, 4), geometry.Balance);
        Assert.Equal(new Ratio(4, 4), geometry.Asymmetry);
    }

    [Fact]
    public void GeometryOnCarrierRecognizesOrthogonalCenteredBridgeSpan()
    {
        Carrier x = 1;
        Carrier y = 2;
        var site = Site.Open(x, new Measure(5))
            .Request(
                new Relation(
                    Site.Open(y, new Measure(-4)),
                    Site.Open(y, new Measure(0))),
                Ratio.Tension(new Measure(4)))
            .Publish(
                new Relation(
                    Site.Open(y, new Measure(0)),
                    Site.Open(y, new Measure(4))),
                Ratio.Tension(new Measure(4)));

        var geometry = SiteIncidence.Create(site).GeometryOn(y);

        Assert.True(geometry.HasGeometry);
        Assert.True(geometry.IsInterior);
        Assert.False(geometry.IsStartEndpoint);
        Assert.False(geometry.IsEndEndpoint);
        Assert.True(geometry.IsCentered);
        Assert.Equal(new Measure(4), geometry.InboundSpan);
        Assert.Equal(new Measure(4), geometry.OutboundSpan);
        Assert.Equal(new Measure(8), geometry.Span);
        Assert.Equal(new Ratio(4, 8), geometry.Position);
        Assert.Equal(new Ratio(0, 8), geometry.Balance);
        Assert.Equal(new Ratio(0, 8), geometry.Asymmetry);
    }
}
