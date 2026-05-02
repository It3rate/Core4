namespace Gojo.Core;

public sealed class TickRef(long ticks) : IElement
{
    public TickRef(ulong ticks)
        : this(checked((long)ticks))
    {
    }

    public long Ticks { get; private set; } = ticks;

    public TickRef Dual() => new(checked(-Ticks));

    public void Set(long ticks) => Ticks = ticks;

    public void Move(long delta) => Ticks = checked(Ticks + delta);

    IElement IElement.Dual() => Dual();

    public override string ToString() => Ticks.ToString();
}
