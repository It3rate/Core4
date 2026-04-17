using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// Primitive experimental inspection over ordered element relations. The signature stays small and numeric, while
/// named readings remain derived views on top of the primitive participation axes.
/// </summary>
public static class RelationInspection
{
    private static readonly RelationCode UnknownPolarity = RelationCode.Of(RelationPolarity.Unknown);
    private static readonly RelationCode DominantPolarity = RelationCode.Of(RelationPolarity.Dominant);
    private static readonly RelationCode RecessivePolarity = RelationCode.Of(RelationPolarity.Recessive);

    private static readonly RelationCode UnknownMergeScope = RelationCode.Of(RelationMergeScope.Unknown);
    private static readonly RelationCode WholeMergeScope = RelationCode.Of(RelationMergeScope.Whole);
    private static readonly RelationCode NodeMergeScope = RelationCode.Of(RelationMergeScope.Node);

    private static readonly RelationCode UnknownNovelty = RelationCode.Of(RelationNovelty.Unknown);
    private static readonly RelationCode ExistingNovelty = RelationCode.Of(RelationNovelty.Existing);
    private static readonly RelationCode NewNovelty = RelationCode.Of(RelationNovelty.New);

    public static RelationSignature Signature(this Relation relation) => new(
        relation.Polarity(),
        relation.MergeScope(),
        relation.CarrierRelation(),
        relation.DomainRelation(),
        relation.Multiplicity);

    public static OrderedRelationCode Polarity(this Relation relation) => new(
        PolarityOf(relation.Base),
        PolarityOf(relation.Attached));

    public static OrderedRelationCode MergeScope(this Relation relation) => new(
        MergeScopeOf(relation.Base),
        MergeScopeOf(relation.Attached));

    public static RelationCode CarrierRelation(this Relation relation)
    {
        if (!TryGetReferenceCarrierRaw(relation.Base, out var baseCarrier)
            || !TryGetReferenceCarrierRaw(relation.Attached, out var attachedCarrier))
        {
            return UnknownNovelty;
        }

        return baseCarrier == attachedCarrier
            ? ExistingNovelty
            : NewNovelty;
    }

    public static RelationCode DomainRelation(this Relation relation)
    {
        if (!TryGetReferenceCarrierRaw(relation.Base, out var baseCarrier)
            || !TryGetReferenceCarrierRaw(relation.Attached, out var attachedCarrier))
        {
            return UnknownNovelty;
        }

        return baseCarrier.Domain == attachedCarrier.Domain
            ? ExistingNovelty
            : NewNovelty;
    }

    public static Measure FreezeOrder(this Relation relation) =>
        new(Math.Max(FreezeOrderOf(relation.Base).Raw, FreezeOrderOf(relation.Attached).Raw) + 1L);

    public static bool IsFirstOrder(this Relation relation) =>
        relation.FreezeOrder() == Measure.One;

    public static bool IsSecondOrder(this Relation relation) =>
        relation.FreezeOrder() > Measure.One;

    public static bool IsRelationToRelation(this Relation relation) =>
        relation.Base is Relation && relation.Attached is Relation;

    public static bool IsCarrierPointRelation(this Relation relation) =>
        TryGetCarrierPointRaw(relation, out _, out _);

    public static bool IsCarrierAlignment(this Relation relation) =>
        TryGetCarrierAlignmentRaw(relation, out _, out _);

    public static bool TryGetCarrierPoint(this Relation relation, out Carrier carrier, out Measure location) =>
        TryGetCarrierPointRaw(relation, out carrier, out location);

    public static bool TryGetCarrierAlignment(this Relation relation, out Carrier @base, out Carrier attached) =>
        TryGetCarrierAlignmentRaw(relation, out @base, out attached);

    public static bool IsPointPair(this Relation relation) =>
        TryGetCarrierPointPairRaw(
            relation,
            out _,
            out _,
            out _,
            out _);

    public static bool TryGetCarrierPointPair(
        this Relation relation,
        out Carrier baseCarrier,
        out Measure baseLocation,
        out Carrier attachedCarrier,
        out Measure attachedLocation) =>
        TryGetCarrierPointPairRaw(
            relation,
            out baseCarrier,
            out baseLocation,
            out attachedCarrier,
            out attachedLocation);

    public static bool IsSameCarrierPointPair(this Relation relation) =>
        relation.IsPointPair()
        && relation.CarrierRelation() == ExistingNovelty;

    public static bool IsCrossCarrierPointPair(this Relation relation) =>
        relation.IsPointPair()
        && relation.CarrierRelation() == NewNovelty;

    public static bool IsSameDomainCrossCarrierPointPair(this Relation relation) =>
        relation.IsPointPair()
        && relation.CarrierRelation() == NewNovelty
        && relation.DomainRelation() == ExistingNovelty;

    public static bool CanReadAsUnitCandidate(this Relation relation) =>
        TryGetUnitCandidateRaw(
            relation,
            out _,
            out _,
            out _,
            out _);

    public static bool CanReadAsJoinCandidate(this Relation relation) =>
        relation.IsCrossCarrierPointPair();

    public static bool TryGetUnitCandidate(
        this Relation relation,
        out Carrier carrier,
        out Measure originLocation,
        out Measure attachedLocation,
        out Measure attachedOffset) =>
        TryGetUnitCandidateRaw(
            relation,
            out carrier,
            out originLocation,
            out attachedLocation,
            out attachedOffset);

    public static bool IsUnitCandidatePair(this Relation relation) =>
        TryGetUnitCandidatePairRaw(relation, out _, out _);

    public static bool TryGetUnitCandidatePair(
        this Relation relation,
        out Relation baseUnit,
        out Relation attachedUnit) =>
        TryGetUnitCandidatePairRaw(relation, out baseUnit, out attachedUnit);

    public static bool IsSameCarrierUnitCandidatePair(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.CarrierRelation() == ExistingNovelty;

    public static bool IsCrossCarrierUnitCandidatePair(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.CarrierRelation() == NewNovelty;

    public static bool SharesUnitBase(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && UnitBasesShareRaw(baseUnit, attachedUnit);

    public static bool CanReadAsSelfInteractionCandidate(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.SharesUnitBase()
        && relation.CarrierRelation() == ExistingNovelty
        && HaveSamePolarity(relation.Polarity());

    public static bool CanReadAsDirectedSegmentCandidate(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.SharesUnitBase()
        && relation.CarrierRelation() == ExistingNovelty
        && HaveOpposedPolarity(relation.Polarity());

    public static bool CanReadAsAreaCandidate(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.SharesUnitBase()
        && relation.CarrierRelation() == NewNovelty
        && relation.DomainRelation() == NewNovelty
        && HaveSamePolarity(relation.Polarity());

    public static bool CanReadAsComplexCandidate(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.SharesUnitBase()
        && relation.CarrierRelation() == NewNovelty
        && relation.DomainRelation() == NewNovelty
        && HaveOpposedPolarity(relation.Polarity());

    public static bool CanReadAsSelfCandidate(this Relation relation) =>
        TryGetCarrierPointPairRaw(
            relation,
            out var baseCarrier,
            out var baseLocation,
            out var attachedCarrier,
            out var attachedLocation)
        && baseCarrier == attachedCarrier
        && baseLocation == attachedLocation;

    public static bool CanReadAsAlignedCandidate(this Relation relation) =>
        relation.IsPointPair()
        && relation.DomainRelation() == ExistingNovelty
        && !relation.CanReadAsSelfCandidate()
        && HaveSamePolarity(relation.Polarity());

    public static bool CanReadAsOpposedCandidate(this Relation relation) =>
        relation.IsPointPair()
        && relation.DomainRelation() == ExistingNovelty
        && HaveOpposedPolarity(relation.Polarity());

    public static Measure AttachedOffset(this Relation relation) =>
        TryGetCarrierPointPairRaw(
            relation,
            out var baseCarrier,
            out var baseLocation,
            out var attachedCarrier,
            out var attachedLocation)
        && baseCarrier == attachedCarrier
            ? attachedLocation - baseLocation
            : Measure.Zero;

    public static bool IsRepeated(this Relation relation) =>
        relation.Signature().Multiplicity != Measure.One;

    public static bool SharesBase(this Relation relation, Relation other) =>
        Equals(relation.Base, other.Base);

    private static bool TryGetCarrierPointRaw(Relation relation, out Carrier carrier, out Measure location)
    {
        if (relation.Base is Carrier pointCarrier && relation.Attached is Measure pointLocation)
        {
            carrier = pointCarrier;
            location = pointLocation;
            return true;
        }

        carrier = default;
        location = default;
        return false;
    }

    private static bool TryGetCarrierAlignmentRaw(Relation relation, out Carrier @base, out Carrier attached)
    {
        if (relation.Base is Carrier baseCarrier && relation.Attached is Carrier attachedCarrier)
        {
            @base = baseCarrier;
            attached = attachedCarrier;
            return true;
        }

        @base = default;
        attached = default;
        return false;
    }

    private static bool TryGetCarrierPointPairRaw(
        Relation relation,
        out Carrier baseCarrier,
        out Measure baseLocation,
        out Carrier attachedCarrier,
        out Measure attachedLocation)
    {
        if (relation.Base is Relation basePoint
            && relation.Attached is Relation attachedPoint
            && TryGetCarrierPointRaw(basePoint, out baseCarrier, out baseLocation)
            && TryGetCarrierPointRaw(attachedPoint, out attachedCarrier, out attachedLocation))
        {
            return true;
        }

        baseCarrier = default;
        baseLocation = default;
        attachedCarrier = default;
        attachedLocation = default;
        return false;
    }

    private static bool TryGetUnitCandidateRaw(
        Relation relation,
        out Carrier carrier,
        out Measure originLocation,
        out Measure attachedLocation,
        out Measure attachedOffset)
    {
        if (TryGetCarrierPointPairRaw(
                relation,
                out carrier,
                out originLocation,
                out var pairedCarrier,
                out attachedLocation)
            && carrier == pairedCarrier
            && originLocation != attachedLocation)
        {
            attachedOffset = attachedLocation - originLocation;
            return true;
        }

        carrier = default;
        originLocation = default;
        attachedLocation = default;
        attachedOffset = default;
        return false;
    }

    private static bool TryGetUnitCandidatePairRaw(
        Relation relation,
        out Relation baseUnit,
        out Relation attachedUnit)
    {
        if (relation.Base is Relation baseRelation
            && relation.Attached is Relation attachedRelation
            && TryGetUnitCandidateRaw(baseRelation, out _, out _, out _, out _)
            && TryGetUnitCandidateRaw(attachedRelation, out _, out _, out _, out _))
        {
            baseUnit = baseRelation;
            attachedUnit = attachedRelation;
            return true;
        }

        baseUnit = default;
        attachedUnit = default;
        return false;
    }

    private static bool UnitBasesShareRaw(Relation baseUnit, Relation attachedUnit)
    {
        if (Equals(baseUnit.Base, attachedUnit.Base))
        {
            return true;
        }

        return baseUnit.Base is Relation baseOrigin
            && attachedUnit.Base is Relation attachedOrigin
            && TryGetCarrierPointRaw(baseOrigin, out _, out var baseOriginLocation)
            && TryGetCarrierPointRaw(attachedOrigin, out _, out var attachedOriginLocation)
            && baseOriginLocation == attachedOriginLocation;
    }

    private static bool HaveSamePolarity(OrderedRelationCode polarity) =>
        IsKnownPolarity(polarity.Base)
        && polarity.Base == polarity.Attached;

    private static bool HaveOpposedPolarity(OrderedRelationCode polarity) =>
        IsKnownPolarity(polarity.Base)
        && IsKnownPolarity(polarity.Attached)
        && polarity.Base != polarity.Attached;

    private static bool IsKnownPolarity(RelationCode polarity) =>
        polarity != UnknownPolarity;

    private static RelationCode PolarityOf(IElement element) => element switch
    {
        Measure measure => measure.Sign == -Measure.One ? RecessivePolarity : DominantPolarity,
        Carrier => DominantPolarity,
        Relation relation when TryGetUnitCandidateRaw(relation, out _, out _, out _, out var offset) =>
            offset.Sign == -Measure.One ? RecessivePolarity : DominantPolarity,
        Relation relation when TryGetCarrierPointRaw(relation, out _, out var location) =>
            location.Sign == -Measure.One ? RecessivePolarity : DominantPolarity,
        Relation relation => relation.Signature().Polarity.Attached,
        _ => UnknownPolarity
    };

    private static RelationCode MergeScopeOf(IElement element) => element switch
    {
        Measure => NodeMergeScope,
        Carrier => WholeMergeScope,
        Relation relation when TryGetCarrierPointRaw(relation, out _, out _) => NodeMergeScope,
        Relation => WholeMergeScope,
        _ => UnknownMergeScope
    };

    private static Measure FreezeOrderOf(IElement element) => element switch
    {
        Relation relation => relation.FreezeOrder(),
        _ => Measure.Zero
    };

    private static bool TryGetReferenceCarrierRaw(IElement element, out Carrier carrier)
    {
        switch (element)
        {
            case Carrier directCarrier:
                carrier = directCarrier;
                return true;
            case Relation relation when TryGetCarrierPointRaw(relation, out carrier, out _):
                return true;
            case Relation relation when TryGetUnitCandidateRaw(relation, out carrier, out _, out _, out _):
                return true;
            case Relation relation when TryGetReferenceCarrierRaw(relation.Base, out carrier):
                return true;
            default:
                carrier = default;
                return false;
        }
    }
}
