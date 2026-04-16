using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// A numeric read of how the trolley's current traversal signature meets one local basis encounter. It is descriptive
/// for now: it does not yet choose policy, but it exposes resolution compatibility, directional preservation or
/// inversion, and whether the encountered basis is one-sided or bilateral for the current directional question.
/// </summary>
public readonly record struct BasisEncounterAgreement(
    SiteBasisEncounter Encounter,
    Ratio Resolution,
    Ratio Continuation,
    Ratio Capability,
    Ratio Commitment) : ISupportFold
{
    public ISupportFold CalibrationRead => Encounter;

    public ISupportProjection FoldRead => CalibrationRead.FoldRead;

    // TODO(core): Delete this compatibility alias once callers consume FoldRead directly.
    public Ratio Fold => FoldRead.NormalizedRatio();

    // TODO(core): Delete this compatibility alias once callers consume CalibrationRead/FoldRead directly.
    public Ratio Calibration => Fold;

    public Measure Potential => CalibrationRead.Potential;

    public Ratio Rank => Resolution.Add(Continuation).Add(Capability).Add(Commitment).Normalize();

    public bool PreservesDirection => Continuation == new Ratio(Measure.One, Measure.One);

    public bool InvertsDirection => Continuation == new Ratio(-Measure.One, Measure.One);

    public bool BlocksDirection => Continuation == Ratio.Zero(Measure.One);

    public bool IsBilateral => Capability == new Ratio(Measure.One, Measure.One);

    public bool IsOneSided => Capability == new Ratio(Measure.One, new Measure(2));

    public bool HasCommittedTransfer => Commitment.Potential != Measure.Zero;

    public Ratio Transmission => Commitment.Canonicalize();

    public Ratio Retention => new Ratio(Measure.One, Measure.One).Subtract(Commitment).Canonicalize();

    public int ComparePreference(BasisEncounterAgreement other)
    {
        var traversal = CompareTraversalPreference(other);

        if (traversal != 0)
        {
            return traversal;
        }

        return CompareRatio(Resolution, other.Resolution);
    }

    public int CompareTraversalPreference(BasisEncounterAgreement other)
    {
        var continuation = CompareRatio(Continuation, other.Continuation);

        if (continuation != 0)
        {
            return continuation;
        }

        var capability = CompareRatio(Capability, other.Capability);

        if (capability != 0)
        {
            return capability;
        }

        return CompareRatio(Commitment, other.Commitment);
    }

    public int CompareTransferPreference(BasisEncounterAgreement other) =>
        CompareRatio(Commitment, other.Commitment);

    public bool CanReflectAtLocalPinnedBoundary(bool hasClosureTarget) =>
        Encounter.IsExplicitPinnedBasis
        && Encounter.IsLocalToSite
        && !hasClosureTarget
        && PreservesDirection;

    public static BasisEncounterAgreement Create(
        TraversalSignature signature,
        SiteBasisEncounter encounter,
        Measure desiredDirection)
    {
        var current = desiredDirection.Sign;
        var supportsCurrent = current.Raw != 0L && encounter.SupportsDirection(current);
        var supportsOpposite = current.Raw != 0L && encounter.SupportsDirection(-current);
        var continuation = supportsCurrent
            ? new Ratio(Measure.One, Measure.One)
            : supportsOpposite
                ? new Ratio(-Measure.One, Measure.One)
                : Ratio.Zero(Measure.One);
        var capability = supportsCurrent && supportsOpposite
            ? new Ratio(Measure.One, Measure.One)
            : supportsCurrent || supportsOpposite
                ? new Ratio(Measure.One, new Measure(2))
                : Ratio.Zero(Measure.One);
        var resolution = ResolutionAgreement(signature.CalibrationRead.Potential, encounter.Potential);
        var commitment = encounter.Commitment;

        return new BasisEncounterAgreement(encounter, resolution, continuation, capability, commitment);
    }

    private static Ratio ResolutionAgreement(Measure left, Measure right)
    {
        var low = left.Raw < right.Raw ? left : right;
        var high = left.Raw < right.Raw ? right : left;

        return high == Measure.Zero
            ? Ratio.Zero(Measure.One)
            : new Ratio(low, high).Normalize();
    }

    private static int CompareRatio(Ratio left, Ratio right)
        => left.CompareValue(right);
}
