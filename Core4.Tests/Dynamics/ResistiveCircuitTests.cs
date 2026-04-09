using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;
using Core4.Types;

namespace Core4.Tests.Types;

public sealed class ResistiveCircuitTests
{
    [Fact]
    public void ResistiveCircuitGroupsAttachedBranchEndpointsIntoSharedNodes()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var topLeft = new Relation(Site.Open(y, new Measure(0)), Site.Open(y, new Measure(4)));
        var topRight = new Relation(y, Site.Open(y, new Measure(4)), Site.Open(y, new Measure(8)));
        var bottomLeft = new Relation(Site.Open(z, new Measure(0)), Site.Open(z, new Measure(4)));
        var bottomRight = new Relation(z, Site.Open(z, new Measure(4)), Site.Open(z, new Measure(8)));

        var entry = Site.Open(x, new Measure(4))
            .Publish(topLeft, Ratio.Tension(new Measure(4)))
            .Publish(bottomLeft, Ratio.Tension(new Measure(4)));
        var merge = Site.Open(x, new Measure(12))
            .Request(topRight, Ratio.Tension(new Measure(4)))
            .Request(bottomRight, Ratio.Tension(new Measure(4)));

        var topology = ConnectedSiteTopology.Create([entry, merge]);
        var circuit = ResistiveCircuit.Create(
            topology,
            [
                ResistiveBranch.Create(topLeft, new Measure(1)),
                ResistiveBranch.Create(topRight, new Measure(3)),
                ResistiveBranch.Create(bottomLeft, new Measure(2)),
                ResistiveBranch.Create(bottomRight, new Measure(2))
            ],
            [
                ResistiveBoundary.Create(entry, new Measure(12)),
                ResistiveBoundary.Create(merge, Measure.Zero)
            ]);

        Assert.Equal(4, circuit.Nodes.Length);
        Assert.Equal(4, circuit.Branches.Length);
        Assert.Equal(circuit.NodeKeyOf(entry), circuit.NodeKeyOf(topLeft.Origin));
        Assert.Equal(circuit.NodeKeyOf(entry), circuit.NodeKeyOf(bottomLeft.Origin));
        Assert.Equal(circuit.NodeKeyOf(merge), circuit.NodeKeyOf(topRight.Anchor));
        Assert.Equal(circuit.NodeKeyOf(merge), circuit.NodeKeyOf(bottomRight.Anchor));
    }

    [Fact]
    public void ResistiveCircuitSettlesUnevenParallelBranchMidpoints()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var topLeft = new Relation(Site.Open(y, new Measure(0)), Site.Open(y, new Measure(4)));
        var topRight = new Relation(y, Site.Open(y, new Measure(4)), Site.Open(y, new Measure(8)));
        var bottomLeft = new Relation(Site.Open(z, new Measure(0)), Site.Open(z, new Measure(4)));
        var bottomRight = new Relation(z, Site.Open(z, new Measure(4)), Site.Open(z, new Measure(8)));

        var entry = Site.Open(x, new Measure(4))
            .Publish(topLeft, Ratio.Tension(new Measure(4)))
            .Publish(bottomLeft, Ratio.Tension(new Measure(4)));
        var merge = Site.Open(x, new Measure(12))
            .Request(topRight, Ratio.Tension(new Measure(4)))
            .Request(bottomRight, Ratio.Tension(new Measure(4)));

        var topology = ConnectedSiteTopology.Create([entry, merge]);
        var circuit = ResistiveCircuit.Create(
            topology,
            [
                ResistiveBranch.Create(topLeft, new Measure(1)),
                ResistiveBranch.Create(topRight, new Measure(3)),
                ResistiveBranch.Create(bottomLeft, new Measure(2)),
                ResistiveBranch.Create(bottomRight, new Measure(2))
            ],
            [
                ResistiveBoundary.Create(entry, new Measure(12)),
                ResistiveBoundary.Create(merge, Measure.Zero)
            ]);

        var state = circuit.Seed().Settle(new Measure(128));

        Assert.Equal(9d, state.PotentialAt(topLeft.Anchor).Approximate, 6);
        Assert.Equal(6d, state.PotentialAt(bottomLeft.Anchor).Approximate, 6);
        Assert.Equal(3d, state.DropOn(circuit.Branches[0]).Approximate, 6);
        Assert.Equal(3d, state.CurrentOn(circuit.Branches[0]).Approximate, 6);
        Assert.True(state.MaxResidual.Approximate <= 0.000_001d);
    }

    [Fact]
    public void ResistiveCircuitSettlesLeadLoopAndTailWithoutOverflow()
    {
        Carrier x = 1;
        Carrier y = 2;
        Carrier z = 3;

        var source = Site.Open(x, new Measure(0));
        var split = Site.Open(x, new Measure(4));
        var sink = Site.Open(x, new Measure(18));

        var upperEntry = Site.Open(y, new Measure(0));
        var upperMid = Site.Open(y, new Measure(4));
        var upperExit = Site.Open(y, new Measure(8));
        var lowerEntry = Site.Open(z, new Measure(0));
        var lowerMid = Site.Open(z, new Measure(4));
        var lowerExit = Site.Open(z, new Measure(8));

        var topLeft = new Relation(upperEntry, upperMid);
        var topRight = new Relation(y, upperMid, upperExit);
        var bottomLeft = new Relation(lowerEntry, lowerMid);
        var bottomRight = new Relation(z, lowerMid, lowerExit);
        var merge = Site.Open(x, new Measure(12))
            .Request(topRight, Ratio.Tension(new Measure(3)))
            .Request(bottomRight, Ratio.Tension(new Measure(2)));
        split = split
            .Publish(topLeft, Ratio.Tension(new Measure(1)))
            .Publish(bottomLeft, Ratio.Tension(new Measure(2)));

        var sourceLead = new Relation(source, split);
        var tailLead = new Relation(merge, sink);

        var topology = ConnectedSiteTopology.Create([split, merge], sourceLead, tailLead);
        var circuit = ResistiveCircuit.Create(
            topology,
            [
                ResistiveBranch.Create(sourceLead, Measure.One),
                ResistiveBranch.Create(topLeft, Measure.One),
                ResistiveBranch.Create(topRight, new Measure(3)),
                ResistiveBranch.Create(bottomLeft, new Measure(2)),
                ResistiveBranch.Create(bottomRight, new Measure(2)),
                ResistiveBranch.Create(tailLead, Measure.One)
            ],
            [
                ResistiveBoundary.Create(source, new Measure(12)),
                ResistiveBoundary.Create(sink, Measure.Zero)
            ]);

        var state = circuit.Seed().Settle(new Measure(128));

        Assert.True(state.MaxResidual.Approximate <= 0.000_001d);
        Assert.True(state.PotentialAt(upperMid).Approximate > state.PotentialAt(lowerMid).Approximate);

        foreach (var branch in circuit.Branches)
        {
            _ = state.DropOn(branch);
            _ = state.CurrentOn(branch);
            _ = state.DissipationOn(branch);
        }
    }
}
