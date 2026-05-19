using Gojo.Core;
using Gojo.Reads;
using Gojo.Structures;

namespace Gojo.Tests.Core;

public sealed class OperationFollowTests
{
    [Fact]
    public void FollowRoutesShareOfEqualityResidualIntoFollower()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);

        var follow = OperationFollow.Step(
            new Carrier(),
            new CarrierLocation(value, 10),
            new CarrierLocation(value, 18),
            residual,
            responseNumerator: 1,
            responseDenominator: 2);

        Assert.True(OperationFollow.TryRead(
            follow,
            out var claim,
            out var responseRoute,
            out var applied,
            out var next,
            out var remaining,
            out var routeResidue));
        Assert.Equal(new Relation(Relation.Point(residual, 2), Relation.Point(value, 1)), responseRoute);
        Assert.Equal(new CarrierLocation(value, 4), Assert.IsType<CarrierLocation>(applied));
        Assert.Equal(new CarrierLocation(value, 14), Assert.IsType<CarrierLocation>(next));
        Assert.Equal(new CarrierLocation(residual, 4), Assert.IsType<CarrierLocation>(remaining));
        Assert.Equal(new CarrierLocation(residual, 0), Assert.IsType<CarrierLocation>(routeResidue));
        Assert.True(OperationClaim.TryRead(claim, out _, out var claimResidual, out _, out _, out _, out _));
        Assert.Equal(new CarrierLocation(residual, 8), Assert.IsType<CarrierLocation>(claimResidual));
    }

    [Fact]
    public void FollowKeepsProjectionResidueVisible()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);

        var follow = OperationFollow.Step(
            new Carrier(),
            new CarrierLocation(value, 0),
            new CarrierLocation(value, 5),
            residual,
            responseNumerator: 1,
            responseDenominator: 2);

        Assert.True(OperationFollow.TryRead(
            follow,
            out _,
            out _,
            out var applied,
            out var next,
            out var remaining,
            out var routeResidue));
        Assert.Equal(new CarrierLocation(value, 2), Assert.IsType<CarrierLocation>(applied));
        Assert.Equal(new CarrierLocation(value, 2), Assert.IsType<CarrierLocation>(next));
        Assert.Equal(new CarrierLocation(residual, 3), Assert.IsType<CarrierLocation>(remaining));
        Assert.Equal(new CarrierLocation(residual, 1), Assert.IsType<CarrierLocation>(routeResidue));
    }

    [Fact]
    public void FollowHandlesNegativeResidualWithoutCreatingInverseFlow()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);

        var follow = OperationFollow.Step(
            new Carrier(),
            new CarrierLocation(value, 10),
            new CarrierLocation(value, 3),
            residual,
            responseNumerator: 1,
            responseDenominator: 2);

        Assert.True(OperationFollow.TryRead(
            follow,
            out _,
            out _,
            out var applied,
            out var next,
            out var remaining,
            out var routeResidue));
        var appliedLocation = Assert.IsType<CarrierLocation>(applied);
        Assert.True(appliedLocation.Carrier.SameIdentityAs(value));
        Assert.False(appliedLocation.Carrier.IsInverse);
        Assert.Equal(-3, appliedLocation.Ticks);
        Assert.Equal(new CarrierLocation(value, 7), Assert.IsType<CarrierLocation>(next));
        Assert.Equal(new CarrierLocation(residual, -4), Assert.IsType<CarrierLocation>(remaining));
        Assert.Equal(new CarrierLocation(residual, -1), Assert.IsType<CarrierLocation>(routeResidue));
    }

    [Fact]
    public void FollowLeavesOpposedFlowResidualUnfolded()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);

        var follow = OperationFollow.Step(
            new Carrier(),
            new CarrierLocation(value.Inverse(), 3),
            new CarrierLocation(value, 5),
            residual,
            responseNumerator: 1,
            responseDenominator: 2);

        Assert.True(OperationFollow.TryRead(
            follow,
            out var claim,
            out _,
            out var applied,
            out var next,
            out var remaining,
            out var routeResidue));
        Assert.Equal(new CarrierLocation(value.Inverse(), 0), Assert.IsType<CarrierLocation>(applied));
        Assert.Equal(new CarrierLocation(value.Inverse(), 3), Assert.IsType<CarrierLocation>(next));

        var fusedRemaining = Assert.IsType<Relation>(remaining);
        Assert.True(fusedRemaining.TryReadFusedCarrierFaces(out var inverseFace, out var positiveFace));
        Assert.True(inverseFace.Carrier.SameIdentityAs(residual.Inverse()));
        Assert.Equal(-3, inverseFace.Ticks);
        Assert.True(positiveFace.Carrier.SameIdentityAs(residual));
        Assert.Equal(5, positiveFace.Ticks);
        Assert.Equal(fusedRemaining, routeResidue);
        Assert.True(OperationClaim.TryRead(claim, out _, out var claimResidual, out _, out _, out _, out _));
        Assert.Equal(fusedRemaining, claimResidual);
    }
}
