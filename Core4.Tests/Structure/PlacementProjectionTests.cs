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

        Assert.Equal(new Ratio(3, 10), placement.ProjectionRead.AsRatio());
        Assert.Equal(new Ratio(3, 10), placement.Projection);
        Assert.Equal(new Ratio(3, 10), placement.Ratio);
        Assert.Equal(new Ratio(3, 10), placement.Resolve(new Ratio(3, 10)).ProjectionRead.AsRatio());
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
        Assert.Equal(new Ratio(7, 10), placement.ProjectionRead.AsRatio());
        Assert.Equal(new Ratio(7, 10), placement.Projection);
        Assert.Equal(new Ratio(7, 10), located);
    }

    [Fact]
    public void CustomSupportProjectionCanTravelAcrossSupportContract()
    {
        Carrier x = 1;
        ISupport support = x;
        ISupportProjection projection = new ProxyProjection(new Ratio(new Measure(100), new Measure(107), new Measure(110)));

        var placement = support.Place(projection);
        var located = support.Locate(projection, x);

        Assert.Equal(new Ratio(new Measure(100), new Measure(107), new Measure(110)), placement.ProjectionRead.AsRatio());
        Assert.Equal(new Ratio(new Measure(100), new Measure(107), new Measure(110)), placement.Projection);
        Assert.Equal(new Ratio(new Measure(100), new Measure(107), new Measure(110)), located);
    }

    [Fact]
    public void PlacementCanResolveFromSupportProjectionInterface()
    {
        Carrier x = 1;
        var placement = Placement.Exact(x, new Measure(5));
        ISupportProjection projection = new Ratio(9, 10);

        var resolved = placement.Resolve(projection);

        Assert.Equal(new Ratio(9, 10), resolved.ProjectionRead.AsRatio());
        Assert.Equal(new Ratio(9, 10), resolved.Projection);
        Assert.Equal(new Ratio(9, 10), resolved.Ratio);
    }

    private readonly record struct ProxyProjection(Ratio Value) : ISupportProjection
    {
        public Measure Potential => Value.Potential;

        public Ratio AsRatio() => Value;
    }
}
