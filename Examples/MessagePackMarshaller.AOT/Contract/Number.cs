using MessagePack;

namespace Contract;

[MessagePackObject]
public struct Number
{
    public Number(int value)
    {
        Value = value;
    }

    [Key(1)]
    public int Value { get; set; }

    public static implicit operator Number(int value) => new(value);

    public override string ToString() => Value.ToString();
}