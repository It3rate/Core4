
namespace Core4.Topology;

/// <summary>
/// A numeric read of how the trolley's current traversal signature meets one local basis encounter. It is descriptive
/// for now: it does not yet choose policy, but it exposes resolution compatibility, directional preservation or
/// inversion, and whether the encountered basis is one-sided or bilateral for the current directional question.
/// </summary>
public readonly record struct BasisEncounterAgreement(
    SiteBasisEncounter Encounter,
    SupportProjectionRead ResolutionRead,
    SupportProjectionRead ContinuationRead,
    SupportProjectionRead CapabilityRead,
    SupportProjectionRead CommitmentRead) : ISupportFold
{
    public BasisEncounterAgreement(
        SiteBasisEncounter encounter,
        ISupportProjection resolution,
        ISupportProjection continuation,
        ISupportProjection capability,
        ISupportProjection commitment) : this(
            encounter,
            new SupportProjectionRead(resolution.CompatibilityRatio()),
            new SupportProjectionRead(continuation.CompatibilityRatio()),
            new SupportProjectionRead(capability.CompatibilityRatio()),
            new SupportProjectionRead(commitment.CompatibilityRatio()))
    {
    }

    public ISupportFold CalibrationRead => Encounter;

    public ISupportProjection FoldRead => CalibrationRead.FoldRead;

    // TODO(core): Delete this compatibility alias once callers consume FoldRead directly.
    public Ratio Fold => FoldRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume CalibrationRead/FoldRead directly.
    public Ratio Calibration => Fold;

    public Measure Potential => CalibrationRead.Potential;

    // TODO(core): Delete this compatibility alias once callers consume ResolutionRead directly.
    public Ratio Resolution => ResolutionRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume ContinuationRead directly.
    public Ratio Continuation => ContinuationRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume CapabilityRead directly.
    public Ratio Capability => CapabilityRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume CommitmentRead directly.
    public Ratio Commitment => CommitmentRead.CompatibilityRatio();

    public ISupportProjection RankRead => ResolutionRead
        .AddRead(ContinuationRead)
        .AddRead(CapabilityRead)
        .AddRead(CommitmentRead);

    // TODO(core): Delete this compatibility alias once callers consume RankRead directly.
    public Ratio Rank => RankRead.CompatibilityRatio();

    public bool PreservesDirection => Continuation == new Ratio(Measure.One, Measure.One);

    public bool InvertsDirection => Continuation == new Ratio(-Measure.One, Measure.One);

    public bool BlocksDirection => Continuation == Ratio.Zero(Measure.One);

    public bool IsBilateral => Capability == new Ratio(Measure.One, Measure.One);

    public bool IsOneSided => Capability == new Ratio(Measure.One, new Measure(2));

    public bool HasCommittedTransfer => CommitmentRead.Potential != Measure.Zero;

    public ISupportProjection TransmissionRead => CommitmentRead.CanonicalizeRead();

    // TODO(core): Delete this compatibility alias once callers consume TransmissionRead directly.
    public Ratio Transmission => TransmissionRead.CompatibilityRatio();

    public ISupportProjection RetentionRead =>
        new SupportProjectionRead(new Ratio(Measure.One, Measure.One)).SubtractRead(CommitmentRead);

    // TODO(core): Delete this compatibility alias once callers consume RetentionRead directly.
    public Ratio Retention => RetentionRead.CompatibilityRatio();

    public int ComparePreference(BasisEncounterAgreement other)
    {
        var traversal = CompareTraversalPreference(other);

        if (traversal != 0)
        {
            return traversal;
        }

        return CompareRead(ResolutionRead, other.ResolutionRead);
    }

    public int CompareTraversalPreference(BasisEncounterAgreement other)
    {
        var continuation = CompareRead(ContinuationRead, other.ContinuationRead);

        if (continuation != 0)
        {
            return continuation;
        }

        var capability = CompareRead(CapabilityRead, other.CapabilityRead);

        if (capability != 0)
        {
            return capability;
        }

        return CompareRead(CommitmentRead, other.CommitmentRead);
    }

    public int CompareTransferPreference(BasisEncounterAgreement other) =>
        CompareRead(CommitmentRead, other.CommitmentRead);

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
        var resolution = ResolutionAgreementRead(signature.CalibrationRead.Potential, encounter.Potential);
        var commitment = encounter.CommitmentRead;

        return new BasisEncounterAgreement(encounter, resolution, continuation, capability, commitment);
    }

    private static ISupportProjection ResolutionAgreementRead(Measure left, Measure right)
    {
        var low = left.Raw < right.Raw ? left : right;
        var high = left.Raw < right.Raw ? right : left;

        return new SupportProjectionRead(high == Measure.Zero
            ? Ratio.Zero(Measure.One)
            : new Ratio(low, high).Canonicalize());
    }

    private static int CompareRead(ISupportProjection left, ISupportProjection right)
        => left.CompareValueRead(right);
}
