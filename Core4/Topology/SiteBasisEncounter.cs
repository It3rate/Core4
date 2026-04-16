using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// One basis-like calibration law encounterable at a pinned site. This remains descriptive for now: it surfaces the
/// local one-sided or bilateral calibration already implied by incident relations without yet deciding traversal
/// policy from it.
/// </summary>
public readonly record struct SiteBasisEncounter(
    Site Site,
    Carrier Carrier,
    Basis Basis,
    ISupport SourceSupport,
    Measure SupportPotential)
{
    public Measure Grade => Basis.Grade;

    public Measure RelationCount => Basis.RelationCount;

    public ISupportFold CalibrationRead => Basis;

    public Ratio Calibration => CalibrationRead.Fold.Normalize();

    public Measure Potential => CalibrationRead.Potential;

    public bool IsAtomic => Basis.IsAtomic;

    public bool IsComposite => Basis.IsComposite;

    public Ratio Commitment => CalibrationRead.Potential == Measure.Zero
        ? Ratio.Zero(Measure.One)
        : new Ratio(
            SupportPotential.Raw <= CalibrationRead.Potential.Raw
                ? SupportPotential
                : CalibrationRead.Potential,
            CalibrationRead.Potential).Canonicalize();

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
        new(site, relation.Carrier, Basis.Freeze(relation.Flip()), relation, relation.Potential);

    internal static SiteBasisEncounter FromOutgoing(Site site, Relation relation) =>
        new(site, relation.Carrier, Basis.Freeze(relation), relation, relation.Potential);

    internal static SiteBasisEncounter FromPassThrough(Site site, Relation incoming, Relation outgoing) =>
        new(
            site,
            outgoing.Carrier,
            Basis.Fuse(
                Basis.Freeze(incoming.Flip()),
                Basis.Freeze(outgoing)),
            outgoing,
            incoming.Potential + outgoing.Potential);

    internal static SiteBasisEncounter FromPinnedBasis(Site site, Carrier carrier, Basis basis, Measure supportPotential) =>
        new(site, carrier, basis, basis, supportPotential);

    internal bool TryGetExplicitRemap(out Site target, out Relation relation)
    {
        if (!IsExplicitPinnedBasis || !IsAtomic || !IsLocalToSite)
        {
            target = default;
            relation = default;
            return false;
        }

        relation = Basis.AtomicRelation;
        target = relation.Anchor;
        return !SameNode(Site, target);
    }

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

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}
