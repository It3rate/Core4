using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// Very early structural inspection over experimental relations. These predicates intentionally stay close to the
/// raw element pattern instead of creating a full taxonomy too early.
/// </summary>
public static class RelationInspection
{
    private static readonly Measure UnknownOrder = Measure.Zero;
    private static readonly Measure CarrierOrder = Measure.One;
    private static readonly Measure MeasureOrder = new(2);
    private static readonly Measure RelationOrderBase = new(3);

    private static readonly Measure CarrierPatternUnknown = Measure.Zero;
    private static readonly Measure CarrierPatternSameCarrier = Measure.One;
    private static readonly Measure CarrierPatternSameDomain = new(2);
    private static readonly Measure CarrierPatternDifferentDomain = new(3);

    private static readonly Measure BasePatternUnknown = Measure.Zero;
    private static readonly Measure BasePatternDistinct = Measure.One;
    private static readonly Measure BasePatternShared = new(2);

    private static readonly Measure AlignmentPatternUnknown = Measure.Zero;
    private static readonly Measure AlignmentPatternSelf = Measure.One;
    private static readonly Measure AlignmentPatternAligned = new(2);
    private static readonly Measure AlignmentPatternOpposed = new(3);

    private static readonly Measure FusionPatternUnknown = Measure.Zero;
    private static readonly Measure FusionPatternSelf = Measure.One;
    private static readonly Measure FusionPatternDirectedSegment = new(2);
    private static readonly Measure FusionPatternArea = new(3);
    private static readonly Measure FusionPatternComplex = new(4);

    public static RelationSignature Signature(this Relation relation) => new(
        relation.ElementOrder(),
        relation.FreezeOrder(),
        relation.CarrierPattern(),
        relation.BasePattern(),
        relation.AlignmentPattern(),
        relation.FusionPattern(),
        relation.Multiplicity);

    public static ElementOrderSignature ElementOrder(this Relation relation) => new(
        OrderOf(relation.Base),
        OrderOf(relation.Attached));

    public static Measure FreezeOrder(this Relation relation) =>
        new(Math.Max(FreezeOrderOf(relation.Base).Raw, FreezeOrderOf(relation.Attached).Raw) + 1L);

    public static Measure CarrierPattern(this Relation relation)
    {
        if (TryGetReferenceCarrier(relation.Base, out var baseCarrier)
            && TryGetReferenceCarrier(relation.Attached, out var attachedCarrier))
        {
            if (baseCarrier == attachedCarrier)
            {
                return CarrierPatternSameCarrier;
            }

            return baseCarrier.Domain == attachedCarrier.Domain
                ? CarrierPatternSameDomain
                : CarrierPatternDifferentDomain;
        }

        return CarrierPatternUnknown;
    }

    public static Measure BasePattern(this Relation relation)
    {
        if (relation.Base is Relation baseRelation && relation.Attached is Relation attachedRelation)
        {
            return Equals(baseRelation.Base, attachedRelation.Base)
                ? BasePatternShared
                : BasePatternDistinct;
        }

        return BasePatternUnknown;
    }

    public static bool IsFirstOrder(this Relation relation) =>
        relation.Signature().FreezeOrder == Measure.One;

    public static bool IsSecondOrder(this Relation relation) =>
        relation.Signature().FreezeOrder > Measure.One;

    public static bool IsRelationToRelation(this Relation relation) =>
        relation.Base is Relation && relation.Attached is Relation;

    public static bool IsCarrierPointRelation(this Relation relation) =>
        relation.Base is Carrier && relation.Attached is Measure;

    public static bool IsCarrierAlignment(this Relation relation) =>
        relation.Base is Carrier && relation.Attached is Carrier;

    public static bool TryGetCarrierPoint(this Relation relation, out Carrier carrier, out Measure location)
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

    public static bool TryGetCarrierAlignment(this Relation relation, out Carrier @base, out Carrier attached)
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

    public static bool IsPointPair(this Relation relation) =>
        relation.Base is Relation basePoint
        && relation.Attached is Relation attachedPoint
        && basePoint.IsCarrierPointRelation()
        && attachedPoint.IsCarrierPointRelation();

    public static bool TryGetCarrierPointPair(
        this Relation relation,
        out Carrier baseCarrier,
        out Measure baseLocation,
        out Carrier attachedCarrier,
        out Measure attachedLocation)
    {
        if (relation.Base is Relation basePoint
            && relation.Attached is Relation attachedPoint
            && basePoint.TryGetCarrierPoint(out baseCarrier, out baseLocation)
            && attachedPoint.TryGetCarrierPoint(out attachedCarrier, out attachedLocation))
        {
            return true;
        }

        baseCarrier = default;
        baseLocation = default;
        attachedCarrier = default;
        attachedLocation = default;
        return false;
    }

    public static bool IsSameCarrierPointPair(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out _, out var attachedCarrier, out _)
        && baseCarrier == attachedCarrier;

    public static bool IsCrossCarrierPointPair(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out _, out var attachedCarrier, out _)
        && baseCarrier != attachedCarrier;

    public static bool IsSameDomainCrossCarrierPointPair(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out _, out var attachedCarrier, out _)
        && baseCarrier != attachedCarrier
        && baseCarrier.Domain == attachedCarrier.Domain;

    public static bool CanReadAsUnitCandidate(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation)
        && baseCarrier == attachedCarrier
        && baseLocation != attachedLocation;

    public static bool CanReadAsJoinCandidate(this Relation relation) =>
        relation.IsCrossCarrierPointPair();

    public static bool TryGetUnitCandidate(
        this Relation relation,
        out Carrier carrier,
        out Measure originLocation,
        out Measure attachedLocation,
        out Measure attachedOffset)
    {
        if (relation.CanReadAsUnitCandidate()
            && relation.TryGetCarrierPointPair(out carrier, out originLocation, out _, out attachedLocation))
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

    public static bool IsUnitCandidatePair(this Relation relation) =>
        relation.Base is Relation baseUnit
        && relation.Attached is Relation attachedUnit
        && baseUnit.CanReadAsUnitCandidate()
        && attachedUnit.CanReadAsUnitCandidate();

    public static bool TryGetUnitCandidatePair(
        this Relation relation,
        out Relation baseUnit,
        out Relation attachedUnit)
    {
        if (relation.Base is Relation baseRelation
            && relation.Attached is Relation attachedRelation
            && baseRelation.CanReadAsUnitCandidate()
            && attachedRelation.CanReadAsUnitCandidate())
        {
            baseUnit = baseRelation;
            attachedUnit = attachedRelation;
            return true;
        }

        baseUnit = default;
        attachedUnit = default;
        return false;
    }

    public static bool IsSameCarrierUnitCandidatePair(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && baseUnit.TryGetUnitCandidate(out var baseCarrier, out _, out _, out _)
        && attachedUnit.TryGetUnitCandidate(out var attachedCarrier, out _, out _, out _)
        && baseCarrier == attachedCarrier;

    public static bool IsCrossCarrierUnitCandidatePair(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && baseUnit.TryGetUnitCandidate(out var baseCarrier, out _, out _, out _)
        && attachedUnit.TryGetUnitCandidate(out var attachedCarrier, out _, out _, out _)
        && baseCarrier != attachedCarrier;

    public static bool SharesUnitBase(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && (
            Equals(baseUnit.Base, attachedUnit.Base)
            || baseUnit.Base is Relation baseOrigin
            && attachedUnit.Base is Relation attachedOrigin
            && baseOrigin.TryGetCarrierPoint(out _, out var baseOriginLocation)
            && attachedOrigin.TryGetCarrierPoint(out _, out var attachedOriginLocation)
            && baseOriginLocation == attachedOriginLocation);

    public static bool CanReadAsSelfInteractionCandidate(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && relation.SharesUnitBase()
        && baseUnit.TryGetUnitCandidate(out var baseCarrier, out _, out _, out var baseOffset)
        && attachedUnit.TryGetUnitCandidate(out var attachedCarrier, out _, out _, out var attachedOffset)
        && baseCarrier == attachedCarrier
        && baseOffset == attachedOffset;

    public static bool CanReadAsDirectedSegmentCandidate(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && relation.SharesUnitBase()
        && baseUnit.TryGetUnitCandidate(out var baseCarrier, out _, out _, out var baseOffset)
        && attachedUnit.TryGetUnitCandidate(out var attachedCarrier, out _, out _, out var attachedOffset)
        && baseCarrier == attachedCarrier
        && baseOffset.Sign != Measure.Zero
        && baseOffset.Sign == -attachedOffset.Sign;

    public static bool CanReadAsAreaCandidate(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && relation.SharesUnitBase()
        && baseUnit.TryGetUnitCandidate(out var baseCarrier, out _, out _, out var baseOffset)
        && attachedUnit.TryGetUnitCandidate(out var attachedCarrier, out _, out _, out var attachedOffset)
        && baseCarrier != attachedCarrier
        && baseCarrier.Domain != attachedCarrier.Domain
        && baseOffset.Sign != Measure.Zero
        && baseOffset.Sign == attachedOffset.Sign;

    public static bool CanReadAsComplexCandidate(this Relation relation) =>
        relation.TryGetUnitCandidatePair(out var baseUnit, out var attachedUnit)
        && relation.SharesUnitBase()
        && baseUnit.TryGetUnitCandidate(out var baseCarrier, out _, out _, out var baseOffset)
        && attachedUnit.TryGetUnitCandidate(out var attachedCarrier, out _, out _, out var attachedOffset)
        && baseCarrier != attachedCarrier
        && baseCarrier.Domain != attachedCarrier.Domain
        && baseOffset.Sign != Measure.Zero
        && baseOffset.Sign == -attachedOffset.Sign;

    public static bool CanReadAsSelfCandidate(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation)
        && baseCarrier == attachedCarrier
        && baseLocation == attachedLocation;

    public static bool CanReadAsAlignedCandidate(this Relation relation)
    {
        if (!relation.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation))
        {
            return false;
        }

        if (!(baseCarrier == attachedCarrier || baseCarrier.Domain == attachedCarrier.Domain))
        {
            return false;
        }

        if (baseCarrier == attachedCarrier && baseLocation == attachedLocation)
        {
            return false;
        }

        var baseSign = baseLocation.Sign;
        var attachedSign = attachedLocation.Sign;
        return baseSign != Measure.Zero
            && attachedSign != Measure.Zero
            && baseSign == attachedSign;
    }

    public static bool CanReadAsOpposedCandidate(this Relation relation)
    {
        if (!relation.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation))
        {
            return false;
        }

        if (!(baseCarrier == attachedCarrier || baseCarrier.Domain == attachedCarrier.Domain))
        {
            return false;
        }

        var baseSign = baseLocation.Sign;
        var attachedSign = attachedLocation.Sign;
        return baseSign != Measure.Zero
            && attachedSign != Measure.Zero
            && baseSign == -attachedSign;
    }

    public static Measure AlignmentPattern(this Relation relation)
    {
        if (relation.CanReadAsSelfCandidate())
        {
            return AlignmentPatternSelf;
        }

        if (relation.CanReadAsAlignedCandidate())
        {
            return AlignmentPatternAligned;
        }

        if (relation.CanReadAsOpposedCandidate())
        {
            return AlignmentPatternOpposed;
        }

        return AlignmentPatternUnknown;
    }

    public static Measure FusionPattern(this Relation relation)
    {
        if (relation.CanReadAsSelfInteractionCandidate())
        {
            return FusionPatternSelf;
        }

        if (relation.CanReadAsDirectedSegmentCandidate())
        {
            return FusionPatternDirectedSegment;
        }

        if (relation.CanReadAsAreaCandidate())
        {
            return FusionPatternArea;
        }

        if (relation.CanReadAsComplexCandidate())
        {
            return FusionPatternComplex;
        }

        return FusionPatternUnknown;
    }

    public static Measure AttachedOffset(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation)
        && baseCarrier == attachedCarrier
            ? attachedLocation - baseLocation
            : Measure.Zero;

    public static bool IsRepeated(this Relation relation) =>
        relation.Signature().Multiplicity != Measure.One;

    public static bool SharesBase(this Relation relation, Relation other) =>
        Equals(relation.Base, other.Base);

    private static Measure OrderOf(IElement element) => element switch
    {
        Carrier => CarrierOrder,
        Measure => MeasureOrder,
        Relation relation => new(RelationOrderBase.Raw + relation.FreezeOrder().Raw - 1L),
        _ => UnknownOrder
    };

    private static Measure FreezeOrderOf(IElement element) => element switch
    {
        Relation relation => relation.FreezeOrder(),
        _ => Measure.Zero
    };

    private static bool TryGetReferenceCarrier(IElement element, out Carrier carrier)
    {
        switch (element)
        {
            case Carrier directCarrier:
                carrier = directCarrier;
                return true;
            case Relation relation when relation.IsCarrierPointRelation():
                carrier = (Carrier)relation.Base;
                return true;
            case Relation relation when relation.TryGetUnitCandidate(out carrier, out _, out _, out _):
                return true;
            default:
                carrier = default;
                return false;
        }
    }
}
