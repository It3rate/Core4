namespace ConceptUnits;

/// <summary>
/// Low-resolution concept encoding primitives for Gojo-centric concept work.
///
/// These primitives are intentionally structural and computable, and are meant
/// to be refined into higher-resolution designs over time.
/// </summary>
public static class ConceptUnitPrimitives
{
    public enum TopologyPrimitive
    {
        Area,
        LineRoute,
        Neck,
        BridgeNeck,
        HLink,
        StemJoin,
        SplitNode,
        MergeNode,
        LoopClosure,
        DeadEndSink,
        BypassBranch,
        FixedAnchor
    }

    public enum FlowContract
    {
        UnspecifiedTraversal,
        BidirectionalDeclared,
        ForwardConstrained,
        InverseConstrained,
        RadialConvergent,
        RadialDivergent
    }

    public enum ParameterPolicy
    {
        None,
        Derived,
        Tunable
    }

    public enum SettlementPolicy
    {
        Pass,
        Retain,
        Latch,
        Release,
        Dissipate,
        Sacrifice
    }

    public enum FocusPolicy
    {
        Neck,
        Join,
        Boundary,
        Gradient,
        Contour,
        Novelty
    }

    public sealed record ConceptUnit(
        string Name,
        TopologyPrimitive[] Topology,
        FlowContract Flow,
        ParameterPolicy Parameters,
        SettlementPolicy Settlement,
        FocusPolicy Focus,
        string[] Notes);
}
