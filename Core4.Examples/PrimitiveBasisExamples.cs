using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

/// <summary>
/// Builds the six primitive fused forms from the same Core4 basis machinery used by the numeric explorer.
/// </summary>
public static class PrimitiveBasisExamples
{
    public const string PresetName = "Primitive Bases";

    private static readonly Measure UnitSpan = new(8);

    public static PrimitiveBasisSnapshot Build(NumericExplorerPreset preset)
    {
        var horizontal = new Carrier(11);
        var vertical = new Carrier(12);
        var origin = Measure.Zero;

        var dominantBasis = Basis.Freeze(horizontal, origin, UnitSpan);
        var recessiveBasis = Basis.Freeze(horizontal, origin, -UnitSpan);
        var topBasis = Basis.Freeze(vertical, origin, UnitSpan);
        var bottomBasis = Basis.Freeze(vertical, origin, -UnitSpan);

        var dominantValue = dominantBasis.Project(preset.DominantValueOffset);
        var recessiveValue = recessiveBasis.Project(-preset.RecessiveValueOffset);
        var topValue = dominantValue;
        var bottomValue = recessiveValue;

        var self = dominantValue.Multiply(recessiveValue).Normalize();
        var directed = dominantValue.Subtract(recessiveValue).Normalize();
        var area = dominantValue.Multiply(topValue).Normalize();
        var antiArea = recessiveValue.Multiply(bottomValue).Normalize();
        var antiComplex = recessiveValue.Multiply(topValue).Normalize();
        var complex = dominantValue.Multiply(bottomValue).Normalize();

        return new PrimitiveBasisSnapshot(
            preset,
            dominantBasis,
            recessiveBasis,
            topBasis,
            bottomBasis,
            Basis.Fuse(dominantBasis, dominantBasis),
            Basis.Fuse(recessiveBasis, dominantBasis),
            Basis.Fuse(dominantBasis, topBasis),
            Basis.Fuse(recessiveBasis, bottomBasis),
            Basis.Fuse(recessiveBasis, topBasis),
            Basis.Fuse(dominantBasis, bottomBasis),
            dominantValue,
            recessiveValue,
            topValue,
            bottomValue,
            self,
            directed,
            area,
            antiArea,
            antiComplex,
            complex);
    }

}
