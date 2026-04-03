using System.Globalization;

namespace Core4.Engine;

public readonly record struct Carrier(long Id)
{
    public static implicit operator Carrier(long id) => new(id);

    public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
}
