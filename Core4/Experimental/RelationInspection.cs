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

    public static RelationSignature Signature(this Relation relation) => new(
        relation.ElementOrder(),
        relation.FreezeOrder(),
        relation.CarrierPattern(),
        relation.BasePattern(),
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

    public static bool CanReadAsUnitCandidate(this Relation relation) =>
        relation.TryGetCarrierPointPair(out var baseCarrier, out var baseLocation, out var attachedCarrier, out var attachedLocation)
        && baseCarrier == attachedCarrier
        && baseLocation != attachedLocation;

    public static bool CanReadAsJoinCandidate(this Relation relation) =>
        relation.IsCrossCarrierPointPair();

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
            default:
                carrier = default;
                return false;
        }
    }
}
