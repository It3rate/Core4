using System.Collections.Immutable;
using Core4.Engine;

namespace Core4.Sessions;

public static class GradeOneExamples
{
    public static ImmutableArray<(string Name, Basis Basis)> CreateAtomic()
    {
        var x = new Carrier(1);

        return
        [
            ("atomic", Basis.Freeze(new Relation(Site.Open(x, 0), Site.Open(x, 10)))),
            ("atomic-shifted", Basis.Freeze(new Relation(Site.Open(x, 100), Site.Open(x, 110))))
        ];
    }

    public static ImmutableArray<(string Name, Basis Basis)> Create()
    {
        var x = new Carrier(1);
        var y = new Carrier(2);

        var xRecessive = Basis.Freeze(new Relation(Site.Open(x, 0), Site.Open(x, -10)));
        var xDominant = Basis.Freeze(new Relation(Site.Open(x, 0), Site.Open(x, 10)));
        var xDominantWide = Basis.Freeze(new Relation(Site.Open(x, 0), Site.Open(x, 20)));

        var yRecessive = Basis.Freeze(new Relation(Site.Open(y, 0), Site.Open(y, -10)));
        var yDominant = Basis.Freeze(new Relation(Site.Open(y, 0), Site.Open(y, 10)));

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

    public static ImmutableArray<(string Name, Basis Basis)> CreateShiftedOrigins()
    {
        var x = new Carrier(1);
        var y = new Carrier(2);

        var xRecessive = Basis.Freeze(new Relation(Site.Open(x, 100), Site.Open(x, 90)));
        var xDominant = Basis.Freeze(new Relation(Site.Open(x, 100), Site.Open(x, 110)));
        var xDominantWide = Basis.Freeze(new Relation(Site.Open(x, 100), Site.Open(x, 120)));

        var yRecessive = Basis.Freeze(new Relation(Site.Open(y, 300), Site.Open(y, 290)));
        var yDominant = Basis.Freeze(new Relation(Site.Open(y, 300), Site.Open(y, 310)));

        return
        [
            ("self-shifted", Basis.Fuse(xDominant, xDominantWide)),
            ("segment-shifted", Basis.Fuse(xRecessive, xDominant)),
            ("rr-shifted", Basis.Fuse(xRecessive, yRecessive)),
            ("rd-shifted", Basis.Fuse(xRecessive, yDominant)),
            ("dr-shifted", Basis.Fuse(xDominant, yRecessive)),
            ("dd-shifted", Basis.Fuse(xDominant, yDominant))
        ];
    }

    public static ImmutableArray<string> Describe()
    {
        var examples = Create();
        var descriptions = ImmutableArray.CreateBuilder<string>(examples.Length);

        foreach (var (name, basis) in examples)
        {
            descriptions.Add(
                $"{name}: grade={basis.Grade.Raw}, relations={basis.RelationCount.Raw}, carriers={basis.CarrierCount.Raw}, " +
                $"sameCarrier={basis.IsSameCarrierPair()}, aligned={basis.IsAlignedPair()}, opposed={basis.IsOpposedPair()}, kernel={basis.KernelCode().Raw}, " +
                $"potential={basis.Potential.Raw}, ratio={basis.Ratio.Normalize()}, supportCarrier={basis.PrimaryCarrier.Id}");
        }

        return descriptions.MoveToImmutable();
    }

    public static ImmutableArray<string> DescribeAtomic()
    {
        var examples = CreateAtomic();
        var descriptions = ImmutableArray.CreateBuilder<string>(examples.Length);

        foreach (var (name, basis) in examples)
        {
            descriptions.Add(
                $"{name}: grade={basis.Grade.Raw}, relations={basis.RelationCount.Raw}, carriers={basis.CarrierCount.Raw}, " +
                $"potential={basis.Potential.Raw}, ratio={basis.Ratio.Normalize()}, orientation={basis.Orientation.Raw}, supportCarrier={basis.PrimaryCarrier.Id}");
        }

        return descriptions.MoveToImmutable();
    }

    public static ImmutableArray<string> DescribeShiftedOrigins()
    {
        var examples = CreateShiftedOrigins();
        var descriptions = ImmutableArray.CreateBuilder<string>(examples.Length);

        foreach (var (name, basis) in examples)
        {
            descriptions.Add(
                $"{name}: grade={basis.Grade.Raw}, relations={basis.RelationCount.Raw}, carriers={basis.CarrierCount.Raw}, " +
                $"sameCarrier={basis.IsSameCarrierPair()}, aligned={basis.IsAlignedPair()}, opposed={basis.IsOpposedPair()}, kernel={basis.KernelCode().Raw}, " +
                $"potential={basis.Potential.Raw}, ratio={basis.Ratio.Normalize()}, supportCarrier={basis.PrimaryCarrier.Id}");
        }

        return descriptions.MoveToImmutable();
    }
}
