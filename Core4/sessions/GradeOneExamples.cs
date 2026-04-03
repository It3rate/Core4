using System.Collections.Immutable;
using Core4.Engine;

namespace Core4.Sessions;

public static class GradeOneExamples
{
    public static ImmutableArray<(string Name, Basis Basis)> Create()
    {
        var x = new Carrier(1);
        var y = new Carrier(2);

        var xRecessive = Basis.Freeze(new Pin(x, 0, -10));
        var xDominant = Basis.Freeze(new Pin(x, 0, 10));
        var xDominantWide = Basis.Freeze(new Pin(x, 0, 20));

        var yRecessive = Basis.Freeze(new Pin(y, 0, -10));
        var yDominant = Basis.Freeze(new Pin(y, 0, 10));

        return
        [
            ("self", Basis.Fuse(xDominant, xDominantWide)),
            ("segment", Basis.Fuse(xRecessive, xDominant)),
            ("rr", Basis.Fuse(xRecessive, yRecessive)),
            ("rd", Basis.Fuse(xRecessive, yDominant)),
            ("dr", Basis.Fuse(xDominant, yRecessive)),
            ("dd", Basis.Fuse(xDominant, yDominant))
        ];
    }

    public static ImmutableArray<string> Describe()
    {
        var examples = Create();
        var descriptions = ImmutableArray.CreateBuilder<string>(examples.Length);

        foreach (var (name, basis) in examples)
        {
            descriptions.Add(
                $"{name}: grade={basis.Grade.Raw}, pins={basis.PinCount.Raw}, carriers={basis.CarrierCount.Raw}, " +
                $"sameCarrier={basis.IsSameCarrierPair()}, aligned={basis.IsAlignedPair()}, opposed={basis.IsOpposedPair()}, kernel={basis.KernelCode().Raw}");
        }

        return descriptions.MoveToImmutable();
    }
}
