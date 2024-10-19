using System.Diagnostics.CodeAnalysis;
using MemoryPack;

namespace Contract;

[MemoryPackable]
public readonly partial record struct Number
{
    [SetsRequiredMembers]
    public Number(int value)
    {
        Value = value;
    }

    [MemoryPackOrder(0)]
    public required int Value { get; init; }

    public static implicit operator Number(int value) => new(value);
}