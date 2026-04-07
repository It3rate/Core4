using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// An ordered readable run on one carrier. It is derived from shared relations on that carrier plus any topology
/// sites whose readable pin resolves onto the same carrier-local nodes.
/// </summary>
public readonly record struct CarrierChain(Carrier Carrier, ImmutableArray<CarrierChainNode> Nodes, ImmutableArray<Relation> Relations)
{
    public Measure NodeCount => new(Nodes.Length);

    public Measure RelationCount => new(Relations.Length);

    public Measure AttachmentCount
    {
        get
        {
            long total = 0L;

            foreach (var node in Nodes)
            {
                total += node.AttachmentCount.Raw;
            }

            return new Measure(total);
        }
    }

    public Measure TotalSpan
    {
        get
        {
            long total = 0L;

            foreach (var relation in Relations)
            {
                total += relation.Potential.Raw;
            }

            return new Measure(total);
        }
    }

    public CarrierChainNode Start => Nodes.Length != 0 ? Nodes[0] : default;

    public CarrierChainNode End => Nodes.Length != 0 ? Nodes[^1] : default;

    public bool IsSingleton => RelationCount == Measure.Zero;
}
