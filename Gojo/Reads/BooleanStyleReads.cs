using Gojo.Core;

namespace Gojo.Reads;

public static class BooleanStyleReads
{
    public static Relation[] AndOn(Carrier reader, params Relation[] relations) =>
        RelationReads.TensionOn(reader, relations);

    public static Relation[] OrOn(Carrier reader, params Relation[] relations) =>
        RelationReads.SupportOn(reader, relations);

    public static Relation[] XorOn(Carrier reader, params Relation[] relations) =>
        RelationReads.ExclusiveOn(reader, relations);

    public static Relation[] NotOn(Carrier reader, Relation scope, params Relation[] blocked) =>
        RelationReads.UncoveredSpansOn(reader, scope, blocked);

    public static Relation[] PassOn(Carrier reader, Relation scope, params Relation[] support) =>
        RelationReads.CoveredSpansWithinOn(reader, scope, support);

    public static Relation[] BlockOn(Carrier reader, Relation scope, params Relation[] support) =>
        RelationReads.UncoveredSpansOn(reader, scope, support);
}
