using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// One incident support leg at a site. A leg can come from a live relation or an orthogonal flow without changing
/// ontology: both are supports meeting a pinned place with direction and potential.
/// </summary>
public readonly record struct SiteLeg(ISupport Support, Carrier Carrier, Measure Direction, Measure Potential)
{
    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public static SiteLeg FromFlow(Flow flow) => new(flow.Support, flow.Carrier, flow.Direction, flow.Potential);

    public static bool TryFromRelation(Relation relation, Site site, out SiteLeg leg)
    {
        var position = site.On(relation.Carrier).Normalize();
        var origin = relation.OriginPosition.Normalize();
        var anchor = relation.AnchorPosition.Normalize();
        var atOrigin = position == origin;
        var atAnchor = position == anchor;

        if (!atOrigin && !atAnchor)
        {
            leg = default;
            return false;
        }

        var direction = atOrigin == atAnchor
            ? Measure.Zero
            : atOrigin
                ? Measure.One
                : -Measure.One;

        leg = new SiteLeg(relation, relation.Carrier, direction, relation.Potential);
        return true;
    }
}
