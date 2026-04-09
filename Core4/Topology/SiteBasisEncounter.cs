using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// One basis-like calibration law encounterable at a pinned site. This remains descriptive for now: it surfaces the
/// local one-sided or bilateral calibration already implied by incident relations without yet deciding traversal
/// policy from it.
/// </summary>
public readonly record struct SiteBasisEncounter(Site Site, Carrier Carrier, Basis Basis, ISupport SourceSupport)
{
    public Measure Grade => Basis.Grade;

    public Measure RelationCount => Basis.RelationCount;

    public Ratio Calibration => Basis.Ratio.Normalize();

    public Measure Potential => Basis.Potential;

    public bool IsAtomic => Basis.IsAtomic;

    public bool IsComposite => Basis.IsComposite;

    public bool IsExplicitPinnedBasis => SourceSupport is Basis;

    public bool IsRelationDerived => SourceSupport is Relation;

    public bool IsLocalToSite => HasSharedOriginAtSite();

    public bool HasRecessiveCalibration => IsAtomic
        ? Basis.IsRecessive
        : Basis.IsAtomicPair && Basis.Recessive.IsAtomic && Basis.Recessive.IsRecessive;

    public bool HasDominantCalibration => IsAtomic
        ? Basis.IsDominant
        : Basis.IsAtomicPair && Basis.Dominant.IsAtomic && Basis.Dominant.IsDominant;

    public bool IsOneSided => HasRecessiveCalibration != HasDominantCalibration;

    public bool IsBilateral => Basis.IsAtomicPair
        && Basis.IsSameCarrierPair()
        && Basis.IsOpposedPair()
        && IsLocalToSite
        && HasRecessiveCalibration
        && HasDominantCalibration;

    public bool SupportsDirection(Measure direction) => direction.Raw switch
    {
        < 0L => HasRecessiveCalibration,
        > 0L => HasDominantCalibration,
        _ => false
    };

    public ImmutableArray<Relation> LocalRelations => Basis.SelectRelations();

    internal static SiteBasisEncounter FromIncoming(Site site, Relation relation) =>
        new(site, relation.Carrier, Basis.Freeze(relation.Flip()), relation);

    internal static SiteBasisEncounter FromOutgoing(Site site, Relation relation) =>
        new(site, relation.Carrier, Basis.Freeze(relation), relation);

    internal static SiteBasisEncounter FromPassThrough(Site site, Relation incoming, Relation outgoing) =>
        new(
            site,
            outgoing.Carrier,
            Basis.Fuse(
                Basis.Freeze(incoming.Flip()),
                Basis.Freeze(outgoing)),
            outgoing);

    internal static SiteBasisEncounter FromPinnedBasis(Site site, Carrier carrier, Basis basis) =>
        new(site, carrier, basis, basis);

    private bool HasSharedOriginAtSite()
    {
        var sitePosition = Site.On(Carrier).Normalize();

        foreach (var relation in Basis.Relations)
        {
            if (relation.OriginPosition.Normalize() != sitePosition)
            {
                return false;
            }
        }

        return true;
    }
}
