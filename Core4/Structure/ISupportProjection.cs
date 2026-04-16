using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A support-derived read that can still be used as a placement or traversal coordinate. The current engine uses
/// <see cref="Ratio"/> as the compatibility projection, but support-first code should depend on this interface where
/// possible rather than assuming every projection is primitive ratio forever.
/// </summary>
public interface ISupportProjection
{
    Measure Potential { get; }

    Ratio AsRatio();
}
