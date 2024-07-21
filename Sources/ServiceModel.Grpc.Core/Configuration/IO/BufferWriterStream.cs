// <copyright>
// Copyright Max Ieremenko
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

using System.Buffers;
using Grpc.Core.Utils;

namespace ServiceModel.Grpc.Configuration.IO;

internal sealed class BufferWriterStream : Stream
{
    private readonly IBufferWriter<byte> _writer;

    public BufferWriterStream(IBufferWriter<byte> writer)
    {
        GrpcPreconditions.CheckNotNull(writer, nameof(writer));

        _writer = GrpcPreconditions.CheckNotNull(writer, nameof(writer));
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken token) => Task.CompletedTask;

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();

    public override int EndRead(IAsyncResult asyncResult) => throw new NotSupportedException();

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) => throw new NotSupportedException();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token) => throw new NotSupportedException();

    public override int ReadByte() => throw new NotSupportedException();

    public override void WriteByte(byte value)
    {
        var span = _writer.GetSpan(1);
        span[0] = value;
        _writer.Advance(1);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        GrpcPreconditions.CheckNotNull(buffer, nameof(buffer));

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (buffer.Length - offset < count)
        {
            throw new ArgumentException();
        }

        var span = _writer.GetSpan(count);
        buffer.AsSpan(offset, count).CopyTo(span);
        _writer.Advance(count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token)
    {
        Write(buffer, offset, count);
        return Task.CompletedTask;
    }

#if NETSTANDARD2_1
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var span = _writer.GetSpan(buffer.Length);
        buffer.CopyTo(span);
        _writer.Advance(buffer.Length);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);
        return default;
    }
#endif
}