using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A support-level read that exposes whole-structure potential together with the current fold comparing its ordered
/// substructure. Unlike a placement projection, a fold can remain meaningful for composite supports and attachments.
/// </summary>
public interface ISupportFold
{
    Measure Potential { get; }

    Ratio Fold { get; }
}
