using Core4.Numerics;
using Core4.Structure;

namespace Core4.Tests.Structure;

public sealed class PlacementProjectionTests
{
    [Fact]
    public void PlacementExposesProjectionAndCompatibilityRatioAlias()
    {
        Carrier x = 1;
        var placement = new Placement(x, new Ratio(3, 10));

        Assert.Equal(new Ratio(3, 10), placement.Projection);
        Assert.Equal(new Ratio(3, 10), placement.Ratio);
        Assert.Equal(new Ratio(3, 10), placement.Resolve(new Ratio(3, 10)).Projection);
    }

    [Fact]
    public void RatioActsAsCompatibilitySupportProjection()
    {
        Carrier x = 1;
        ISupportProjection projection = new Ratio(7, 10);

        var placement = x.Place(projection);
        var located = x.Locate(projection, x);

        Assert.Equal(new Ratio(7, 10), projection.AsRatio());
        Assert.Equal(new Ratio(7, 10), placement.Projection);
        Assert.Equal(new Ratio(7, 10), located);
    }

    [Fact]
    public void PlacementCanResolveFromSupportProjectionInterface()
    {
        Carrier x = 1;
        var placement = Placement.Exact(x, new Measure(5));
        ISupportProjection projection = new Ratio(9, 10);

        var resolved = placement.Resolve(projection);

        Assert.Equal(new Ratio(9, 10), resolved.Projection);
        Assert.Equal(new Ratio(9, 10), resolved.Ratio);
    }
}
