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
        var incoming = ComposeIncoming(site);

        if (site.Orthogonal.Residual.Potential != Measure.Zero || site.Orthogonal.Residual.Dominant != Measure.Zero)
        {
            incoming = incoming.Potential == Measure.Zero && incoming.Dominant == Measure.Zero
                ? site.Orthogonal.Residual.Normalize()
                : AddOnSupport(incoming, site.Orthogonal.Residual);
        }

        var feedback = ComposeFeedback(site);
        var available = SubtractOnSupport(incoming, feedback);
        var uptake = new Ratio(Measure.One, Measure.One).Subtract(stiffness).Normalize();
        var softened = available.ScaleBy(uptake).Normalize();
        var routed = softened.ScaleBy(routingGain).Normalize();
        var toDominant = routed.ScaleBy(dominantBias).Normalize();
        var toRecessive = SubtractOnSupport(routed, toDominant);
        var nextAnchor = AddOnSupport(anchor, toRecessive);
        var nextClaim = AddOnSupport(claim, toDominant);
        var provisional = AddOnSupport(AddOnSupport(anchor, claim), available);
        var published = AddOnSupport(nextAnchor, nextClaim);
        var settled = published;
        var remainder = CollapseZeroResidual(SubtractOnSupport(available, routed));
        var next = site.Anchor(nextAnchor).Resolve(nextClaim).WithResidual(remainder);

        return new SiteSettlement(site, anchor, incoming, provisional, published, feedback, settled, remainder, next);
    }

    private static Ratio ComposeIncoming(Site site) => ComposeGroups(site.Carrier, site.Orthogonal.LocalIncomingGroups());

    private static Ratio ComposeFeedback(Site site) => ComposeGroups(
        site.Carrier,
        [.. site.Orthogonal.LocalOutgoingGroups(), .. site.Orthogonal.LocalOscillatingGroups()]);

    private static Ratio ComposeGroups(Carrier carrier, IEnumerable<OrthogonalFlowGroup> groups)
    {
        var hasAny = false;
        var total = Ratio.Zero(Measure.Zero);

        foreach (var group in groups)
        {
            // Same-carrier groups already speak the local support language and may balance algebraically.
            // Foreign-carrier groups stay distinct longer by contributing unresolved potential instead of
            // canceling each other away before the site has had any local interpretation step.
            var projected = group.Carrier == carrier
                ? group.Value.Normalize()
                : Ratio.Tension(group.Value.Potential);

            total = hasAny ? AddOnSupport(total, projected) : projected;
            hasAny = true;
        }

        return hasAny ? total.Normalize() : Ratio.Zero(Measure.Zero);
    }

    private static Ratio AddOnSupport(Ratio left, Ratio right)
    {
        var leftForMath = NeutralizeAbsence(left, right);
        var rightForMath = NeutralizeAbsence(right, left);

        if (IsAbsent(leftForMath) && IsAbsent(rightForMath))
        {
            return Ratio.Zero(Measure.Zero);
        }

        return leftForMath.Add(rightForMath).Normalize();
    }

    private static Ratio SubtractOnSupport(Ratio left, Ratio right)
    {
        var leftForMath = NeutralizeAbsence(left, right);
        var rightForMath = NeutralizeAbsence(right, left);

        if (IsAbsent(leftForMath) && IsAbsent(rightForMath))
        {
            return Ratio.Zero(Measure.Zero);
        }

        return leftForMath.Subtract(rightForMath).Normalize();
    }

    // TODO: This neutral-absence lift is the small runtime bridge. The longer-term operator law should let
    // composition slots carry numeric participation/commitment directly, so "no contribution yet", "resolved
    // contribution", and "unresolved tension" remain distinct without ad hoc support-local neutralization.
    private static Ratio NeutralizeAbsence(Ratio candidate, Ratio reference) =>
        IsAbsent(candidate) && reference.IsResolved
            ? Ratio.Zero(reference.Dominant)
            : candidate.Normalize();

    private static Ratio CollapseZeroResidual(Ratio ratio) =>
        ratio.Potential == Measure.Zero
            ? Ratio.Zero(Measure.Zero)
            : ratio.Normalize();

    private static bool IsAbsent(Ratio ratio) => ratio.Potential == Measure.Zero && ratio.Dominant == Measure.Zero;
}
