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
    Measure SupportPotential) : ISupportFold
{
    internal SupportStructure Structure => Basis.Structure;

    public Measure Grade => Structure.SupportGrade;

    public Measure RelationCount => Structure.RelationCount;

    public ISupportFold CalibrationRead => Basis;

    public ISupportProjection FoldRead => CalibrationRead.FoldRead;

    // TODO(core): Delete this compatibility alias once callers consume FoldRead directly.
    public Ratio Fold => FoldRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers stop expecting encounter calibration as Ratio.
    public Ratio Calibration => Fold;

    public Measure Potential => CalibrationRead.Potential;

    public bool IsAtomic => Structure.IsAtomic;

    public bool IsComposite => Structure.IsComposite;

    public Ratio Commitment => CalibrationRead.Potential == Measure.Zero
        ? Ratio.Zero(Measure.One)
        : new Ratio(
            SupportPotential.Raw <= CalibrationRead.Potential.Raw
                ? SupportPotential
                : CalibrationRead.Potential,
            CalibrationRead.Potential).Canonicalize();

    public bool IsExplicitPinnedBasis => SourceSupport is Basis;

    public bool IsRelationDerived => SourceSupport is Relation;

    public bool IsLocalToSite => Structure.HasSharedOriginAt(Site, Carrier);

    public bool HasRecessiveCalibration => Structure.HasRecessiveCalibration();

    public bool HasDominantCalibration => Structure.HasDominantCalibration();

    public bool IsOneSided => HasRecessiveCalibration != HasDominantCalibration;

    public bool IsBilateral => Structure.IsBilateralAt(Site, Carrier);

    public bool SupportsDirection(Measure direction) => direction.Raw switch
    {
        < 0L => HasRecessiveCalibration,
        > 0L => HasDominantCalibration,
        _ => false
    };

    public ImmutableArray<Relation> LocalRelations => Structure.SelectRelations();

    public BasisEncounterAgreement Agree(
        TraversalSignature signature,
        Measure desiredDirection) =>
        BasisEncounterAgreement.Create(signature, this, desiredDirection);

    internal static ImmutableArray<SiteBasisEncounter> SelectAt(
        Site site,
        Carrier carrier,
        ImmutableArray<SiteLeg> legs)
    {
        var incoming = ImmutableArray.CreateBuilder<Relation>();
        var outgoing = ImmutableArray.CreateBuilder<Relation>();
        var encounters = ImmutableArray.CreateBuilder<SiteBasisEncounter>();

        foreach (var leg in legs)
        {
            if (TryCreatePinnedBasisEncounter(site, carrier, leg, out var pinned))
            {
                encounters.Add(pinned);
                continue;
            }

            if (leg.Support is not Relation relation)
            {
                continue;
            }

            if (leg.IsIncoming)
            {
                incoming.Add(relation);
                encounters.Add(FromIncoming(site, relation));
            }
            else if (leg.IsOutgoing)
            {
                outgoing.Add(relation);
                encounters.Add(FromOutgoing(site, relation));
            }
        }

        if (incoming.Count == 1 && outgoing.Count == 1)
        {
            encounters.Insert(0, FromPassThrough(site, incoming[0], outgoing[0]));
        }

        return encounters.ToImmutable();
    }

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

        relation = Structure.AtomicRelation;
        target = relation.Anchor;
        return !SameNode(Site, target);
    }

    private static bool TryCreatePinnedBasisEncounter(
        Site site,
        Carrier carrier,
        SiteLeg leg,
        out SiteBasisEncounter encounter)
    {
        if (leg.Support is Basis basis && basis.PrimaryCarrier == carrier)
        {
            encounter = FromPinnedBasis(site, carrier, basis, leg.Potential);
            return true;
        }

        encounter = default;
        return false;
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();
}
