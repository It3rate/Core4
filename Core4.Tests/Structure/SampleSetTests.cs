using Core4.Numerics;
using Core4.Structure;

namespace Core4.Tests.Structure;

public sealed class SampleSetTests
{
    [Fact]
    public void FusedSampleSetPreservesChildOrderAndFlattenedSites()
    {
        Carrier x = 1;
        Carrier y = 2;

        var recessive = SampleSet.Collect(Site.Open(x, new Measure(7)));
        var dominant = SampleSet.Collect(Site.Open(y, new Measure(8)));
        var fused = SampleSet.Fuse(recessive, dominant);

        Assert.True(fused.IsComposite);
        Assert.Equal(new Measure(2), fused.SiteCount);
        Assert.Equal(x, fused.Recessive.Sites[0].Carrier);
        Assert.Equal(y, fused.Dominant.Sites[0].Carrier);
        Assert.Equal(new Measure(7), fused.Measures[0]);
        Assert.Equal(new Measure(8), fused.Measures[1]);
    }

    [Fact]
    public void CarrierSelectionReturnsOnlyMatchingSamplesInOriginalOrder()
    {
        Carrier x = 1;
        Carrier y = 2;

        var samples = SampleSet.Fuse(
            SampleSet.Collect(Site.Open(x, new Measure(3))),
            SampleSet.Fuse(
                SampleSet.Collect(Site.Open(y, new Measure(5))),
                SampleSet.Collect(Site.Open(x, new Measure(7)))));

        var selected = samples.Select(x);

        Assert.Equal(new Measure(2), selected.SiteCount);
        Assert.True(selected.IsComposite);
        Assert.Equal(new Measure(3), selected.Recessive.Measures[0]);
        Assert.Equal(new Measure(7), selected.Dominant.Measures[0]);
    }
}
