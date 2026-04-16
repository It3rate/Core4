using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// One incident support leg at a site. A leg can come from a live relation or an orthogonal flow without changing
/// ontology: both are supports meeting a pinned place with direction and potential.
/// </summary>
public readonly record struct SiteLeg(ISupport Support, Measure Direction, Measure Potential)
{
    public Carrier Carrier => Support.PrimaryCarrier;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    // TODO: When stage-3 connected topology lands, derive site-relative direction from the shared topology context
    // instead of caching it inside this local projection.
    public static SiteLeg FromFlow(Flow flow) => new(flow.Support, flow.Direction, flow.Potential);

    public static bool TryFromRelation(Relation relation, Site site, out SiteLeg leg)
    {
        var position = site.On(relation.Carrier).Normalize();
        var origin = relation.OriginPositionRead.AsRatio().Normalize();
        var anchor = relation.AnchorPositionRead.AsRatio().Normalize();
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

        leg = new SiteLeg(relation, direction, relation.Potential);
        return true;
    }
}
