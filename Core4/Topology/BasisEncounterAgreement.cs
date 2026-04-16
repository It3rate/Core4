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
    Ratio Commitment)
{
    public ISupportFold CalibrationRead => Encounter;

    public Ratio Calibration => CalibrationRead.Fold.Normalize();

    public Measure Potential => CalibrationRead.Potential;

    public Ratio Rank => Resolution.Add(Continuation).Add(Capability).Add(Commitment).Normalize();

    public bool PreservesDirection => Continuation == new Ratio(Measure.One, Measure.One);

    public bool InvertsDirection => Continuation == new Ratio(-Measure.One, Measure.One);

    public bool BlocksDirection => Continuation == Ratio.Zero(Measure.One);

    public bool IsBilateral => Capability == new Ratio(Measure.One, Measure.One);

    public bool IsOneSided => Capability == new Ratio(Measure.One, new Measure(2));

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
        var resolution = ResolutionAgreement(signature.Calibration.Potential, encounter.Potential);
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
}
