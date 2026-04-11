using System;
using System.Collections.Immutable;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Fusion glossary scene. Two selected aligned/orthogonal elements are staged, fused, and then recorded in the
/// gallery of reusable pair outcomes.
/// </summary>
public readonly record struct GlossaryFusionDemoState(
    ImmutableArray<GlossaryFusionSourceState> Sources,
    ImmutableArray<GlossaryFusionResultState> Results,
    GlossaryFusionSelectionState Selection,
    GlossaryFusionAnimationState Animation,
    long LastResultKey)
{
    public const double StageAnimationSeconds = 0.65d;
    public const double FuseAnimationSeconds = 1.9d;

    public static GlossaryFusionDemoState CreateDefault()
        => new(
            CreateSources(),
            CreateResults(),
            default,
            default,
            0L);

    public bool IsAnimating => Animation.Mode != 0L;

    public bool HasFirstSelection => Selection.FirstSourceId != 0L;

    public double CurrentAnimationDurationSeconds => Animation.Mode == 1L
        ? StageAnimationSeconds
        : FuseAnimationSeconds;

    public string PhaseText
    {
        get
        {
            if (Animation.Mode == 1L && TryFindSource(Animation.FirstSourceId, out var staged))
            {
                return $"staging {staged.BucketLabel.ToLowerInvariant()}";
            }

            if (Animation.Mode == 2L && TryFindResult(Animation.ResultKey, out var result))
            {
                return $"fusing into {result.Title.ToLowerInvariant()}";
            }

            if (HasFirstSelection && TryFindSource(Selection.FirstSourceId, out var selected))
            {
                return $"picked {selected.BucketLabel.ToLowerInvariant()}";
            }

            if (LastResultKey != 0L && TryFindResult(LastResultKey, out var last))
            {
                return $"last result {last.Title.ToLowerInvariant()}";
            }

            return "select any source element";
        }
    }

    public string HintText
    {
        get
        {
            if (Animation.Mode == 2L)
            {
                return "orthogonal inputs inherit unit ticks from aligned inputs when the pair hardens";
            }

            if (HasFirstSelection)
            {
                return "pick a second source to complete the pair";
            }

            return $"{ResultsDiscovered} fused outcomes discovered";
        }
    }

    public int ResultsDiscovered
    {
        get
        {
            var total = 0;
            foreach (var result in Results)
            {
                if (result.IsDiscovered)
                {
                    total++;
                }
            }

            return total;
        }
    }

    public GlossaryFusionDemoState BeginSelect(long sourceId)
    {
        if (IsAnimating || !TryFindSource(sourceId, out _))
        {
            return this;
        }

        if (!HasFirstSelection)
        {
            return this with
            {
                Animation = new GlossaryFusionAnimationState(1L, sourceId, 0L, 0L, 0d)
            };
        }

        if (!TryResultKeyFor(Selection.FirstSourceId, sourceId, out var resultKey))
        {
            return this;
        }

        return this with
        {
            Animation = new GlossaryFusionAnimationState(2L, Selection.FirstSourceId, sourceId, resultKey, 0d)
        };
    }

    public GlossaryFusionDemoState WithAnimationProgress(double progress)
    {
        if (!IsAnimating)
        {
            return this;
        }

        return this with
        {
            Animation = Animation with { Progress = Math.Clamp(progress, 0d, 1d) }
        };
    }

    public GlossaryFusionDemoState CompleteAnimation()
    {
        if (!IsAnimating)
        {
            return this;
        }

        if (Animation.Mode == 1L)
        {
            return this with
            {
                Selection = new GlossaryFusionSelectionState(Animation.FirstSourceId),
                Animation = default
            };
        }

        if (Animation.Mode == 2L)
        {
            return this with
            {
                Results = MarkDiscovered(Results, Animation.ResultKey),
                Selection = default,
                Animation = default,
                LastResultKey = Animation.ResultKey
            };
        }

        return this with { Animation = default };
    }

    public GlossaryFusionDemoState Reset() => CreateDefault();

    public bool TryFindSource(long id, out GlossaryFusionSourceState source)
    {
        foreach (var candidate in Sources)
        {
            if (candidate.Id == id)
            {
                source = candidate;
                return true;
            }
        }

        source = default;
        return false;
    }

    public bool TryFindResult(long key, out GlossaryFusionResultState result)
    {
        foreach (var candidate in Results)
        {
            if (candidate.Key == key)
            {
                result = candidate;
                return true;
            }
        }

        result = default;
        return false;
    }

    public static bool TryResultKeyFor(long firstSourceId, long secondSourceId, out long resultKey)
    {
        var left = Math.Min(firstSourceId, secondSourceId);
        var right = Math.Max(firstSourceId, secondSourceId);
        resultKey = (left, right) switch
        {
            (1L, 1L) => 101L,
            (1L, 2L) => 102L,
            (1L, 3L) => 103L,
            (1L, 4L) => 104L,
            (2L, 2L) => 105L,
            (2L, 3L) => 106L,
            (2L, 4L) => 107L,
            (3L, 3L) => 108L,
            (3L, 4L) => 109L,
            (4L, 4L) => 110L,
            _ => 0L
        };

        return resultKey != 0L;
    }

    private static ImmutableArray<GlossaryFusionResultState> MarkDiscovered(
        ImmutableArray<GlossaryFusionResultState> results,
        long key)
    {
        var builder = results.ToBuilder();
        for (var index = 0; index < builder.Count; index++)
        {
            if (builder[index].Key != key)
            {
                continue;
            }

            builder[index] = builder[index] with { IsDiscovered = true };
            break;
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<GlossaryFusionSourceState> CreateSources()
        =>
        [
            new GlossaryFusionSourceState(1L, "Dominant Aligned", true, false),
            new GlossaryFusionSourceState(2L, "Recessive Aligned", false, false),
            new GlossaryFusionSourceState(3L, "Dominant Orthogonal", true, true),
            new GlossaryFusionSourceState(4L, "Recessive Orthogonal", false, true)
        ];

    private static ImmutableArray<GlossaryFusionResultState> CreateResults()
        =>
        [
            new GlossaryFusionResultState(101L, 1L, 1L, "SELF RIGHT", 1L, false),
            new GlossaryFusionResultState(102L, 1L, 2L, "SEGMENT H", 2L, false),
            new GlossaryFusionResultState(105L, 2L, 2L, "SELF LEFT", 5L, false),
            new GlossaryFusionResultState(106L, 2L, 3L, "ANTI-COMPLEX", 6L, false),
            new GlossaryFusionResultState(103L, 1L, 3L, "AREA", 3L, false),
            new GlossaryFusionResultState(108L, 3L, 3L, "SELF UP", 8L, false),
            new GlossaryFusionResultState(109L, 3L, 4L, "SEGMENT V", 9L, false),
            new GlossaryFusionResultState(110L, 4L, 4L, "SELF DOWN", 10L, false),
            new GlossaryFusionResultState(107L, 2L, 4L, "ANTI-AREA", 7L, false),
            new GlossaryFusionResultState(104L, 1L, 4L, "COMPLEX", 4L, false)
        ];
}

public readonly record struct GlossaryFusionSourceState(
    long Id,
    string BucketLabel,
    bool IsDominant,
    bool IsOrthogonal)
{
    public string ValueLabel => "8/4";
}

public readonly record struct GlossaryFusionResultState(
    long Key,
    long FirstSourceId,
    long SecondSourceId,
    string Title,
    long Kind,
    bool IsDiscovered);

public readonly record struct GlossaryFusionSelectionState(long FirstSourceId);

public readonly record struct GlossaryFusionAnimationState(
    long Mode,
    long FirstSourceId,
    long SecondSourceId,
    long ResultKey,
    double Progress);
