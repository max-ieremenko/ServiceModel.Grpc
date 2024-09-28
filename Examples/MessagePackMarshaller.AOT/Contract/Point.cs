using MessagePack;

namespace Contract;

[MessagePackObject]
public sealed class Point
{
    public Point()
    {
    }

    public Point(Number x, Number y)
    {
        X = x;
        Y = y;
    }

    [Key(1)]
    public Number X { get; set; }

    [Key(2)]
    public Number Y { get; set; }

    public static implicit operator Point((Number X, Number Y) value) => new(value.X, value.Y);

    public override string ToString() => $"{X},{Y}";
}