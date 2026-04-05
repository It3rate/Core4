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

    public Measure SingleValue => Origin + Preset.SingleValueOffset;

    public Measure DominantValue => Origin + Preset.DominantValueOffset;

    public Measure RecessiveValue => Origin - Preset.RecessiveValueOffset;

    public bool HasInverse => Preset.RecessiveSpan != Measure.Zero;

    public bool HasPairedValues => Preset.Controls.ShowDominantValue || Preset.Controls.ShowRecessiveValue;

    public bool IsHiddenOnHalfLine => !HasInverse && Preset.SingleValueOffset.Raw < 0;

    public Ratio HiddenTension => IsHiddenOnHalfLine
        ? Ratio.Tension(Preset.SingleValueOffset.Absolute)
        : Ratio.Zero(Measure.Zero);

    public Measure DomainMin => HasInverse ? RecessiveAnchor : Origin;

    public Measure DomainMax => DominantAnchor;

    public Measure VisualMin => DomainMin - Margin();

    public Measure VisualMax => DomainMax + Margin();

    public Ratio DominantReading => HasPairedValues
        ? DominantBasis.Project(DominantValue)
        : DominantBasis.Project(SingleValue);

    public Ratio RecessiveReading => !HasInverse
        ? Ratio.Zero(Measure.Zero)
        : HasPairedValues
            ? RecessiveBasis.Project(RecessiveValue)
            : RecessiveBasis.Project(SingleValue);

    public string ComplexReading => $"{RecessiveReading.Normalize()}r+{DominantReading.Normalize()}d";

    public string ResolutionMode => !HasInverse
        ? "One-sided half-line"
        : Preset.FuseAligned
            ? "Fused directed segment"
            : "Unfused inverted pair";

    private Measure Margin()
    {
        var span = Math.Max(Preset.DominantSpan.Absolute.Raw, Preset.RecessiveSpan.Absolute.Raw);
        var margin = Math.Max(4L, (long)Math.Ceiling(span * 0.2d));
        return new Measure(margin);
    }
}
