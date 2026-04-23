namespace Core4.Experimental.Bootstrap;

public readonly record struct Carrier(long Id, long Domain = 0)
{
    public static Carrier Unbound => new(0);

    public static Carrier Primary => new(1);

    public override string ToString() => Domain == 0 ? Id.ToString() : $"{Domain}:{Id}";
}
