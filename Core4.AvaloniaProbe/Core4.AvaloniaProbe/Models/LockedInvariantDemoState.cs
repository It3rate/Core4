using System;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// First-pass locked-invariant explorer state. Self interaction and the directed segment still solve locally against
/// their frozen property, while the four 2D families share one signed driven coordinate so the same perturbation can
/// be read through four different locked loci.
/// </summary>
public readonly record struct LockedInvariantDemoState(
    InvariantPair Self,
    InvariantPair Segment,
    InvariantPair Quadrant,
    Measure QuadrantDriver)
{
    private static readonly Measure Resolution = new(64);
    private static readonly Carrier MagnitudeCarrier = new(61);
    private static readonly Basis MagnitudeBasis = Basis.Freeze(MagnitudeCarrier, Measure.Zero, Resolution);
    private static readonly InvariantPair QuadrantSeed = new(Ticks(3d), Ticks(2d));

    private static readonly PairRange SelfRange = new(1.5d, 4.0d);
    private static readonly PairRange SegmentRange = new(1.0d, 4.0d);
    private static readonly Ratio LockedArea = Magnitude(QuadrantSeed.PrimaryTicks).Multiply(Magnitude(QuadrantSeed.SecondaryTicks)).Normalize();
    private static readonly Ratio LockedNormSquared = Magnitude(QuadrantSeed.PrimaryTicks).Multiply(Magnitude(QuadrantSeed.PrimaryTicks))
        .Add(Magnitude(QuadrantSeed.SecondaryTicks).Multiply(Magnitude(QuadrantSeed.SecondaryTicks)))
        .Normalize();
    private static readonly PairRange QuadrantRange = new(-Math.Sqrt(LockedNormSquared.Normalize().Approximate), Math.Sqrt(LockedNormSquared.Normalize().Approximate));

    public static LockedInvariantDemoState Default => new(
        new InvariantPair(Ticks(3d), Ticks(2d)),
        new InvariantPair(Ticks(2d), Ticks(3d)),
        QuadrantSeed,
        Measure.One);

    public InvariantPair AreaPair => QuadrantDriver.Raw >= 0
        ? LockSignedProductFromPrimary(AreaInvariant, Quadrant.PrimaryTicks, QuadrantRange)
        : LockSignedProductFromSecondary(AreaInvariant, Quadrant.SecondaryTicks, QuadrantRange);

    public InvariantPair AntiAreaPair => AreaPair;

    public InvariantPair ComplexPair => QuadrantDriver.Raw >= 0
        ? LockSignedNormFromPrimary(ComplexNormSquared, Quadrant.PrimaryTicks, Quadrant.SecondaryTicks.Sign)
        : LockSignedNormFromSecondary(ComplexNormSquared, Quadrant.SecondaryTicks, Quadrant.PrimaryTicks.Sign);

    public InvariantPair AntiComplexPair => ComplexPair;

    public Ratio SelfPrimaryValue => Magnitude(Self.PrimaryTicks);

    public Ratio SelfSecondaryValue => Magnitude(Self.SecondaryTicks);

    public Ratio SegmentRecessiveValue => Magnitude(Segment.PrimaryTicks);

    public Ratio SegmentDominantValue => Magnitude(Segment.SecondaryTicks);

    public Ratio AreaHorizontalValue => Magnitude(AreaPair.PrimaryTicks);

    public Ratio AreaVerticalValue => Magnitude(AreaPair.SecondaryTicks);

    public Ratio AntiAreaHorizontalValue => Magnitude(AntiAreaPair.PrimaryTicks);

    public Ratio AntiAreaVerticalValue => Magnitude(AntiAreaPair.SecondaryTicks);

    public Ratio ComplexHorizontalValue => Magnitude(ComplexPair.PrimaryTicks);

    public Ratio ComplexVerticalValue => Magnitude(ComplexPair.SecondaryTicks);

    public Ratio AntiComplexHorizontalValue => Magnitude(AntiComplexPair.PrimaryTicks);

    public Ratio AntiComplexVerticalValue => Magnitude(AntiComplexPair.SecondaryTicks);

    public Ratio SelfInvariant => SelfPrimaryValue.Multiply(SelfSecondaryValue).Normalize();

    public Ratio SegmentInvariant => SegmentRecessiveValue.Add(SegmentDominantValue).Normalize();

    public Ratio AreaInvariant => LockedArea;

    public Ratio AntiAreaInvariant => LockedArea;

    public Ratio ComplexNormSquared => LockedNormSquared;

    public Ratio AntiComplexNormSquared => LockedNormSquared;

    public double SelfPrimaryUnits => Units(Self.PrimaryTicks);

    public double SelfSecondaryUnits => Units(Self.SecondaryTicks);

    public double SegmentRecessiveUnits => Units(Segment.PrimaryTicks);

    public double SegmentDominantUnits => Units(Segment.SecondaryTicks);

    public double QuadrantPrimaryUnits => Units(Quadrant.PrimaryTicks);

    public double QuadrantSecondaryUnits => Units(Quadrant.SecondaryTicks);

    public double AreaHorizontalUnits => Units(AreaPair.PrimaryTicks);

    public double AreaVerticalUnits => Units(AreaPair.SecondaryTicks);

    public double AntiAreaHorizontalUnits => Units(AntiAreaPair.PrimaryTicks);

    public double AntiAreaVerticalUnits => Units(AntiAreaPair.SecondaryTicks);

    public double ComplexHorizontalUnits => Units(ComplexPair.PrimaryTicks);

    public double ComplexVerticalUnits => Units(ComplexPair.SecondaryTicks);

    public double AntiComplexHorizontalUnits => Units(AntiComplexPair.PrimaryTicks);

    public double AntiComplexVerticalUnits => Units(AntiComplexPair.SecondaryTicks);

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
        Quadrant = new InvariantPair(ClampSignedTicks(ticks, QuadrantRange), Quadrant.SecondaryTicks),
        QuadrantDriver = Measure.One
    };

    public LockedInvariantDemoState WithAreaVerticalTicks(Measure ticks) => this with
    {
        Quadrant = new InvariantPair(Quadrant.PrimaryTicks, ClampSignedTicks(ticks, QuadrantRange)),
        QuadrantDriver = -Measure.One
    };

    public LockedInvariantDemoState WithAntiAreaHorizontalTicks(Measure ticks) => WithAreaHorizontalTicks(ticks);

    public LockedInvariantDemoState WithAntiAreaVerticalTicks(Measure ticks) => WithAreaVerticalTicks(ticks);

    public LockedInvariantDemoState WithComplexHorizontalTicks(Measure ticks) => WithAreaHorizontalTicks(ticks);

    public LockedInvariantDemoState WithComplexVerticalTicks(Measure ticks) => WithAreaVerticalTicks(ticks);

    public LockedInvariantDemoState WithAntiComplexHorizontalTicks(Measure ticks) => WithAreaHorizontalTicks(ticks);

    public LockedInvariantDemoState WithAntiComplexVerticalTicks(Measure ticks) => WithAreaVerticalTicks(ticks);

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

    private static InvariantPair LockSignedProductFromPrimary(Ratio invariant, Measure ticks, PairRange range)
    {
        var primary = ClampSignedTicks(ticks, range);
        var primaryUnits = Math.Abs(Units(primary));
        if (primaryUnits < 0.0001d)
        {
            return new InvariantPair(primary, Measure.Zero);
        }

        var secondaryUnits = invariant.Normalize().Approximate / primaryUnits;
        var sign = primary.Sign.Raw == 0 ? 1L : primary.Sign.Raw;
        return new InvariantPair(primary, new Measure(Ticks(secondaryUnits).Raw * sign));
    }

    private static InvariantPair LockSignedProductFromSecondary(Ratio invariant, Measure ticks, PairRange range)
    {
        var secondary = ClampSignedTicks(ticks, range);
        var secondaryUnits = Math.Abs(Units(secondary));
        if (secondaryUnits < 0.0001d)
        {
            return new InvariantPair(Measure.Zero, secondary);
        }

        var primaryUnits = invariant.Normalize().Approximate / secondaryUnits;
        var sign = secondary.Sign.Raw == 0 ? 1L : secondary.Sign.Raw;
        return new InvariantPair(new Measure(Ticks(primaryUnits).Raw * sign), secondary);
    }

    private static InvariantPair LockSignedNormFromPrimary(Ratio invariant, Measure ticks, Measure secondarySign)
    {
        var primary = ClampSignedTicks(ticks, QuadrantRange);
        var primaryUnits = Units(primary);
        var secondaryUnits = Math.Sqrt(Math.Max(0d, invariant.Normalize().Approximate - (primaryUnits * primaryUnits)));
        var sign = secondarySign.Raw == 0 ? 1L : secondarySign.Raw;
        return new InvariantPair(primary, new Measure(Ticks(secondaryUnits).Raw * sign));
    }

    private static InvariantPair LockSignedNormFromSecondary(Ratio invariant, Measure ticks, Measure primarySign)
    {
        var secondary = ClampSignedTicks(ticks, QuadrantRange);
        var secondaryUnits = Units(secondary);
        var primaryUnits = Math.Sqrt(Math.Max(0d, invariant.Normalize().Approximate - (secondaryUnits * secondaryUnits)));
        var sign = primarySign.Raw == 0 ? 1L : primarySign.Raw;
        return new InvariantPair(new Measure(Ticks(primaryUnits).Raw * sign), secondary);
    }

    private static Measure ClampMagnitudeTicks(Measure ticks, PairRange range) =>
        Ticks(Math.Clamp(Units(ticks), range.MinimumUnits, range.MaximumUnits));

    private static Measure ClampSignedTicks(Measure ticks, PairRange range) =>
        Ticks(Math.Clamp(Units(ticks), range.MinimumUnits, range.MaximumUnits));

    private readonly record struct PairRange(double MinimumUnits, double MaximumUnits);
}

public readonly record struct InvariantPair(Measure PrimaryTicks, Measure SecondaryTicks);
