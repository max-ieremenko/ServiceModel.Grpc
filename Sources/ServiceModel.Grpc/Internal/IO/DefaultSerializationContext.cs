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
using System.Runtime.CompilerServices;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.IO;

internal sealed class DefaultSerializationContext : SerializationContext, IDisposable
{
    private readonly BufferWriter<byte> _buffer;
    private bool _isSealed;

    public DefaultSerializationContext(int initialCapacity = 1024)
    {
        _buffer = new BufferWriter<byte>(initialCapacity);
    }

    public byte[] GetContent()
    {
        if (!_isSealed)
        {
            throw new InvalidOperationException("Serialization context is not completed.");
        }

        return _buffer.ToArray();
    }

    public override void Complete()
    {
        CheckIsNotSealed();
        _isSealed = true;
    }

    public override void Complete(byte[] payload)
    {
        payload.AssertNotNull(nameof(payload));

        Complete();

        if (payload.Length > 0)
        {
            var span = _buffer.GetSpan(payload.Length);
            payload.AsSpan(0).CopyTo(span);
            _buffer.Advance(payload.Length);
        }
    }

    public override IBufferWriter<byte> GetBufferWriter()
    {
        CheckIsNotSealed();
        return _buffer;
    }

    public override void SetPayloadLength(int payloadLength)
    {
        CheckIsNotSealed();
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckIsNotSealed()
    {
        if (_isSealed)
        {
            throw new InvalidOperationException("Serialization context is completed.");
        }
    }
}