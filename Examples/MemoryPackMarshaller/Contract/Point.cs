using System.Diagnostics.CodeAnalysis;
using MemoryPack;

namespace Contract;

[MemoryPackable]
public sealed partial record Point
{
    public Point()
    {
    }

    [MemoryPackConstructor]
    [SetsRequiredMembers]
    public Point(Number x, Number y)
    {
        X = x;
        Y = y;
    }

    [MemoryPackOrder(0)]
    public required Number X { get; init; }

    [MemoryPackOrder(1)]
    public required Number Y { get; init; }

    public static implicit operator Point((Number X, Number Y) value) => new(value.X, value.Y);
}