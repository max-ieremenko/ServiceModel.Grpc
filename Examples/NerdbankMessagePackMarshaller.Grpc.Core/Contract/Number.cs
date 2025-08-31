using PolyType;

namespace Contract;

[GenerateShape]
public partial record struct Number
{
    public Number(int value)
    {
        Value = value;
    }

    [PropertyShape(Order = 1)]
    public int Value { get; set; }

    public static implicit operator Number(int value) => new(value);
}