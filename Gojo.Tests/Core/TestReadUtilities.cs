using Gojo.Core;

namespace Gojo.Tests.Core;

internal static class TestReadUtilities
{
    public static Relation ResidualLoadOn(this Relation result, Carrier reader)
    {
        var reverse = 0UL;
        var forward = 0UL;
        foreach (var residual in result.ResidualsOn(reader))
        {
            var read = residual.ReadFrom(reader);
            if (read.Direction > 0)
            {
                forward += read.Magnitude;
            }
            else if (read.Direction < 0)
            {
                reverse += read.Magnitude;
            }
        }

        return new(
            new CarrierLocation(reader.Inverse(), reverse),
            new CarrierLocation(reader, forward));
    }
}
