using System;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// First-pass locked-invariant explorer state. Each primitive family keeps one revealed property fixed and lets one
/// participant be perturbed while the compatible counterpart is re-derived.
/// </summary>
public readonly record struct LockedInvariantDemoState(
    InvariantPair Self,
    InvariantPair Segment,
    InvariantPair Area,
    InvariantPair AntiArea,
    InvariantPair Complex,
    InvariantPair AntiComplex)
{
    private static readonly Measure Resolution = new(64);
    private static readonly Carrier MagnitudeCarrier = new(61);
    private static readonly Basis MagnitudeBasis = Basis.Freeze(MagnitudeCarrier, Measure.Zero, Resolution);

    private static readonly PairRange SelfRange = new(1.5d, 4.0d);
    private static readonly PairRange SegmentRange = new(1.0d, 4.0d);
    private static readonly PairRange AreaRange = new(2.25d, 4.0d);
    private static readonly PairRange AntiAreaRange = new(1.0d, 4.0d);
    private static readonly PairRange ComplexRange = new(0.5d, 3.5d);

    public static LockedInvariantDemoState Default => new(
        new InvariantPair(Ticks(3d), Ticks(2d)),
        new InvariantPair(Ticks(2d), Ticks(3d)),
        new InvariantPair(Ticks(3d), Ticks(3d)),
        new InvariantPair(Ticks(2d), Ticks(2d)),
        new InvariantPair(Ticks(3d), Ticks(2d)),
        new InvariantPair(Ticks(2d), Ticks(3d)));

    public Ratio SelfPrimaryValue => Magnitude(Self.PrimaryTicks);

    public Ratio SelfSecondaryValue => Magnitude(Self.SecondaryTicks);

    public Ratio SegmentRecessiveValue => Magnitude(Segment.PrimaryTicks);

    public Ratio SegmentDominantValue => Magnitude(Segment.SecondaryTicks);

    public Ratio AreaHorizontalValue => Magnitude(Area.PrimaryTicks);

    public Ratio AreaVerticalValue => Magnitude(Area.SecondaryTicks);

    public Ratio AntiAreaHorizontalValue => Magnitude(AntiArea.PrimaryTicks);

    public Ratio AntiAreaVerticalValue => Magnitude(AntiArea.SecondaryTicks);

    public Ratio ComplexHorizontalValue => Magnitude(Complex.PrimaryTicks);

    public Ratio ComplexVerticalValue => Magnitude(Complex.SecondaryTicks);

    public Ratio AntiComplexHorizontalValue => Magnitude(AntiComplex.PrimaryTicks);

    public Ratio AntiComplexVerticalValue => Magnitude(AntiComplex.SecondaryTicks);

    public Ratio SelfInvariant => SelfPrimaryValue.Multiply(SelfSecondaryValue).Normalize();

    public Ratio SegmentInvariant => SegmentRecessiveValue.Add(SegmentDominantValue).Normalize();

    public Ratio AreaInvariant => AreaHorizontalValue.Multiply(AreaVerticalValue).Normalize();

    public Ratio AntiAreaInvariant => AntiAreaHorizontalValue.Multiply(AntiAreaVerticalValue).Normalize();

    public Ratio ComplexNormSquared => ComplexHorizontalValue.Multiply(ComplexHorizontalValue)
        .Add(ComplexVerticalValue.Multiply(ComplexVerticalValue))
        .Normalize();

    public Ratio AntiComplexNormSquared => AntiComplexHorizontalValue.Multiply(AntiComplexHorizontalValue)
        .Add(AntiComplexVerticalValue.Multiply(AntiComplexVerticalValue))
        .Normalize();

    public double SelfPrimaryUnits => Units(Self.PrimaryTicks);

    public double SelfSecondaryUnits => Units(Self.SecondaryTicks);

    public double SegmentRecessiveUnits => Units(Segment.PrimaryTicks);

    public double SegmentDominantUnits => Units(Segment.SecondaryTicks);

    public double AreaHorizontalUnits => Units(Area.PrimaryTicks);

    public double AreaVerticalUnits => Units(Area.SecondaryTicks);

    public double AntiAreaHorizontalUnits => Units(AntiArea.PrimaryTicks);

    public double AntiAreaVerticalUnits => Units(AntiArea.SecondaryTicks);

    public double ComplexHorizontalUnits => Units(Complex.PrimaryTicks);

    public double ComplexVerticalUnits => Units(Complex.SecondaryTicks);

    public double AntiComplexHorizontalUnits => Units(AntiComplex.PrimaryTicks);

    public double AntiComplexVerticalUnits => Units(AntiComplex.SecondaryTicks);

    public double SelfInvariantUnits => SelfInvariant.Normalize().Approximate;

    public double SegmentInvariantUnits => SegmentInvariant.Normalize().Approximate;

    public double AreaInvariantUnits => AreaInvariant.Normalize().Approximate;

    public double AntiAreaInvariantUnits => AntiAreaInvariant.Normalize().Approximate;

    public double ComplexRadiusUnits => Math.Sqrt(ComplexNormSquared.Normalize().Approximate);

    public double AntiComplexRadiusUnits => Math.Sqrt(AntiComplexNormSquared.Normalize().Approximate);

    public LockedInvariantDemoState WithSelfPrimaryTicks(Measure ticks) => this with
    {
        Self = LockProductFromPrimary(SelfInvariant, ticks, SelfRange)
    };

    public LockedInvariantDemoState WithSelfSecondaryTicks(Measure ticks) => this with
    {
        Self = LockProductFromSecondary(SelfInvariant, ticks, SelfRange)
    };

    public LockedInvariantDemoState WithSegmentRecessiveTicks(Measure ticks) => this with
    {
        Segment = LockSumFromPrimary(SegmentInvariant, ticks, SegmentRange)
    };

    public LockedInvariantDemoState WithSegmentDominantTicks(Measure ticks) => this with
    {
        Segment = LockSumFromSecondary(SegmentInvariant, ticks, SegmentRange)
    };

    public LockedInvariantDemoState WithAreaHorizontalTicks(Measure ticks) => this with
    {
        Area = LockProductFromPrimary(AreaInvariant, ticks, AreaRange)
    };

    public LockedInvariantDemoState WithAreaVerticalTicks(Measure ticks) => this with
    {
        Area = LockProductFromSecondary(AreaInvariant, ticks, AreaRange)
    };

    public LockedInvariantDemoState WithAntiAreaHorizontalTicks(Measure ticks) => this with
    {
        AntiArea = LockProductFromPrimary(AntiAreaInvariant, ticks, AntiAreaRange)
    };

    public LockedInvariantDemoState WithAntiAreaVerticalTicks(Measure ticks) => this with
    {
        AntiArea = LockProductFromSecondary(AntiAreaInvariant, ticks, AntiAreaRange)
    };

    public LockedInvariantDemoState WithComplexHorizontalTicks(Measure ticks) => this with
    {
        Complex = LockNormFromPrimary(ComplexNormSquared, ticks, ComplexRange)
    };

    public LockedInvariantDemoState WithComplexVerticalTicks(Measure ticks) => this with
    {
        Complex = LockNormFromSecondary(ComplexNormSquared, ticks, ComplexRange)
    };

    public LockedInvariantDemoState WithAntiComplexHorizontalTicks(Measure ticks) => this with
    {
        AntiComplex = LockNormFromPrimary(AntiComplexNormSquared, ticks, ComplexRange)
    };

    public LockedInvariantDemoState WithAntiComplexVerticalTicks(Measure ticks) => this with
    {
        AntiComplex = LockNormFromSecondary(AntiComplexNormSquared, ticks, ComplexRange)
    };

    public static Measure Ticks(double units) =>
        new((long)Math.Round(units * Resolution.Raw, MidpointRounding.AwayFromZero));

    public static double Units(Measure ticks) => Magnitude(ticks).Normalize().Approximate;

    private static Ratio Magnitude(Measure ticks) => MagnitudeBasis.Project(ticks).Normalize();

    private static InvariantPair LockProductFromPrimary(Ratio invariant, Measure ticks, PairRange range)
    {
        var primary = ClampMagnitudeTicks(ticks, range);
        var secondary = Ticks(invariant.Divide(Magnitude(primary)).Normalize().Approximate);
        return new InvariantPair(primary, secondary);
    }

    private static InvariantPair LockProductFromSecondary(Ratio invariant, Measure ticks, PairRange range)
    {
        var secondary = ClampMagnitudeTicks(ticks, range);
        var primary = Ticks(invariant.Divide(Magnitude(secondary)).Normalize().Approximate);
        return new InvariantPair(primary, secondary);
    }

    private static InvariantPair LockSumFromPrimary(Ratio invariant, Measure ticks, PairRange range)
    {
        var primary = ClampMagnitudeTicks(ticks, range);
        var secondary = Ticks(invariant.Subtract(Magnitude(primary)).Normalize().Approximate);
        return new InvariantPair(primary, secondary);
    }

    private static InvariantPair LockSumFromSecondary(Ratio invariant, Measure ticks, PairRange range)
    {
        var secondary = ClampMagnitudeTicks(ticks, range);
        var primary = Ticks(invariant.Subtract(Magnitude(secondary)).Normalize().Approximate);
        return new InvariantPair(primary, secondary);
    }

    private static InvariantPair LockNormFromPrimary(Ratio invariant, Measure ticks, PairRange range)
    {
        var primary = ClampMagnitudeTicks(ticks, range);
        var primaryUnits = Units(primary);
        var secondary = Ticks(Math.Sqrt(Math.Max(0d, invariant.Normalize().Approximate - (primaryUnits * primaryUnits))));
        return new InvariantPair(primary, secondary);
    }

    private static InvariantPair LockNormFromSecondary(Ratio invariant, Measure ticks, PairRange range)
    {
        var secondary = ClampMagnitudeTicks(ticks, range);
        var secondaryUnits = Units(secondary);
        var primary = Ticks(Math.Sqrt(Math.Max(0d, invariant.Normalize().Approximate - (secondaryUnits * secondaryUnits))));
        return new InvariantPair(primary, secondary);
    }

    private static Measure ClampMagnitudeTicks(Measure ticks, PairRange range) =>
        Ticks(Math.Clamp(Units(ticks), range.MinimumUnits, range.MaximumUnits));

    private readonly record struct PairRange(double MinimumUnits, double MaximumUnits);
}

public readonly record struct InvariantPair(Measure PrimaryTicks, Measure SecondaryTicks);
