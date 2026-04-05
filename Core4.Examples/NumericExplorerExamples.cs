using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

/// <summary>
/// Reusable builders and presets for the numeric explorer UI.
/// </summary>
public static class NumericExplorerExamples
{
    public static ImmutableArray<NumericExplorerPreset> Presets() =>
    [
        new(
            "Dominant Only",
            "A one-sided interval that reads only to the dominant side.",
            0,
            10,
            0,
            7,
            0,
            0,
            0,
            0,
            false,
            new NumericExplorerControls(
                ShowDominantSpan: true,
                ShowRecessiveSpan: false,
                ShowValueOffset: true,
                ShowResidual: false,
                ShowTopSignal: false,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                ValueMin: 0,
                ValueMax: 24)),
        new(
            "Inverted Pair",
            "Dominant and recessive intervals can coexist before they are frozen into one fused basis.",
            0,
            10,
            10,
            -4,
            0,
            0,
            0,
            0,
            false,
            new NumericExplorerControls(
                ShowDominantSpan: true,
                ShowRecessiveSpan: true,
                ShowValueOffset: true,
                ShowResidual: false,
                ShowTopSignal: false,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                ValueMin: -24,
                ValueMax: 24)),
        new(
            "Frozen Segment",
            "The opposed pair is now frozen and fused, so the same horizontal structure becomes a basis.",
            0,
            10,
            10,
            -4,
            0,
            0,
            0,
            0,
            true,
            new NumericExplorerControls(
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowValueOffset: true,
                ShowResidual: false,
                ShowTopSignal: false,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                ValueMin: -24,
                ValueMax: 24)),
        new(
            "Orthogonal Output",
            "Orthogonal tension can publish upward while the aligned segment remains fixed in place.",
            0,
            10,
            10,
            5,
            6,
            6,
            0,
            0,
            true,
            new NumericExplorerControls(
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowValueOffset: true,
                ShowResidual: true,
                ShowTopSignal: true,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                ValueMin: -24,
                ValueMax: 24)),
        new(
            "Orthogonal Input",
            "Orthogonal input arrives from below while the same site is already publishing upward.",
            0,
            10,
            10,
            5,
            7,
            6,
            5,
            0,
            true,
            new NumericExplorerControls(
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowValueOffset: true,
                ShowResidual: true,
                ShowTopSignal: true,
                ShowBottomSignal: true,
                ShowBottomSignal2: false,
                ValueMin: -24,
                ValueMax: 24)),
        new(
            "Multiple Inputs",
            "Several bottom-side inputs can branch into the same site while the aligned basis stays simple.",
            0,
            10,
            10,
            5,
            8,
            6,
            4,
            7,
            true,
            new NumericExplorerControls(
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowValueOffset: true,
                ShowResidual: true,
                ShowTopSignal: true,
                ShowBottomSignal: true,
                ShowBottomSignal2: true,
                ValueMin: -24,
                ValueMax: 24))
    ];

    public static NumericExplorerSnapshot Build(NumericExplorerPreset preset)
    {
        var main = new Carrier(1);
        var dominantBasis = Basis.Freeze(main, preset.Origin, preset.Origin + preset.DominantSpan);
        var recessiveBasis = Basis.Freeze(main, preset.Origin, preset.Origin - preset.RecessiveSpan);
        var displayBasis = preset.FuseAligned && preset.RecessiveSpan != Measure.Zero
            ? Basis.Fuse(recessiveBasis, dominantBasis)
            : dominantBasis;

        var activeProjection = Project(preset, dominantBasis, recessiveBasis);
        var channels = Channels(preset);
        var source = CreateSourceSite(preset, channels);
        var resolved = source.Resolve(activeProjection.IsTension ? Ratio.Tension(activeProjection.Potential) : activeProjection);
        var settlement = resolved.Settle();

        return new NumericExplorerSnapshot(
            preset,
            channels,
            source,
            resolved,
            settlement,
            dominantBasis,
            recessiveBasis,
            displayBasis,
            activeProjection);
    }

    private static Site CreateSourceSite(
        NumericExplorerPreset preset,
        ImmutableArray<NumericExplorerChannel> channels)
    {
        var site = Site.Open(new Carrier(1), preset.Origin)
            .Anchor(new Ratio(Measure.Zero, Measure.One))
            .WithResidual(Ratio.Tension(preset.Residual));

        foreach (var channel in channels)
        {
            if (channel.Value.Potential == Measure.Zero && channel.Value.Dominant == Measure.Zero)
            {
                continue;
            }

            site = site.Connect(channel.ToFlow());
        }

        return site;
    }

    private static ImmutableArray<NumericExplorerChannel> Channels(NumericExplorerPreset preset) =>
    [
        new("Out", new Carrier(2), Ratio.Tension(preset.TopSignal), Provenance.SelfDriven(preset.TopSignal)),
        new("In A", new Carrier(3), Ratio.Tension(preset.BottomSignal), Provenance.OtherDriven(preset.BottomSignal)),
        new("In B", new Carrier(4), Ratio.Tension(preset.BottomSignal2), Provenance.OtherDriven(preset.BottomSignal2))
    ];

    private static Ratio Project(
        NumericExplorerPreset preset,
        Basis dominantBasis,
        Basis recessiveBasis)
    {
        var value = preset.Origin + preset.ValueOffset;

        if (preset.ValueOffset.Raw < 0)
        {
            return preset.RecessiveSpan == Measure.Zero
                ? Ratio.Tension(preset.ValueOffset.Absolute)
                : recessiveBasis.Project(value);
        }

        return dominantBasis.Project(value);
    }
}
