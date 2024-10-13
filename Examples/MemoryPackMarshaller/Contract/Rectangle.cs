using System.Diagnostics.CodeAnalysis;
using MemoryPack;

namespace Contract;

[MemoryPackable]
public sealed partial record Rectangle
{
    public Rectangle()
    {
    }

    [MemoryPackConstructor]
    [SetsRequiredMembers]
    public Rectangle(Point leftTop, Number width, Number height)
    {
        LeftTop = leftTop;
        Width = width;
        Height = height;
    }

    [MemoryPackOrder(0)]
    public required Point LeftTop { get; init; }

    [MemoryPackOrder(1)]
    public required Number Width { get; init; }

    [MemoryPackOrder(2)]
    public required Number Height { get; init; }
}