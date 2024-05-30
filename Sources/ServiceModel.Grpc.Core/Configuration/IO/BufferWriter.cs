// <copyright>
// Copyright 2020 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Buffers;

namespace ServiceModel.Grpc.Configuration.IO;

internal sealed class BufferWriter<T> : IBufferWriter<T>, IDisposable
{
    private readonly int _minimumLength;
    private T[] _buffer;
    private int _length;

    public BufferWriter(int minimumLength)
    {
        if (minimumLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumLength));
        }

        _minimumLength = minimumLength;
        _buffer = ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public void Advance(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (_length + count > _buffer.Length)
        {
            throw new ArgumentException();
        }

        _length += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeHint));
        }

        CheckCapacity(sizeHint);
        return _buffer.AsMemory(_length);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeHint));
        }

        CheckCapacity(sizeHint);
        return _buffer.AsSpan(_length);
    }

    public T[] ToArray()
    {
        return _buffer.AsSpan(0, _length).ToArray();
    }

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_buffer, true);
    }

    private void CheckCapacity(int sizeHint)
    {
        var available = _buffer.Length - _length;
        if (sizeHint == 0)
        {
            sizeHint = _minimumLength;
        }

        if (sizeHint <= available)
        {
            return;
        }

        var increment = Math.Max(sizeHint, _minimumLength);

        var backup = _buffer;

        _buffer = ArrayPool<T>.Shared.Rent(backup.Length + increment);
        Array.Copy(backup, 0, _buffer, 0, _length);

        ArrayPool<T>.Shared.Return(backup);
    }
}