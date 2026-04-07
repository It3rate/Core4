using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// An ordered readable run on one carrier. It is derived from shared relations on that carrier plus any topology
/// sites whose readable pin resolves onto the same carrier-local nodes. The intended reading is usually:
/// node, relation, node, relation, node. Nodes are the pinned places you encounter while walking the carrier,
/// and relations are the same-carrier spans between adjacent nodes in that run.
/// </summary>
/// <param name="Carrier">The carrier this run is ordered on.</param>
/// <param name="Nodes">The carrier-local pins encountered while walking this connected run in carrier order. Each
/// node may also host attached sites from other carriers that pin onto the same position.</param>
/// <param name="Relations">Only the relations on this same carrier that connect the nodes in this run, ordered by
/// their lower endpoint. This is not the full set of orthogonal relations incident to each attached site.</param>
public readonly record struct CarrierChain(Carrier Carrier, ImmutableArray<CarrierChainNode> Nodes, ImmutableArray<Relation> Relations)
{
    // Nodes are the ordered places you encounter while walking the carrier.
    // Relations are the same-carrier spans between adjacent nodes for this connected run.
    // To inspect extra branch or join structure at a node, read the attached sites through ConnectedSiteTopology.
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
