using System;
using System.Buffers;
using System.Diagnostics;

namespace Contract;

[DebuggerDisplay("Length {Length}")]
public sealed class RentedArray : IDisposable
{
    private readonly ArrayPool<byte> _owner;

    public RentedArray(ArrayPool<byte> owner, int length)
        : this(owner, owner.Rent(length), length)
    {
    }

    private RentedArray(ArrayPool<byte> owner, byte[] array, int length)
    {
        _owner = owner;
        Array = array;
        Length = length;
    }

    public byte[] Array { get; }

    public int Length { get; private set; }

    public static RentedArray Rent(int length, ArrayPool<byte>? owner = default)
    {
        var pool = owner ?? ArrayPool<byte>.Shared;
        return new RentedArray(pool, length);
    }

    public void Resize(int newLength)
    {
        Length = newLength;
    }

    public void Dispose()
    {
        _owner.Return(Array);
    }
}