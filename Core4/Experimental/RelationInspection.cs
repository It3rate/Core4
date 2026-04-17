using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// Very early structural inspection over experimental relations. These predicates intentionally stay close to the
/// raw element pattern instead of creating a full taxonomy too early.
/// </summary>
public static class RelationInspection
{
    public static bool IsFirstOrder(this Relation relation) =>
        relation.Base is not Relation && relation.Attached is not Relation;

    public static bool IsSecondOrder(this Relation relation) =>
        relation.Base is Relation || relation.Attached is Relation;

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
        relation.Multiplicity != Measure.One;

    public static bool SharesBase(this Relation relation, Relation other) =>
        Equals(relation.Base, other.Base);
}
