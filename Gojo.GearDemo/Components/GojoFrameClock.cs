namespace Gojo.GearDemo.Components;

internal sealed class GojoFrameClock
{
    private TimeSpan? lastFrameTime;
    private double pendingFrameMilliseconds;

    public void Reset()
    {
        lastFrameTime = null;
        pendingFrameMilliseconds = 0d;
    }

    public bool Advance(TimeSpan now, bool isRunning, Action<long> advanceMilliseconds)
    {
        var advanced = false;
        if (lastFrameTime is { } last)
        {
            var seconds = Math.Clamp((now - last).TotalSeconds, 0d, 0.1d);
            if (isRunning)
            {
                pendingFrameMilliseconds += seconds * 1000d;
                var wholeMilliseconds = (long)Math.Floor(pendingFrameMilliseconds);
                if (wholeMilliseconds > 0)
                {
                    advanceMilliseconds(wholeMilliseconds);
                    pendingFrameMilliseconds -= wholeMilliseconds;
                    advanced = true;
                }
            }
        }

        lastFrameTime = now;
        return advanced;
    }
}
