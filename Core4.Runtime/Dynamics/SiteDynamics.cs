using Core4.Numerics;
using Core4.Structure;

namespace Core4.Dynamics;

/// <summary>
/// Runtime settlement operators for passive site structure. The core site remains an inspectable claim-bearing
/// structure; this operator applies one concrete relaxation law to it.
/// </summary>
public static class SiteDynamics
{
    public static SiteSettlement Settle(Site site)
    {
        var anchor = site.Kernel.Recessive.Normalize();
        var claim = site.Kernel.Dominant.Normalize();
        var stiffness = site.Kernel.Stiffness.Normalize();
        var routingGain = site.Kernel.RoutingGain.Normalize();
        var dominantBias = site.Kernel.DominantBias.Normalize();
        var incoming = site.Orthogonal.LocalIncoming();

        if (site.Orthogonal.Residual.Potential != Measure.Zero || site.Orthogonal.Residual.Dominant != Measure.Zero)
        {
            incoming = incoming.Potential == Measure.Zero && incoming.Dominant == Measure.Zero
                ? site.Orthogonal.Residual.Normalize()
                : incoming.Add(site.Orthogonal.Residual).Normalize();
        }

        var feedback = site.Orthogonal.LocalOutgoing().Add(site.Orthogonal.LocalOscillating()).Normalize();
        var available = incoming.Subtract(feedback).Normalize();
        var uptake = new Ratio(Measure.One, Measure.One).Subtract(stiffness).Normalize();
        var softened = available.ScaleBy(uptake).Normalize();
        var routed = softened.ScaleBy(routingGain).Normalize();
        var toDominant = routed.ScaleBy(dominantBias).Normalize();
        var toRecessive = routed.Subtract(toDominant).Normalize();
        var nextAnchor = anchor.Add(toRecessive).Normalize();
        var nextClaim = claim.Add(toDominant).Normalize();
        var provisional = anchor.Add(claim).Add(available).Normalize();
        var published = nextAnchor.Add(nextClaim).Normalize();
        var settled = published;
        var remainder = available.Subtract(routed).Normalize();
        var next = site.Anchor(nextAnchor).Resolve(nextClaim).WithResidual(remainder);

        return new SiteSettlement(site, anchor, incoming, provisional, published, feedback, settled, remainder, next);
    }
}
