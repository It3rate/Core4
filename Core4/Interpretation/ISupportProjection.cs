
namespace Core4.Interpretation;

/// <summary>
/// A support-derived read that can still be used as a placement or traversal coordinate. The current engine uses
/// <see cref="SupportProjectionRead"/> as the support-shaped wrapper, while <see cref="Ratio"/> remains the current
/// compatibility projection. Support-first code should depend on this interface where possible rather than assuming
/// every projection is primitive ratio forever.
/// </summary>
public interface ISupportProjection
{
    Measure Potential { get; }

    // TODO(core): Delete this compatibility fold once support reads no longer need to collapse back to Ratio.
    Ratio AsRatio();
}

