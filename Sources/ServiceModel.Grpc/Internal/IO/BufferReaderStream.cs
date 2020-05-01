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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Internal.IO
{
    internal sealed class BufferReaderStream : Stream
    {
        private ReadOnlySequence<byte> _sequence;

        public BufferReaderStream(ReadOnlySequence<byte> sequence)
        {
            _sequence = sequence;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override Task FlushAsync(CancellationToken token) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

        public override void WriteByte(byte value) => throw new NotSupportedException();

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => throw new NotSupportedException();

        public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException();

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken token) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            buffer.AssertNotNull(nameof(buffer));

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

            if (count == 0)
            {
                return 0;
            }

            var source = _sequence.Slice(0, Math.Min(count, _sequence.Length));
            if (source.Length > 0)
            {
                source.CopyTo(buffer.AsSpan(offset, count));
                _sequence = _sequence.Slice(source.End);
            }

            return (int)source.Length;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            var result = Read(buffer, offset, count);
            return Task.FromResult(result);
        }

        public override int ReadByte()
        {
            if (_sequence.Length == 0)
            {
                return -1;
            }

            var source = _sequence.Slice(0, 1);
            _sequence = _sequence.Slice(source.End);

            return source.First.Span[0];
        }
    }
}
