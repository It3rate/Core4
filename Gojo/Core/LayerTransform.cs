namespace Gojo.Core;

public readonly record struct LayerTransform(long Value)
{
    public static LayerTransform Additive => new(0);

    public static LayerTransform Expansive(ulong magnitude) => new(checked((long)magnitude));

    public static LayerTransform Contractive(ulong magnitude) => new(-checked((long)magnitude));

    public bool IsAdditive => Value == 0;

    public bool IsExpansive => Value > 0;

    public bool IsContractive => Value < 0;

    public ulong Magnitude => Value switch
    {
        < 0 => checked((ulong)-Value),
        > 0 => checked((ulong)Value),
        _ => 0
    };
}
