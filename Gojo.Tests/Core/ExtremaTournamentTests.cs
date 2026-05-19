using Gojo.Core;
using Gojo.Structures;

namespace Gojo.Tests.Core;

public sealed class ExtremaTournamentTests
{
    [Fact]
    public void MinimumTournamentSelectsLowestMeasureThroughPairwiseBouts()
    {
        var measure = new Carrier();
        var first = Relation.Point(new Carrier(), 0);
        var second = Relation.Point(new Carrier(), 1);
        var third = Relation.Point(new Carrier(), 2);
        var entries = new[]
        {
            ExtremaTournament.Entry(first, new CarrierLocation(measure, 7)),
            ExtremaTournament.Entry(second, new CarrierLocation(measure, 3)),
            ExtremaTournament.Entry(third, new CarrierLocation(measure, 5))
        };

        var tournament = ExtremaTournament.Minimum(new Carrier(), entries, out var rounds);

        Assert.True(ExtremaTournament.TryReadWinner(tournament, out var winner));
        Assert.True(ExtremaTournament.TryReadEntry(winner, out var value, out var winnerMeasure));
        Assert.Equal(second, value);
        Assert.Equal(new CarrierLocation(measure, 3), winnerMeasure);
        Assert.Equal(2, rounds.Length);

        Assert.True(ExtremaTournament.TryReadRound(rounds[0], out var firstRoundIndex, out var firstRoundBouts));
        Assert.Equal(0, firstRoundIndex.Ticks);
        Assert.Single(firstRoundBouts);
        Assert.True(ExtremaTournament.TryReadBout(
            firstRoundBouts[0],
            out _,
            out var firstWinner,
            out var firstRemainder,
            out var firstMargin));
        Assert.True(ExtremaTournament.TryReadEntry(firstWinner, out var firstWinnerValue, out _));
        Assert.True(ExtremaTournament.TryReadEntry(firstRemainder, out var firstRemainderValue, out _));
        Assert.Equal(second, firstWinnerValue);
        Assert.Equal(first, firstRemainderValue);
        Assert.Equal(new CarrierLocation(measure, 4), firstMargin);
    }

    [Fact]
    public void MaximumTournamentSelectsHighestMeasure()
    {
        var measure = new Carrier();
        var low = Relation.Point(new Carrier(), 0);
        var high = Relation.Point(new Carrier(), 1);
        var entries = new[]
        {
            ExtremaTournament.Entry(low, new CarrierLocation(measure, 2)),
            ExtremaTournament.Entry(high, new CarrierLocation(measure, 9))
        };

        var tournament = ExtremaTournament.Maximum(new Carrier(), entries, out var rounds);

        Assert.True(ExtremaTournament.TryReadWinner(tournament, out var winner));
        Assert.True(ExtremaTournament.TryReadEntry(winner, out var value, out var winnerMeasure));
        Assert.Equal(high, value);
        Assert.Equal(new CarrierLocation(measure, 9), winnerMeasure);
        Assert.Single(rounds);
        Assert.True(ExtremaTournament.TryReadRound(rounds[0], out _, out var bouts));
        Assert.True(ExtremaTournament.TryReadBout(bouts[0], out _, out _, out _, out var margin));
        Assert.Equal(new CarrierLocation(measure, 7), margin);
    }

    [Fact]
    public void TiedTournamentPreservesZeroMarginAndKeepsFirstEntry()
    {
        var measure = new Carrier();
        var first = Relation.Point(new Carrier(), 0);
        var second = Relation.Point(new Carrier(), 1);
        var entries = new[]
        {
            ExtremaTournament.Entry(first, new CarrierLocation(measure, 4)),
            ExtremaTournament.Entry(second, new CarrierLocation(measure, 4))
        };

        var tournament = ExtremaTournament.Minimum(new Carrier(), entries, out var rounds);

        Assert.True(ExtremaTournament.TryReadWinner(tournament, out var winner));
        Assert.True(ExtremaTournament.TryReadEntry(winner, out var value, out _));
        Assert.Equal(first, value);
        Assert.True(ExtremaTournament.TryReadRound(rounds[0], out _, out var bouts));
        Assert.True(ExtremaTournament.TryReadBout(bouts[0], out _, out _, out var remainder, out var margin));
        Assert.True(ExtremaTournament.TryReadEntry(remainder, out var remainderValue, out _));
        Assert.Equal(second, remainderValue);
        Assert.Equal(new CarrierLocation(measure, 0), margin);
    }
}
