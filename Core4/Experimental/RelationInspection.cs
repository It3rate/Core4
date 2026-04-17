using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// Signature-backed classification over experimental relations. Raw extraction stays structural so the signature can
/// remain a lossy numeric summary instead of becoming a second graph container.
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

    private static readonly Measure ReadPatternUnknown = Measure.Zero;
    private static readonly Measure ReadPatternCarrierPoint = Measure.One;
    private static readonly Measure ReadPatternCarrierAlignment = new(2);
    private static readonly Measure ReadPatternPointPair = new(3);
    private static readonly Measure ReadPatternUnitCandidate = new(4);
    private static readonly Measure ReadPatternJoinCandidate = new(5);
    private static readonly Measure ReadPatternUnitCandidatePair = new(6);

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
        relation.ComputeCarrierPattern(),
        relation.ComputeBasePattern(),
        relation.ComputeReadPattern(),
        relation.ComputeAlignmentPattern(),
        relation.ComputeFusionPattern(),
        relation.Multiplicity);

    public static ElementOrderSignature ElementOrder(this Relation relation) => new(
        OrderOf(relation.Base),
        OrderOf(relation.Attached));

    public static Measure FreezeOrder(this Relation relation) =>
        new(Math.Max(FreezeOrderOf(relation.Base).Raw, FreezeOrderOf(relation.Attached).Raw) + 1L);

    public static Measure CarrierPattern(this Relation relation) =>
        relation.Signature().CarrierPattern;

    public static Measure BasePattern(this Relation relation) =>
        relation.Signature().BasePattern;

    public static Measure ReadPattern(this Relation relation) =>
        relation.Signature().ReadPattern;

    public static Measure AlignmentPattern(this Relation relation) =>
        relation.Signature().AlignmentPattern;

    public static Measure FusionPattern(this Relation relation) =>
        relation.Signature().FusionPattern;

    public static bool IsFirstOrder(this Relation relation) =>
        relation.Signature().FreezeOrder == Measure.One;

    public static bool IsSecondOrder(this Relation relation) =>
        relation.Signature().FreezeOrder > Measure.One;

    public static bool IsRelationToRelation(this Relation relation) =>
        relation.Base is Relation && relation.Attached is Relation;

    public static bool IsCarrierPointRelation(this Relation relation) =>
        relation.ReadPattern() == ReadPatternCarrierPoint;

    public static bool IsCarrierAlignment(this Relation relation) =>
        relation.ReadPattern() == ReadPatternCarrierAlignment;

    public static bool TryGetCarrierPoint(this Relation relation, out Carrier carrier, out Measure location) =>
        TryGetCarrierPointRaw(relation, out carrier, out location);

    public static bool TryGetCarrierAlignment(this Relation relation, out Carrier @base, out Carrier attached) =>
        TryGetCarrierAlignmentRaw(relation, out @base, out attached);

    public static bool IsPointPair(this Relation relation)
    {
        var readPattern = relation.ReadPattern();
        return readPattern == ReadPatternPointPair
            || readPattern == ReadPatternUnitCandidate
            || readPattern == ReadPatternJoinCandidate;
    }

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
        && relation.CarrierPattern() == CarrierPatternSameCarrier;

    public static bool IsCrossCarrierPointPair(this Relation relation)
    {
        var carrierPattern = relation.CarrierPattern();
        return relation.IsPointPair()
            && (carrierPattern == CarrierPatternSameDomain
                || carrierPattern == CarrierPatternDifferentDomain);
    }

    public static bool IsSameDomainCrossCarrierPointPair(this Relation relation) =>
        relation.IsPointPair()
        && relation.CarrierPattern() == CarrierPatternSameDomain;

    public static bool CanReadAsUnitCandidate(this Relation relation) =>
        relation.ReadPattern() == ReadPatternUnitCandidate;

    public static bool CanReadAsJoinCandidate(this Relation relation) =>
        relation.ReadPattern() == ReadPatternJoinCandidate;

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
        relation.ReadPattern() == ReadPatternUnitCandidatePair;

    public static bool TryGetUnitCandidatePair(
        this Relation relation,
        out Relation baseUnit,
        out Relation attachedUnit) =>
        TryGetUnitCandidatePairRaw(relation, out baseUnit, out attachedUnit);

    public static bool IsSameCarrierUnitCandidatePair(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.CarrierPattern() == CarrierPatternSameCarrier;

    public static bool IsCrossCarrierUnitCandidatePair(this Relation relation)
    {
        var carrierPattern = relation.CarrierPattern();
        return relation.IsUnitCandidatePair()
            && (carrierPattern == CarrierPatternSameDomain
                || carrierPattern == CarrierPatternDifferentDomain);
    }

    public static bool SharesUnitBase(this Relation relation) =>
        relation.IsUnitCandidatePair()
        && relation.BasePattern() == BasePatternShared;

    public static bool CanReadAsSelfInteractionCandidate(this Relation relation) =>
        relation.FusionPattern() == FusionPatternSelf;

    public static bool CanReadAsDirectedSegmentCandidate(this Relation relation) =>
        relation.FusionPattern() == FusionPatternDirectedSegment;

    public static bool CanReadAsAreaCandidate(this Relation relation) =>
        relation.FusionPattern() == FusionPatternArea;

    public static bool CanReadAsComplexCandidate(this Relation relation) =>
        relation.FusionPattern() == FusionPatternComplex;

    public static bool CanReadAsSelfCandidate(this Relation relation) =>
        relation.AlignmentPattern() == AlignmentPatternSelf;

    public static bool CanReadAsAlignedCandidate(this Relation relation) =>
        relation.AlignmentPattern() == AlignmentPatternAligned;

    public static bool CanReadAsOpposedCandidate(this Relation relation) =>
        relation.AlignmentPattern() == AlignmentPatternOpposed;

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

    private static Measure ComputeCarrierPattern(this Relation relation)
    {
        if (TryGetReferenceCarrierRaw(relation.Base, out var baseCarrier)
            && TryGetReferenceCarrierRaw(relation.Attached, out var attachedCarrier))
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

    private static Measure ComputeBasePattern(this Relation relation)
    {
        if (relation.Base is Relation baseRelation && relation.Attached is Relation attachedRelation)
        {
            return ElementsShareBase(baseRelation.Base, attachedRelation.Base)
                ? BasePatternShared
                : BasePatternDistinct;
        }

        return BasePatternUnknown;
    }

    private static Measure ComputeReadPattern(this Relation relation)
    {
        if (TryGetCarrierPointRaw(relation, out _, out _))
        {
            return ReadPatternCarrierPoint;
        }

        if (TryGetCarrierAlignmentRaw(relation, out _, out _))
        {
            return ReadPatternCarrierAlignment;
        }

        if (TryGetUnitCandidatePairRaw(relation, out _, out _))
        {
            return ReadPatternUnitCandidatePair;
        }

        if (TryGetCarrierPointPairRaw(
            relation,
            out var baseCarrier,
            out var baseLocation,
            out var attachedCarrier,
            out var attachedLocation))
        {
            if (baseCarrier == attachedCarrier && baseLocation != attachedLocation)
            {
                return ReadPatternUnitCandidate;
            }

            if (baseCarrier != attachedCarrier)
            {
                return ReadPatternJoinCandidate;
            }

            return ReadPatternPointPair;
        }

        return ReadPatternUnknown;
    }

    private static Measure ComputeAlignmentPattern(this Relation relation)
    {
        if (!TryGetCarrierPointPairRaw(
            relation,
            out var baseCarrier,
            out var baseLocation,
            out var attachedCarrier,
            out var attachedLocation))
        {
            return AlignmentPatternUnknown;
        }

        if (!(baseCarrier == attachedCarrier || baseCarrier.Domain == attachedCarrier.Domain))
        {
            return AlignmentPatternUnknown;
        }

        if (baseCarrier == attachedCarrier && baseLocation == attachedLocation)
        {
            return AlignmentPatternSelf;
        }

        var baseSign = baseLocation.Sign;
        var attachedSign = attachedLocation.Sign;
        if (baseSign == Measure.Zero || attachedSign == Measure.Zero)
        {
            return AlignmentPatternUnknown;
        }

        if (baseSign == attachedSign)
        {
            return AlignmentPatternAligned;
        }

        return baseSign == -attachedSign
            ? AlignmentPatternOpposed
            : AlignmentPatternUnknown;
    }

    private static Measure ComputeFusionPattern(this Relation relation)
    {
        if (!TryGetUnitCandidatePairRaw(relation, out var baseUnit, out var attachedUnit)
            || !UnitBasesShareRaw(baseUnit, attachedUnit)
            || !TryGetUnitCandidateRaw(baseUnit, out var baseCarrier, out _, out _, out var baseOffset)
            || !TryGetUnitCandidateRaw(attachedUnit, out var attachedCarrier, out _, out _, out var attachedOffset))
        {
            return FusionPatternUnknown;
        }

        if (baseCarrier == attachedCarrier)
        {
            if (baseOffset == attachedOffset)
            {
                return FusionPatternSelf;
            }

            return baseOffset.Sign != Measure.Zero && baseOffset.Sign == -attachedOffset.Sign
                ? FusionPatternDirectedSegment
                : FusionPatternUnknown;
        }

        if (baseCarrier.Domain == attachedCarrier.Domain || baseOffset.Sign == Measure.Zero)
        {
            return FusionPatternUnknown;
        }

        if (baseOffset.Sign == attachedOffset.Sign)
        {
            return FusionPatternArea;
        }

        return baseOffset.Sign == -attachedOffset.Sign
            ? FusionPatternComplex
            : FusionPatternUnknown;
    }

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

    private static bool ElementsShareBase(IElement baseElement, IElement attachedElement)
    {
        if (Equals(baseElement, attachedElement))
        {
            return true;
        }

        return baseElement is Relation baseRelation
            && attachedElement is Relation attachedRelation
            && TryGetCarrierPointRaw(baseRelation, out _, out var baseLocation)
            && TryGetCarrierPointRaw(attachedRelation, out _, out var attachedLocation)
            && baseLocation == attachedLocation;
    }

    private static bool UnitBasesShareRaw(Relation baseUnit, Relation attachedUnit) =>
        ElementsShareBase(baseUnit.Base, attachedUnit.Base);

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
            default:
                carrier = default;
                return false;
        }
    }
}
