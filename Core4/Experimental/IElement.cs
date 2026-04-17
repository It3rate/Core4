namespace Core4.Experimental;

/// <summary>
/// Minimal experimental endpoint for structural freezing. Every experimental element exposes a polarity, even if it
/// is currently neutral.
/// </summary>
public interface IElement
{
    Measure Polarity { get; }
}
