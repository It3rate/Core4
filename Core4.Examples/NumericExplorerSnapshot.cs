using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

/// <summary>
/// Fully built numeric explorer state derived from one preset.
/// </summary>
public sealed record NumericExplorerSnapshot(
    NumericExplorerPreset Preset,
    ImmutableArray<NumericExplorerChannel> Channels,
    Site SourceSite,
    Site ResolvedSite,
    SiteSettlement Settlement,
    Basis DominantBasis,
    Basis RecessiveBasis,
    Basis DisplayBasis,
    Ratio ActiveProjection)
{
    public Carrier MainCarrier => new(1);

    public Measure Origin => Preset.Origin;

    public Measure DominantAnchor => Origin + Preset.DominantSpan;

    public Measure RecessiveAnchor => Origin - Preset.RecessiveSpan;

    public Measure Value => Origin + Preset.ValueOffset;

    public bool HasInverse => Preset.RecessiveSpan != Measure.Zero;

    public bool IsHiddenOnHalfLine => !HasInverse && Preset.ValueOffset.Raw < 0;

    public Ratio HiddenTension => IsHiddenOnHalfLine
        ? Ratio.Tension(Preset.ValueOffset.Absolute)
        : Ratio.Zero(Measure.Zero);

    public Measure DomainMin => HasInverse ? RecessiveAnchor : Origin;

    public Measure DomainMax => DominantAnchor;

    public Measure VisualMin
    {
        get
        {
            var margin = Margin();

            if (HasInverse)
            {
                return DomainMin - margin;
            }

            return Origin - margin;
        }
    }

    public Measure VisualMax
    {
        get
        {
            return DomainMax + Margin();
        }
    }

    public Ratio DominantReading => DominantBasis.Project(Value);

    public Ratio RecessiveReading => HasInverse ? RecessiveBasis.Project(Value) : Ratio.Zero(Measure.Zero);

    public string ResolutionMode => !HasInverse
        ? "One-sided half-line"
        : Preset.FuseAligned
            ? "Fused directed segment"
            : "Unfused inverted pair";

    private Measure Margin()
    {
        var span = Math.Max(Preset.DominantSpan.Absolute.Raw, Preset.RecessiveSpan.Absolute.Raw);
        var margin = Math.Max(3L, (long)Math.Ceiling(span * 0.2d));
        return new Measure(margin);
    }
}
