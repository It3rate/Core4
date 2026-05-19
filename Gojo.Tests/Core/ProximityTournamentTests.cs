using Gojo.Core;
using Gojo.Structures;

namespace Gojo.Tests.Core;

public sealed class ProximityTournamentTests
{
    [Fact]
    public void NearestSelectsSmallestEqualityClaimResidual()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);
        var measure = new Carrier(value.UnitShape);
        IElement[] candidates =
        [
            new CarrierLocation(value, 1),
            new CarrierLocation(value, 7),
            new CarrierLocation(value, 4)
        ];

        var proximity = ProximityTournament.Nearest(
            new Carrier(),
            candidates,
            new CarrierLocation(value, 5),
            residual,
            measure,
            out var choices,
            out var rounds);

        Assert.True(ProximityTournament.TryReadChoices(proximity, out var readChoices));
        Assert.Equal(choices, readChoices);
        Assert.Equal(2, rounds.Length);
        Assert.True(ProximityTournament.TryReadWinner(proximity, out var winner));
        Assert.True(ProximityTournament.TryReadChoice(
            winner,
            out var winnerValue,
            out _,
            out var equalityClaim,
            out _,
            out var distance,
            out var boundaryViolation,
            out var selectionMeasure));
        Assert.Equal(new CarrierLocation(value, 4), Assert.IsType<CarrierLocation>(winnerValue));
        Assert.Equal(new CarrierLocation(residual, 1), distance);
        Assert.Equal(new CarrierLocation(residual, 0), boundaryViolation);
        Assert.Equal(new CarrierLocation(measure, 1), selectionMeasure);
        Assert.True(OperationClaim.TryRead(equalityClaim, out _, out var residualRead, out _, out _, out _, out _));
        Assert.Equal(new CarrierLocation(residual, 1), Assert.IsType<CarrierLocation>(residualRead));
    }

    [Fact]
    public void NearestAtLeastSelectsClosestCandidateAboveTarget()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);
        var measure = new Carrier(value.UnitShape);
        IElement[] candidates =
        [
            new CarrierLocation(value, 4),
            new CarrierLocation(value, 6),
            new CarrierLocation(value, 9)
        ];

        var proximity = ProximityTournament.NearestAtLeast(
            new Carrier(),
            candidates,
            new CarrierLocation(value, 5),
            residual,
            measure,
            out var choices,
            out _);

        Assert.True(ProximityTournament.TryReadWinner(proximity, out var winner));
        Assert.True(ProximityTournament.TryReadChoice(
            winner,
            out var winnerValue,
            out _,
            out _,
            out var winnerBoundary,
            out var winnerDistance,
            out var winnerBoundaryViolation,
            out _));
        Assert.Equal(new CarrierLocation(value, 6), Assert.IsType<CarrierLocation>(winnerValue));
        Assert.Equal(new CarrierLocation(residual, 1), winnerDistance);
        Assert.Equal(new CarrierLocation(residual, 0), winnerBoundaryViolation);
        var winnerBoundaryClaim = Assert.IsType<Relation>(winnerBoundary);
        Assert.True(OperationClaim.TryRead(winnerBoundaryClaim, out _, out _, out _, out _, out var winnerViolationMagnitude, out _));
        Assert.Equal(new CarrierLocation(residual, 0), winnerViolationMagnitude);

        Assert.True(ProximityTournament.TryReadChoice(
            choices[0],
            out _,
            out _,
            out _,
            out var belowBoundary,
            out var belowDistance,
            out var belowBoundaryViolation,
            out var belowSelection));
        Assert.Equal(new CarrierLocation(residual, 1), belowDistance);
        Assert.Equal(new CarrierLocation(residual, 1), belowBoundaryViolation);
        Assert.Equal(6, belowSelection.Ticks);
        var belowBoundaryClaim = Assert.IsType<Relation>(belowBoundary);
        Assert.True(OperationClaim.TryRead(belowBoundaryClaim, out _, out _, out _, out _, out var belowViolationMagnitude, out _));
        Assert.Equal(new CarrierLocation(residual, 1), belowViolationMagnitude);
    }

    [Fact]
    public void NearestAtMostSelectsClosestCandidateBelowTarget()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);
        var measure = new Carrier(value.UnitShape);
        IElement[] candidates =
        [
            new CarrierLocation(value, 2),
            new CarrierLocation(value, 4),
            new CarrierLocation(value, 6)
        ];

        var proximity = ProximityTournament.NearestAtMost(
            new Carrier(),
            candidates,
            new CarrierLocation(value, 5),
            residual,
            measure,
            out var choices,
            out _);

        Assert.True(ProximityTournament.TryReadWinner(proximity, out var winner));
        Assert.True(ProximityTournament.TryReadChoice(
            winner,
            out var winnerValue,
            out _,
            out _,
            out _,
            out var winnerDistance,
            out var winnerBoundaryViolation,
            out _));
        Assert.Equal(new CarrierLocation(value, 4), Assert.IsType<CarrierLocation>(winnerValue));
        Assert.Equal(new CarrierLocation(residual, 1), winnerDistance);
        Assert.Equal(new CarrierLocation(residual, 0), winnerBoundaryViolation);

        Assert.True(ProximityTournament.TryReadChoice(
            choices[2],
            out _,
            out _,
            out _,
            out _,
            out _,
            out var aboveBoundaryViolation,
            out var aboveSelection));
        Assert.Equal(new CarrierLocation(residual, 1), aboveBoundaryViolation);
        Assert.Equal(5, aboveSelection.Ticks);
    }

    [Fact]
    public void DirectionalNearestFallsBackToSmallestBoundaryViolationWhenNoCandidateSatisfiesBoundary()
    {
        var value = new Carrier();
        var residual = new Carrier(value.UnitShape);
        var measure = new Carrier(value.UnitShape);
        IElement[] candidates =
        [
            new CarrierLocation(value, 1),
            new CarrierLocation(value, 3)
        ];

        var proximity = ProximityTournament.NearestAtLeast(
            new Carrier(),
            candidates,
            new CarrierLocation(value, 5),
            residual,
            measure,
            out _,
            out _);

        Assert.True(ProximityTournament.TryReadWinner(proximity, out var winner));
        Assert.True(ProximityTournament.TryReadChoice(
            winner,
            out var winnerValue,
            out _,
            out _,
            out _,
            out var distance,
            out var boundaryViolation,
            out var selectionMeasure));
        Assert.Equal(new CarrierLocation(value, 3), Assert.IsType<CarrierLocation>(winnerValue));
        Assert.Equal(new CarrierLocation(residual, 2), distance);
        Assert.Equal(new CarrierLocation(residual, 2), boundaryViolation);
        Assert.Equal(new CarrierLocation(measure, 12), selectionMeasure);
    }
}
