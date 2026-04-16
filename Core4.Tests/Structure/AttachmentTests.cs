
namespace Core4.Tests.Structure;

public sealed class AttachmentTests
{
    [Fact]
    public void BasisAndAttachmentCanBeConsumedThroughSharedSupportFoldRead()
    {
        Carrier x = 1;
        var basis = Basis.Freeze(x, new Measure(0), new Measure(10));
        var attachment = basis.Attach(new Measure(7));

        ISupportFold frozen = basis;
        ISupportFold live = attachment;

        Assert.Equal(new Ratio(10, 0), frozen.FoldRead.AsRatio());
        Assert.Equal(new Ratio(10, 0), frozen.Fold);
        Assert.Equal(new Measure(10), frozen.Potential);
        Assert.Equal(new Ratio(7, 10), live.FoldRead.AsRatio());
        Assert.Equal(new Ratio(7, 10), live.Fold);
        Assert.Equal(new Measure(7), live.Potential);
    }

    [Fact]
    public void AtomicAttachmentProjectsLiveSiteThroughFrozenUnit()
    {
        Carrier x = 1;
        var basis = Basis.Freeze(x, new Measure(0), new Measure(10));

        var attachment = basis.Attach(new Measure(7));

        Assert.Equal(new Measure(1), attachment.SiteCount);
        Assert.True(attachment.TryGetProjectionRead(out var projectionRead));
        Assert.True(attachment.TryGetProjection(out var projection));
        Assert.Equal(new Ratio(7, 10), projectionRead.AsRatio());
        Assert.Equal(new Ratio(7, 10), projection);
        Assert.Equal(new Ratio(7, 10), attachment.ProjectionRead.AsRatio());
        Assert.Equal(new Ratio(7, 10), attachment.Projection);
        Assert.Equal(new Ratio(7, 10), attachment.FoldRead.AsRatio());
        Assert.Equal(new Ratio(7, 10), attachment.Fold);
        Assert.Equal(attachment.Fold, attachment.Ratio);
        Assert.Equal(new Measure(7), attachment.Potential);
        Assert.Equal(new Measure(1), attachment.Samples.SiteCount);
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
        Assert.False(attachment.TryGetProjectionRead(out var projectionRead));
        Assert.False(attachment.TryGetProjection(out var projection));
        Assert.Equal(Ratio.Tension(new Measure(7)), projectionRead.AsRatio());
        Assert.Equal(Ratio.Tension(new Measure(7)), projection);
        Assert.Equal(new Ratio(2, 5), attachment.FoldRead.AsRatio());
        Assert.Equal(new Ratio(2, 5), attachment.Fold);
        Assert.Equal(attachment.Fold, attachment.Ratio);
        Assert.Equal(new Ratio(-2, -10), attachment.ComponentProjectionReads()[0].AsRatio());
        Assert.Equal(new Ratio(5, 10), attachment.ComponentProjectionReads()[1].AsRatio());
        Assert.Equal(new Ratio(-2, -10), attachment.ComponentProjections()[0]);
        Assert.Equal(new Ratio(5, 10), attachment.ComponentProjections()[1]);
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

    [Fact]
    public void CompositeAttachmentFillsMissingSitesFromExactSupportOrigins()
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Basis.Freeze(x, new Measure(0), new Measure(10));
        var right = Basis.Freeze(
            Site.Open(y.At(new Ratio(3, 2))),
            Site.Open(y.At(new Ratio(7, 2))));
        var fused = Basis.Fuse(left, right);

        var attachment = fused.Attach(Site.Open(x, new Measure(7)));

        Assert.Equal(new Measure(2), attachment.SiteCount);
        Assert.Equal(y, attachment.Dominant.Sites[0].Carrier);
        Assert.Equal(new Ratio(3, 2), attachment.Dominant.Sites[0].Position.Normalize());
        Assert.Equal(Measure.Zero, attachment.Dominant.Potential);
        Assert.Equal(Measure.Zero, attachment.ComponentProjections()[1].Potential);
    }

    [Fact]
    public void AttachmentCanBindSupportIndependentSampleSet()
    {
        Carrier x = 1;
        Carrier y = 2;

        var basis = Basis.Fuse(
            Basis.Freeze(x, new Measure(0), new Measure(10)),
            Basis.Freeze(y, new Measure(0), new Measure(20)));
        var samples = SampleSet.Fuse(
            SampleSet.Collect(Site.Open(x, new Measure(7))),
            SampleSet.Collect(Site.Open(y, new Measure(8))));

        var attachment = basis.Attach(samples);

        Assert.Equal(samples.SiteCount, attachment.Samples.SiteCount);
        Assert.Equal(new Measure(2), attachment.SiteCount);
        Assert.Equal(x, attachment.Recessive.Sites[0].Carrier);
        Assert.Equal(y, attachment.Dominant.Sites[0].Carrier);
        Assert.Equal(new Ratio(7, 10), attachment.ComponentProjectionReads()[0].AsRatio());
        Assert.Equal(new Ratio(8, 20), attachment.ComponentProjectionReads()[1].AsRatio());
    }

    [Fact]
    public void AttachmentPreservesCompositeSampleGroupingWhenShapeMatchesBasis()
    {
        Carrier x = 1;
        Carrier y = 2;

        var basis = Basis.Fuse(
            Basis.Freeze(x, new Measure(0), new Measure(10)),
            Basis.Freeze(y, new Measure(0), new Measure(20)));
        var samples = SampleSet.Fuse(
            SampleSet.Collect(Site.Open(x, new Measure(7))),
            SampleSet.Collect(Site.Open(y, new Measure(8))));

        var attachment = basis.Attach(samples);

        Assert.True(attachment.Samples.IsComposite);
        Assert.Equal(2, attachment.Samples.Inputs.Length);
        Assert.Equal(new Measure(1), attachment.Recessive.Samples.SiteCount);
        Assert.Equal(new Measure(1), attachment.Dominant.Samples.SiteCount);
        Assert.Equal(new Measure(7), attachment.Recessive.Samples.Measures[0]);
        Assert.Equal(new Measure(8), attachment.Dominant.Samples.Measures[0]);
    }

    [Fact]
    public void AttachmentRebuildsCompositeSampleGroupingFromFlatSamplesWhenNeeded()
    {
        Carrier x = 1;
        Carrier y = 2;

        var basis = Basis.Fuse(
            Basis.Freeze(x, new Measure(0), new Measure(10)),
            Basis.Freeze(y, new Measure(0), new Measure(20)));
        var flatSamples = SampleSet.Collect(
            Site.Open(x, new Measure(7)),
            Site.Open(y, new Measure(8)));

        var attachment = basis.Attach(flatSamples);

        Assert.True(attachment.Samples.IsComposite);
        Assert.Equal(2, attachment.Samples.Inputs.Length);
        Assert.Equal(new Measure(7), attachment.Recessive.Samples.Measures[0]);
        Assert.Equal(new Measure(8), attachment.Dominant.Samples.Measures[0]);
    }

    [Fact]
    public void AttachmentCanSelectCarrierLocalSampleSubset()
    {
        Carrier x = 1;
        Carrier y = 2;

        var basis = Basis.Fuse(
            Basis.Freeze(x, new Measure(0), new Measure(10)),
            Basis.Freeze(y, new Measure(0), new Measure(20)),
            Basis.Freeze(x, new Measure(0), new Measure(30)));
        var attachment = basis.Attach(
            SampleSet.Fuse(
                SampleSet.Collect(Site.Open(x, new Measure(7))),
                SampleSet.Collect(Site.Open(y, new Measure(8))),
                SampleSet.Collect(Site.Open(x, new Measure(9)))));

        var xSamples = attachment.SelectSamples(x);

        Assert.Equal(new Measure(2), xSamples.SiteCount);
        Assert.True(xSamples.IsComposite);
        Assert.Equal(new Measure(7), xSamples.Recessive.Measures[0]);
        Assert.Equal(new Measure(9), xSamples.Dominant.Measures[0]);
    }
}
