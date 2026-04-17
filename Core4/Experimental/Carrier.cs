namespace Core4.Experimental;

/// <summary>
/// Minimal experimental carrier identity. Domain stays numeric and optional so relation inspection can decide whether
/// an attached element introduces a new carrier or only a new carrier inside an existing domain.
/// </summary>
public readonly record struct Carrier(long Id, long Domain = 0, long PolarityRaw = 0) : IElement
{
    public Measure Polarity => new(PolarityRaw);

    public override string ToString() => Domain == 0 ? Id.ToString() : $"{Domain}:{Id}";
}
