using Gojo.Core;

namespace Gojo.Diagnostics;

public static class TraversalDiagnostics
{
    public static TraversalRead Traverse(Carrier reader, Relation travel, params Relation[] relations) =>
        new(reader, travel, relations);
}
