using Core4.Numerics;
using Core4.Structure;

namespace Core4.Tests.Structure;

public sealed class CarrierTests
{
    [Fact]
    public void SingleArgumentCarrierUsesItsOwnIdAsDefaultDomain()
    {
        Carrier carrier = 7;

        Assert.Equal(new CarrierDomain(7), carrier.Domain);
        Assert.Equal(new Carrier(7, new CarrierDomain(7)), carrier);
    }

    [Fact]
    public void DistinctCarrierInstancesCanShareOneDomain()
    {
        CarrierDomain temperature = 42;
        var celsius = new Carrier(1, temperature);
        var fahrenheit = new Carrier(2, temperature);

        Assert.Equal(temperature, celsius.Domain);
        Assert.Equal(temperature, fahrenheit.Domain);
        Assert.NotEqual(celsius, fahrenheit);
    }

    [Fact]
    public void RelationAndBasisPreserveCarrierDomainMetadata()
    {
        CarrierDomain temperature = 42;
        var celsius = new Carrier(1, temperature);
        var relation = new Relation(
            Site.Open(celsius, new Measure(0)),
            Site.Open(celsius, new Measure(100)));
        var basis = relation.Freeze();

        Assert.Equal(temperature, relation.Carrier.Domain);
        Assert.Equal(temperature, basis.PrimaryCarrier.Domain);
        Assert.Single(basis.SelectCarriers());
        Assert.Equal(temperature, basis.SelectCarriers()[0].Domain);
    }
}
