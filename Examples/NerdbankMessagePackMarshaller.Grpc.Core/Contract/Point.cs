using PolyType;

namespace Contract;

[GenerateShape]
public sealed partial record Point
{
    public Point()
    {
    }

    public Point(Number x, Number y)
    {
        X = x;
        Y = y;
    }

    [PropertyShape(Order = 1)]
    public Number X { get; set; }

    [PropertyShape(Order = 2)]
    public Number Y { get; set; }

    public static implicit operator Point((Number X, Number Y) value) => new(value.X, value.Y);
}