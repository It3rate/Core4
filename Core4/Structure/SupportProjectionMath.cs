using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// Shared transitional math for support-shaped reads. This keeps the current ratio-backed compatibility fold in one
/// place so support-first callers can stay on read surfaces longer without repeating ad hoc <c>AsRatio()</c>
/// plumbing while the richer read algebra is still being pulled out of <see cref="Ratio"/>.
/// </summary>
public static class SupportProjectionMath
{
    public static SupportProjectionRead ToSupportRead(this ISupportProjection projection) =>
        new(projection.CompatibilityRatio().Normalize());

    public static Ratio CompatibilityRatio(this ISupportProjection projection) => projection.AsRatio();

    public static SupportProjectionRead CanonicalizeRead(this ISupportProjection projection) =>
        new(projection.CompatibilityRatio().Canonicalize());

    public static SupportProjectionRead AddRead(this ISupportProjection projection, ISupportProjection other) =>
        new(projection.CompatibilityRatio().Add(other.CompatibilityRatio()).Canonicalize());

    public static SupportProjectionRead SubtractRead(this ISupportProjection projection, ISupportProjection other) =>
        new(projection.CompatibilityRatio().Subtract(other.CompatibilityRatio()).Canonicalize());

    public static SupportProjectionRead MultiplyRead(this ISupportProjection projection, ISupportProjection factor) =>
        new(projection.CompatibilityRatio().Multiply(factor.CompatibilityRatio()).Canonicalize());

    public static int CompareValueRead(this ISupportProjection projection, ISupportProjection other) =>
        projection.CompatibilityRatio().CompareValue(other.CompatibilityRatio());

    // TODO(core): Delete this misleading compatibility name once callers use CompatibilityRatio or read-first math.
    public static Ratio NormalizedRatio(this ISupportProjection projection) => projection.CompatibilityRatio().Normalize();

    // TODO(core): Delete this misleading compatibility name once callers use CanonicalizeRead or richer read math.
    public static SupportProjectionRead NormalizeRead(this ISupportProjection projection) => CanonicalizeRead(projection);

    // TODO(core): Delete this overload once callers pass support-shaped factors directly.
    public static SupportProjectionRead MultiplyRead(this ISupportProjection projection, Ratio factor) =>
        MultiplyRead(projection, (ISupportProjection)factor);
}
