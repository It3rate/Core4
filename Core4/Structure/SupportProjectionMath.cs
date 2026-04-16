using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// Shared compatibility math for support-shaped reads. This keeps the current ratio-backed folding law in one place
/// so support-first callers can stay on read surfaces longer without repeating ad hoc <c>AsRatio()</c> plumbing.
/// </summary>
public static class SupportProjectionMath
{
    public static Ratio NormalizedRatio(this ISupportProjection projection) => projection.AsRatio().Normalize();

    public static SupportProjectionRead NormalizeRead(this ISupportProjection projection) => new(projection.NormalizedRatio());

    public static SupportProjectionRead MultiplyRead(this ISupportProjection projection, Ratio factor) =>
        new(projection.AsRatio().Multiply(factor).Canonicalize());
}
