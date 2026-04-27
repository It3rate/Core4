using Gojo.Core;

namespace Gojo.Reads;

public static class BooleanStyleReads
{
    public static Relation[] AndOn(Carrier reader, params Relation[] relations) =>
        Relation.TensionOn(reader, relations);

    public static Relation[] OrOn(Carrier reader, params Relation[] relations) =>
        Relation.SupportOn(reader, relations);

    public static Relation[] XorOn(Carrier reader, params Relation[] relations) =>
        Relation.ExclusiveOn(reader, relations);

    public static Relation[] NotOn(Carrier reader, Relation scope, params Relation[] blocked) =>
        Relation.UncoveredSpansOn(reader, scope, blocked);

    public static Relation[] PassOn(Carrier reader, Relation scope, params Relation[] support) =>
        Relation.CoveredSpansWithinOn(reader, scope, support);

    public static Relation[] BlockOn(Carrier reader, Relation scope, params Relation[] support) =>
        Relation.UncoveredSpansOn(reader, scope, support);
}
