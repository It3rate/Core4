using Gojo.Core;
using Gojo.Equations;
using Gojo.Structures;

namespace Gojo.Tests.Equations;

public sealed class EquationResolutionSearchTests
{
    [Fact]
    public void CoarseSearchRefinesTowardPolynomialRoot()
    {
        var x = new Carrier();
        var output = new Carrier();
        var polynomial = EquationPolynomial.Create(new Carrier(), x, output, -9, 0, 1);

        var search = EquationResolutionSearch.Search(
            new Carrier(),
            polynomial,
            x,
            output,
            targetTicks: 0,
            startTicks: -10,
            endTicks: 10,
            candidateCount: 5,
            roundCount: 2,
            out var rounds);

        Assert.True(EquationResolutionSearch.TryReadRounds(search, out var readRounds));
        Assert.Equal(rounds, readRounds);
        Assert.Equal(2, rounds.Length);

        Assert.True(EquationResolutionSearch.TryReadSelectedSample(rounds[0], out var coarseSample));
        Assert.True(EquationResolutionSearch.TryReadCandidateSample(
            coarseSample,
            out _,
            out var coarseInput,
            out _,
            out _,
            out _,
            out var coarseMagnitude,
            out _));
        Assert.Equal(new CarrierLocation(x, 0), coarseInput);
        Assert.Equal(9, coarseMagnitude.Ticks);
        Assert.True(EquationResolutionSearch.TryReadRoundTournament(rounds[0], out var coarseTournament));
        Assert.True(ExtremaTournament.TryReadWinner(coarseTournament, out var coarseWinner));
        Assert.True(ExtremaTournament.TryReadEntry(coarseWinner, out var coarseWinnerSample, out var coarseWinnerMagnitude));
        Assert.Equal(coarseSample, coarseWinnerSample);
        Assert.Equal(coarseMagnitude, coarseWinnerMagnitude);

        Assert.True(EquationResolutionSearch.TryReadSelectedSample(rounds[1], out var refinedSample));
        Assert.True(EquationResolutionSearch.TryReadCandidateSample(
            refinedSample,
            out _,
            out var refinedInput,
            out var refinedOutput,
            out _,
            out _,
            out var refinedMagnitude,
            out _));
        Assert.Equal(new CarrierLocation(x, -3), refinedInput);
        Assert.Equal(new CarrierLocation(output, 0), refinedOutput);
        Assert.Equal(0, refinedMagnitude.Ticks);
    }

    [Fact]
    public void SearchRecordsDirectedResidualFaces()
    {
        var x = new Carrier();
        var output = new Carrier();
        var polynomial = EquationPolynomial.Create(new Carrier(), x, output, -4, 0, 1);

        EquationResolutionSearch.Search(
            new Carrier(),
            polynomial,
            x,
            output,
            targetTicks: 0,
            startTicks: -3,
            endTicks: -1,
            candidateCount: 3,
            roundCount: 1,
            out var rounds);

        Assert.True(EquationResolutionSearch.TryReadRound(
            rounds[0],
            out _,
            out _,
            out _,
            out _,
            out var samples));

        Assert.True(EquationResolutionSearch.TryReadCandidateSample(
            samples[0],
            out _,
            out var leftInput,
            out var leftPrediction,
            out var leftSignedResidual,
            out var leftDirectedResidual,
            out var leftMagnitude,
            out _));
        Assert.Equal(new CarrierLocation(x, -3), leftInput);
        Assert.Equal(new CarrierLocation(output, 5), leftPrediction);
        Assert.Equal(-5, leftSignedResidual.Ticks);
        Assert.True(leftDirectedResidual.Carrier.IsInverse);
        Assert.Equal(5, leftDirectedResidual.Ticks);
        Assert.Equal(5, leftMagnitude.Ticks);

        Assert.True(EquationResolutionSearch.TryReadCandidateSample(
            samples[2],
            out _,
            out var rightInput,
            out var rightPrediction,
            out var rightSignedResidual,
            out var rightDirectedResidual,
            out var rightMagnitude,
            out _));
        Assert.Equal(new CarrierLocation(x, -1), rightInput);
        Assert.Equal(new CarrierLocation(output, -3), rightPrediction);
        Assert.Equal(3, rightSignedResidual.Ticks);
        Assert.False(rightDirectedResidual.Carrier.IsInverse);
        Assert.Equal(3, rightDirectedResidual.Ticks);
        Assert.Equal(3, rightMagnitude.Ticks);
    }

    [Fact]
    public void ResolutionSearchCanProbeAnyEquationSurface()
    {
        var x = new Carrier();
        var output = new Carrier();
        var affine = Equation.Affine(x, 0, 10, output, 0, 100);

        EquationResolutionSearch.Search(
            new Carrier(),
            affine,
            x,
            output,
            targetTicks: 30,
            startTicks: 0,
            endTicks: 10,
            candidateCount: 11,
            roundCount: 1,
            out var rounds);

        Assert.True(EquationResolutionSearch.TryReadSelectedSample(rounds[0], out var selected));
        Assert.True(EquationResolutionSearch.TryReadCandidateSample(
            selected,
            out _,
            out var selectedInput,
            out var predicted,
            out var residual,
            out _,
            out var magnitude,
            out _));
        Assert.Equal(new CarrierLocation(x, 3), selectedInput);
        Assert.Equal(new CarrierLocation(output, 30), predicted);
        Assert.Equal(0, residual.Ticks);
        Assert.Equal(0, magnitude.Ticks);
    }
}
