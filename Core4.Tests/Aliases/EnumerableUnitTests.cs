using Core4.Aliases;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;
using SettlementPhase = Core4.Dynamics.SiteSettlement.SettlementPhase;

namespace Core4.Tests.Aliases;

public sealed class EnumerableUnitTests
{
    [Fact]
    public void EnumSeedCreatesEnumerableUnitWithOneTickPerSample()
    {
        var unit = new EnumerableUnit<SettlementPhase>(new Carrier(20));

        Assert.Equal(new Measure(8), unit.TickCount);
        Assert.Equal(new Measure(7), unit.LastTick);
        Assert.Equal(new Ratio(0, 8), unit.PositionOf(SettlementPhase.None));
        Assert.Equal(new Ratio(6, 8), unit.PositionOf(SettlementPhase.Settled));
        Assert.Equal("Provisional", unit.NameOf(SettlementPhase.Provisional));
        Assert.Equal(nameof(SettlementPhase), unit.Name);
    }

    [Fact]
    public void DefaultIterationWalksOneTickAtATimeAcrossValidSamples()
    {
        var unit = new EnumerableUnit<SettlementPhase>(new Carrier(20));
        var positions = unit.IteratePositions();

        Assert.Equal(8, positions.Length);
        Assert.Equal(new Ratio(0, 8), positions[0]);
        Assert.Equal(new Ratio(7, 8), positions[^1]);
    }

    [Fact]
    public void IterationCanUseCustomTickStep()
    {
        var unit = new EnumerableUnit<SettlementPhase>(new Carrier(20));
        var positions = unit.IteratePositions(stepTicks: new Measure(2));

        Assert.Equal(4, positions.Length);
        Assert.Equal(new Ratio(0, 8), positions[0]);
        Assert.Equal(new Ratio(2, 8), positions[1]);
        Assert.Equal(new Ratio(6, 8), positions[^1]);
    }

    [Fact]
    public void PhaseRangeFreezesAsOrdinaryCore4Relation()
    {
        var unit = new EnumerableUnit<SettlementPhase>(new Carrier(20));
        var relation = unit.Relate(SettlementPhase.Provisional, SettlementPhase.Settled);
        var basis = unit.Freeze(SettlementPhase.Provisional, SettlementPhase.Settled);

        Assert.Equal(new Ratio(3, 1), relation.Span);
        Assert.Equal(new Measure(3), relation.Potential);
        Assert.Equal(new Measure(3), basis.Potential);
        Assert.Equal(new Ratio(3, 0), basis.Ratio);
    }

    [Fact]
    public void EnumerableUnitTracksCurrentIndexAcrossEnumeration()
    {
        var unit = new EnumerableUnit<SettlementPhase>(new Carrier(20));

        Assert.False(unit.HasCurrent);

        Assert.True(unit.MoveNext());
        Assert.Equal(Measure.Zero, unit.CurrentIndex);
        Assert.Equal(SettlementPhase.None, unit.Current);

        Assert.True(unit.MoveTo(SettlementPhase.Settled));
        Assert.Equal(new Measure(6), unit.CurrentIndex);
        Assert.Equal(new Ratio(6, 8), unit.PositionAt(unit.CurrentIndex));

        Assert.True(unit.MoveNext());
        Assert.Equal(new Measure(7), unit.CurrentIndex);
        Assert.Equal(SettlementPhase.Remainder, unit.Current);

        Assert.False(unit.MoveNext());
        Assert.False(unit.HasCurrent);
        Assert.Equal(new Measure(8), unit.CurrentIndex);
    }

    [Fact]
    public void EnumerableUnitObservationUsesSharedTraversalPreference()
    {
        var unit = new EnumerableUnit<SettlementPhase>(new Carrier(20));

        Assert.True(unit.TryObserve(new Measure(3), out var observation));
        Assert.Equal(2, observation.ForwardContinuations.Length);
        Assert.True(observation.IsDecisionPoint);
        Assert.True(observation.TryPreferred(out var preferred));
        Assert.True(preferred.IsOutgoing);
        Assert.Equal(unit.Carrier, preferred.Carrier);
        Assert.Equal(new Ratio(4, 8), preferred.Target.Position);
    }
}
