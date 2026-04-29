using Gojo.Core;
using Gojo.Reads;

namespace Gojo.Tests.Core;

public sealed class StructuralTransformRelationTests
{
    [Fact]
    public void PointIsRelationFromLocationToCarrier()
    {
        var carrier = new Carrier(10);
        var point = Point(carrier, 7);

        Assert.Equal(new CarrierLocation(carrier, 7), point.Left);
        Assert.Equal(carrier, point.Right);
        Assert.Equal(new[] { new CarrierLocation(carrier, 7) }, point.CarrierLocations());
    }

    [Fact]
    public void CarrierKeepsUnroutedIncomingValue()
    {
        var carrier = new Carrier(10);
        var structure = Structure(Point(carrier, 0), Point(Carrier.Unknown, 0));

        var settled = structure.AddAndSettle(new CarrierLocation(carrier, 11));

        Assert.Equal(new CarrierLocation(carrier, 11), settled.PositionOn(carrier));
        Assert.Equal(State(carrier, 11), settled.Left);
    }

    [Fact]
    public void SameCarrierHighToLowRelationAccountsResidueAndTension()
    {
        var carrier = new Carrier(10);
        var boundary = new Relation(Point(carrier, 10), Point(carrier, 0));
        var structure = Structure(Point(carrier, 0), boundary);

        var settled = structure.AddAndSettle(new CarrierLocation(carrier, 23));

        Assert.Equal(new CarrierLocation(carrier, 3), settled.PositionOn(carrier));
        Assert.Equal(new CarrierLocation(carrier, 20), settled.Left.ContributionTensionOn(carrier));
        Assert.Equal(State(carrier, 3, 20), settled.Left);
    }

    [Fact]
    public void SameCarrierEqualSiteRelationClampsAndLeavesExcessTension()
    {
        var carrier = new Carrier(10);
        var clamp = new Relation(Point(carrier, 10), Point(carrier, 10));
        var structure = Structure(Point(carrier, 0), clamp);

        var settled = structure.AddAndSettle(new CarrierLocation(carrier, 23));

        Assert.Equal(new CarrierLocation(carrier, 10), settled.PositionOn(carrier));
        Assert.Equal(new CarrierLocation(carrier, 13), settled.Left.ContributionTensionOn(carrier));
    }

    [Fact]
    public void BoundaryRelationCanRouteCarrierTension()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        var boundary = new Relation(Point(residue, 10), Point(residue, 0));
        var route = new Relation(boundary, Point(whole, 0));
        var structure = Structure(
            Surface(Point(residue, 0), Point(whole, 0)),
            Surface(boundary, route));

        var settled = structure.AddAndSettle(new CarrierLocation(residue, 23));

        Assert.Equal(new CarrierLocation(residue, 3), settled.PositionOn(residue));
        Assert.Equal(new CarrierLocation(whole, 2), settled.PositionOn(whole));
        Assert.Equal(new CarrierLocation(residue, 0), settled.Left.ContributionTensionOn(residue));
    }

    [Fact]
    public void PersistedCarrierTensionCanRouteInLaterFrame()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        var boundary = new Relation(Point(residue, 10), Point(residue, 0));
        var route = new Relation(boundary, Point(whole, 0));
        var unabsorbed = Structure(Point(residue, 0), boundary)
            .AddAndSettle(new CarrierLocation(residue, 23));
        var routed = Structure(
                Surface(unabsorbed.Left, Point(whole, 0)),
                Surface(boundary, route))
            .AddAndSettle(new CarrierLocation(residue, 0));

        Assert.Equal(new CarrierLocation(residue, 3), routed.PositionOn(residue));
        Assert.Equal(new CarrierLocation(whole, 2), routed.PositionOn(whole));
        Assert.Equal(new CarrierLocation(residue, 0), routed.Left.ContributionTensionOn(residue));
    }

    [Fact]
    public void CrossCarrierPointRelationAccountsSourceAndRoutesTension()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        var structure = Structure(
            Surface(Point(residue, 0), Point(whole, 0)),
            new Relation(Point(residue, 10), Point(whole, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(residue, 37));

        Assert.Equal(new CarrierLocation(residue, 7), settled.PositionOn(residue));
        Assert.Equal(new CarrierLocation(whole, 3), settled.PositionOn(whole));
        Assert.Equal(new CarrierLocation(residue, 0), settled.Left.ContributionTensionOn(residue));
    }

    [Fact]
    public void PointRelationsSettleThroughBaseTenCarryChain()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var hundreds = new Carrier(30);
        var structure = Structure(
            Surface(Point(ones, 0), Point(tens, 0), Point(hundreds, 0)),
            Surface(
                new Relation(Point(ones, 10), Point(tens, 0)),
                new Relation(Point(tens, 10), Point(hundreds, 0))));

        var settled = structure.AddAndSettle(new CarrierLocation(ones, 375));

        Assert.Equal(new CarrierLocation(ones, 5), settled.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 7), settled.PositionOn(tens));
        Assert.Equal(new CarrierLocation(hundreds, 3), settled.PositionOn(hundreds));
    }

    [Fact]
    public void UnderflowStaysOnCarrierWithoutRelatedInverseReceiver()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        source.AddUnderflow(23);
        var structure = Structure(
            Surface(source, inverse),
            Point(Carrier.Unknown, 0));

        var settled = structure.AddAndSettle(new CarrierLocation(source, 0));

        Assert.Equal(23UL, source.Underflow);
        Assert.Equal(new CarrierLocation(inverse, 0), settled.PositionOn(inverse));
    }

    [Fact]
    public void UnderflowRoutesThroughRelatedInversePointReceiver()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        source.AddUnderflow(23);
        var structure = Structure(
            Surface(source, inverse),
            new Relation(Point(source, 10), Point(inverse, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(source, 0));

        Assert.Equal(3UL, source.Underflow);
        Assert.Equal(new CarrierLocation(inverse, 2), settled.PositionOn(inverse));
    }

    [Fact]
    public void RelatedInverseCarrierBalancesOpposedVisibleValues()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        source.Add(5);
        var structure = Structure(
            Surface(source, inverse),
            new Relation(Point(source, 1), Point(inverse, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(inverse, 7));

        Assert.Equal(new CarrierLocation(source, 0), settled.PositionOn(source));
        Assert.Equal(new CarrierLocation(inverse, 2), settled.PositionOn(inverse));
    }

    [Fact]
    public void OpposedResidualRemainsFreshForLaterRelationsInSameFrame()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        var target = new Carrier(20);
        source.Add(5);
        var structure = Structure(
            Surface(source, inverse, target),
            Surface(
                new Relation(Point(source, 1), Point(inverse, 0)),
                new Relation(Segment(inverse, 0, 1), Segment(target, 0, 10))));

        var settled = structure.AddAndSettle(new CarrierLocation(inverse, 7));

        Assert.Equal(new CarrierLocation(source, 0), settled.PositionOn(source));
        Assert.Equal(new CarrierLocation(inverse, 2), settled.PositionOn(inverse));
        Assert.Equal(new CarrierLocation(target, 20), settled.PositionOn(target));
    }

    [Fact]
    public void ExtentRelationProjectsFreshParticipationAcrossCarrierSpans()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var structure = Structure(
            Surface(Point(source, 0), Point(target, 0)),
            new Relation(Segment(source, 0, 1), Segment(target, 0, 5)));

        var settled = structure.AddAndSettle(new CarrierLocation(source, 4));

        Assert.Equal(new CarrierLocation(source, 4), settled.PositionOn(source));
        Assert.Equal(new CarrierLocation(target, 20), settled.PositionOn(target));
    }

    [Fact]
    public void SettledExtentProjectionDoesNotFireAgainWithoutFreshSource()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var structure = Structure(
            Surface(Point(source, 0), Point(target, 0)),
            new Relation(Segment(source, 0, 1), Segment(target, 0, 5)));

        var first = structure.AddAndSettle(new CarrierLocation(source, 4));
        var second = first.AddAndSettle(new CarrierLocation(source, 0));

        Assert.Equal(new CarrierLocation(source, 4), second.PositionOn(source));
        Assert.Equal(new CarrierLocation(target, 20), second.PositionOn(target));
        Assert.Equal(first.Left, second.Left);
    }

    [Fact]
    public void ExtentProjectionCanFeedLaterRelationsInSameFrame()
    {
        var source = new Carrier(10);
        var middle = new Carrier(20);
        var target = new Carrier(30);
        var structure = Structure(
            Surface(Point(source, 0), Point(middle, 0), Point(target, 0)),
            Surface(
                new Relation(Segment(source, 0, 1), Segment(middle, 0, 5)),
                new Relation(Segment(middle, 0, 1), Segment(target, 0, 2))));

        var settled = structure.AddAndSettle(new CarrierLocation(source, 4));

        Assert.Equal(new CarrierLocation(source, 4), settled.PositionOn(source));
        Assert.Equal(new CarrierLocation(middle, 20), settled.PositionOn(middle));
        Assert.Equal(new CarrierLocation(target, 40), settled.PositionOn(target));
    }

    [Fact]
    public void ContributionReadsProjectCarrierOwnedState()
    {
        var source = new Carrier(10);
        source.Add(7);
        source.AddOverflow(3);

        Assert.Equal(new CarrierLocation(source, 7), source.ContributionTotalOn(source));
        Assert.Equal(new CarrierLocation(source, 3), source.ContributionTensionOn(source));
        Assert.Equal(
            new[]
            {
                Contribution(Point(source, 0), new CarrierLocation(source, 7))
            },
            source.Contributions());
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, ulong start, ulong end) =>
        new(Point(carrier, start), Point(carrier, end));

    private static Relation Contribution(IElement source, CarrierLocation location, ulong scale = 1) =>
        new(source, new Relation(location, new CarrierLocation(location.Carrier, scale)));

    private static Carrier State(Carrier carrier, ulong value, ulong tension = 0)
    {
        carrier.SetValue(value);
        carrier.SetOverflow(tension);
        carrier.SetUnderflow(0);
        return carrier;
    }

    private static Relation Structure(IElement positions, IElement relations) =>
        new(positions, relations);

    private static IElement Surface(params IElement[] elements)
    {
        return elements.Length switch
        {
            0 => Point(Carrier.Unknown, 0),
            1 => elements[0],
            _ => new Relation(elements[0], Surface(elements[1..]))
        };
    }
}
