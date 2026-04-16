using System.Globalization;

namespace Core4.Structure;

/// <summary>
/// One semantic family of carriers, such as temperature or distance. Several carrier instances may belong to the same
/// domain without already agreeing on origin, scale, or calibration.
/// Example: <c>var temperature = new CarrierDomain(7);</c>
/// </summary>
public readonly record struct CarrierDomain(long Id)
{
    public static implicit operator CarrierDomain(long id) => new(id);

    public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
}
