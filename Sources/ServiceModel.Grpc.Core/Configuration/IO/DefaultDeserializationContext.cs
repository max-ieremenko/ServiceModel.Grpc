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
using Grpc.Core;

namespace ServiceModel.Grpc.Configuration.IO;

internal sealed class DefaultDeserializationContext : DeserializationContext
{
    private readonly ReadOnlySequence<byte> _payload;

    public DefaultDeserializationContext(byte[]? payload)
    {
        _payload = new ReadOnlySequence<byte>(payload ?? Array.Empty<byte>());
    }

    public DefaultDeserializationContext(in ReadOnlySequence<byte> payload)
    {
        _payload = payload;
    }

    public override int PayloadLength => (int)_payload.Length;

    public override byte[] PayloadAsNewBuffer()
    {
        if (_payload.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        return _payload.ToArray();
    }

    public override ReadOnlySequence<byte> PayloadAsReadOnlySequence() => _payload;
}