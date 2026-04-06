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
            4,
            0,
            10,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            false,
            new NumericExplorerControls(
                ShowFusedSpan: false,
                ShowDominantSpan: true,
                ShowRecessiveSpan: false,
                ShowSingleValue: true,
                ShowDominantValue: false,
                ShowRecessiveValue: false,
                ShowAlignedControls: true,
                ShowTension: false,
                ShowTopSignal: false,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                SingleValueMin: 0,
                SingleValueMax: 24,
                PairValueMin: 0,
                PairValueMax: 24)),
        new(
            "Inverted Pair",
            "Dominant and recessive intervals can coexist before they are frozen into one fused basis.",
            0,
            4,
            4,
            0,
            10,
            8,
            0,
            0,
            0,
            0,
            0,
            0,
            false,
            new NumericExplorerControls(
                ShowFusedSpan: false,
                ShowDominantSpan: true,
                ShowRecessiveSpan: true,
                ShowSingleValue: false,
                ShowDominantValue: true,
                ShowRecessiveValue: true,
                ShowAlignedControls: true,
                ShowTension: false,
                ShowTopSignal: false,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                SingleValueMin: -24,
                SingleValueMax: 24,
                PairValueMin: -24,
                PairValueMax: 24)),
        new(
            "Frozen Segment",
            "The opposed pair is now frozen and fused, so the same horizontal structure becomes a basis.",
            0,
            4,
            4,
            0,
            10,
            8,
            0,
            0,
            0,
            0,
            0,
            0,
            true,
            new NumericExplorerControls(
                ShowFusedSpan: true,
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowSingleValue: false,
                ShowDominantValue: true,
                ShowRecessiveValue: true,
                ShowAlignedControls: true,
                ShowTension: false,
                ShowTopSignal: false,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                SingleValueMin: -24,
                SingleValueMax: 24,
                PairValueMin: -24,
                PairValueMax: 24)),
        new(
            "Orthogonal Output",
            "Orthogonal tension can publish upward while the aligned segment remains fixed in place.",
            0,
            4,
            4,
            0,
            10,
            8,
            6,
            6,
            0,
            0,
            0,
            0,
            true,
            new NumericExplorerControls(
                ShowFusedSpan: true,
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowSingleValue: false,
                ShowDominantValue: true,
                ShowRecessiveValue: true,
                ShowAlignedControls: false,
                ShowTension: true,
                ShowTopSignal: true,
                ShowBottomSignal: false,
                ShowBottomSignal2: false,
                SingleValueMin: -24,
                SingleValueMax: 24,
                PairValueMin: -24,
                PairValueMax: 24)),
        new(
            "Orthogonal Input",
            "Orthogonal input arrives from below while the same site is already publishing upward.",
            0,
            4,
            4,
            0,
            10,
            8,
            7,
            6,
            5,
            0,
            5,
            0,
            true,
            new NumericExplorerControls(
                ShowFusedSpan: true,
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowSingleValue: false,
                ShowDominantValue: true,
                ShowRecessiveValue: true,
                ShowAlignedControls: false,
                ShowTension: true,
                ShowTopSignal: true,
                ShowBottomSignal: true,
                ShowBottomSignal2: false,
                SingleValueMin: -24,
                SingleValueMax: 24,
                PairValueMin: -24,
                PairValueMax: 24)),
        new(
            PrimitiveBasisExamples.PresetName,
            "Six primitive fused forms built from one aligned pair and one orthogonal pair.",
            0,
            8,
            8,
            0,
            16,
            8,
            0,
            16,
            8,
            0,
            0,
            0,
            true,
            new NumericExplorerControls(
                ShowFusedSpan: false,
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowSingleValue: false,
                ShowDominantValue: true,
                ShowRecessiveValue: true,
                ShowAlignedControls: false,
                ShowTension: false,
                ShowTopSignal: true,
                ShowBottomSignal: true,
                ShowBottomSignal2: false,
                SingleValueMin: 0,
                SingleValueMax: 24,
                PairValueMin: -24,
                PairValueMax: 24)),
        new(
            "Multiple Inputs",
            "Several bottom-side inputs can branch into the same site while the aligned basis stays simple.",
            0,
            4,
            4,
            0,
            10,
            8,
            8,
            6,
            4,
            6,
            4,
            6,
            true,
            new NumericExplorerControls(
                ShowFusedSpan: true,
                ShowDominantSpan: false,
                ShowRecessiveSpan: false,
                ShowSingleValue: false,
                ShowDominantValue: true,
                ShowRecessiveValue: true,
                ShowAlignedControls: false,
                ShowTension: true,
                ShowTopSignal: true,
                ShowBottomSignal: true,
                ShowBottomSignal2: true,
                SingleValueMin: -24,
                SingleValueMax: 24,
                PairValueMin: -24,
                PairValueMax: 24))
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
            .WithResidual(Ratio.Tension(new Measure(preset.Tension.Raw + preset.BottomTension.Raw + preset.BottomTension2.Raw)));

        foreach (var channel in channels)
        {
            if (channel.Value.Potential == Measure.Zero && channel.Value.Dominant == Measure.Zero && channel.Tension == Measure.Zero)
            {
                continue;
            }

            site = site.Connect(channel.ToFlow());
        }

        return site;
    }

    private static ImmutableArray<NumericExplorerChannel> Channels(NumericExplorerPreset preset)
    {
        var channels = ImmutableArray.CreateBuilder<NumericExplorerChannel>(3);

        if (preset.Controls.ShowTopSignal)
        {
            channels.Add(new(
                "Out",
                new Carrier(2),
                Ratio.Tension(preset.TopSignal),
                preset.Tension,
                Provenance.SelfDriven(preset.TopSignal),
                0,
                1));
        }

        if (preset.Controls.ShowBottomSignal2)
        {
            channels.Add(new(
                "In A",
                new Carrier(3),
                Ratio.Tension(preset.BottomSignal),
                preset.BottomTension,
                Provenance.OtherDriven(preset.BottomSignal),
                0,
                2));
            channels.Add(new(
                "In B",
                new Carrier(4),
                Ratio.Tension(preset.BottomSignal2),
                preset.BottomTension2,
                Provenance.OtherDriven(preset.BottomSignal2),
                1,
                2));
        }
        else if (preset.Controls.ShowBottomSignal)
        {
            channels.Add(new(
                "In",
                new Carrier(3),
                Ratio.Tension(preset.BottomSignal),
                preset.BottomTension,
                Provenance.OtherDriven(preset.BottomSignal),
                0,
                1));
        }

        return channels.ToImmutable();
    }

    private static Ratio Project(
        NumericExplorerPreset preset,
        Basis dominantBasis,
        Basis recessiveBasis)
    {
        if (preset.Controls.ShowDominantValue || preset.Controls.ShowRecessiveValue)
        {
            var dominant = preset.DominantValueOffset == Measure.Zero
                ? Ratio.Zero(Measure.Zero)
                : dominantBasis.Project(preset.Origin + preset.DominantValueOffset);
            var recessive = preset.RecessiveValueOffset == Measure.Zero
                ? Ratio.Zero(Measure.Zero)
                : recessiveBasis.Project(preset.Origin - preset.RecessiveValueOffset);

            return recessive.Add(dominant);
        }

        var value = preset.Origin + preset.SingleValueOffset;

        if (preset.SingleValueOffset.Raw < 0)
        {
            return preset.RecessiveSpan == Measure.Zero
                ? Ratio.Tension(preset.SingleValueOffset.Absolute)
                : recessiveBasis.Project(value);
        }

        return dominantBasis.Project(value);
    }
}
