using Core4.Numerics;
using Core4.Structure;

namespace Core4.Tests.Structure;

public sealed class AttachmentTests
{
    [Fact]
    public void AtomicAttachmentProjectsLiveSiteThroughFrozenUnit()
    {
        Carrier x = 1;
        var basis = Basis.Freeze(x, new Measure(0), new Measure(10));

        var attachment = basis.Attach(new Measure(7));

        Assert.Equal(new Measure(1), attachment.SiteCount);
        Assert.Equal(new Ratio(7, 10), attachment.Projection);
        Assert.Equal(new Measure(7), attachment.Potential);
        Assert.Equal(new Measure(7), attachment.Measures[0]);
    }

    [Fact]
    public void SegmentAttachmentReportsStartPivotEndAndSpan()
    {
        Carrier x = 1;
        var recessive = Basis.Freeze(x, new Measure(0), new Measure(-10));
        var dominant = Basis.Freeze(x, new Measure(0), new Measure(10));
        var segment = Basis.Fuse(recessive, dominant);

        var attachment = segment.Attach(new Measure(-2), new Measure(5));

        Assert.True(attachment.IsOpposedPair);
        Assert.Equal(new Measure(-2), attachment.SegmentStart());
        Assert.Equal(Measure.Zero, attachment.SegmentPivot());
        Assert.Equal(new Measure(5), attachment.SegmentEnd());
        Assert.Equal(new Measure(7), attachment.SegmentSpanMeasure());
        Assert.Equal(new Ratio(-70, -100), attachment.SegmentSpanProjection());
    }

    [Fact]
    public void OrthogonalAttachmentPlacesResolvedCoefficientInKernelQuadrant()
    {
        Carrier x = 1;
        Carrier y = 2;

        var recessive = Basis.Freeze(x, new Measure(0), new Measure(-10));
        var dominant = Basis.Freeze(y, new Measure(0), new Measure(10));
        var orthogonal = Basis.Fuse(recessive, dominant);

        var attachment = orthogonal.Attach(new Measure(-2), new Measure(4));
        var quadrants = attachment.QuadrantCoefficients();

        Assert.Equal(new Measure(1), attachment.KernelCode);
        Assert.Equal(new Ratio(-8, -100), quadrants[1]);
        Assert.Equal(new Ratio(0, -100), quadrants[0]);
        Assert.Equal(new Ratio(0, -100), quadrants[2]);
        Assert.Equal(new Ratio(0, -100), quadrants[3]);
    }

    [Fact]
    public void SelfPairAttachmentUsesProjectionProductAsCoefficient()
    {
        Carrier x = 1;
        var dominant = Basis.Freeze(x, new Measure(0), new Measure(10));
        var dominantWide = Basis.Freeze(x, new Measure(0), new Measure(20));
        var self = Basis.Fuse(dominant, dominantWide);

        var attachment = self.Attach(new Measure(7), new Measure(14));

        Assert.True(attachment.IsSelfPair);
        Assert.Equal(new Measure(98), attachment.Potential);
        Assert.Equal(new Ratio(98, 200), attachment.ResolvedCoefficient());
    }
}
