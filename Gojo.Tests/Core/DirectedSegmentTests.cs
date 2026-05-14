using Gojo.Core;
using Gojo.Reads;

namespace Gojo.Tests.Core;

public sealed class DirectedSegmentTests
{
    [Fact]
    public void DirectedSegmentUsesOpposedStartAndPositiveEndpoint()
    {
        var axis = new Carrier();

        var segment = Relation.DirectedSegment(axis, inverseStart: 2, positiveEnd: 3);

        Assert.True(segment.TryReadDirectedSegment(out var start, out var end));
        Assert.True(start.Carrier.SameIdentityAs(axis.Inverse()));
        Assert.Equal(2, start.Ticks);
        Assert.True(end.Carrier.SameIdentityAs(axis));
        Assert.Equal(3, end.Ticks);
    }

    [Fact]
    public void DirectedSegmentProductDistributesEndpointParticipation()
    {
        var lengthShape = new Carrier().UnitShape;
        var x = new Carrier(lengthShape);
        var y = new Carrier(lengthShape);
        var area = new Carrier(UnitShape.Product(lengthShape, lengthShape));
        var xSegment = Relation.DirectedSegment(x, inverseStart: 2, positiveEnd: 3);
        var ySegment = Relation.DirectedSegment(y, inverseStart: 4, positiveEnd: 5);

        var product = DirectedSegmentReads.DirectedSegmentProduct(xSegment, ySegment, area, out var terms);

        Assert.True(product.TryReadOrderedElementsOn((Carrier)product.Right, out var elements));
        Assert.Equal(terms, elements);
        AssertTerm(terms[0], area.Inverse(), 8);
        AssertTerm(terms[1], area.Inverse(), 10);
        AssertTerm(terms[2], area.Inverse(), 12);
        AssertTerm(terms[3], area, 15);

        static void AssertTerm(Relation term, Carrier expectedCarrier, long expectedTicks)
        {
            var target = Assert.IsType<CarrierLocation>(term.Left);
            Assert.True(target.Carrier.SameIdentityAs(expectedCarrier));
            Assert.Equal(expectedTicks, target.Ticks);
            Assert.IsType<Relation>(term.Right);
        }
    }
}
