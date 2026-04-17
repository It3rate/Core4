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

    public static bool IsRepeated(this Relation relation) =>
        relation.Multiplicity != Measure.One;

    public static bool SharesBase(this Relation relation, Relation other) =>
        Equals(relation.Base, other.Base);
}
