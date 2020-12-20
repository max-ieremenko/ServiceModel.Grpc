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
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.IO
{
    internal sealed class DefaultDeserializationContext : DeserializationContext
    {
        private readonly byte[]? _payload;

        public DefaultDeserializationContext(byte[]? payload)
        {
            _payload = payload;
        }

        public override int PayloadLength => _payload?.Length ?? 0;

        public override byte[] PayloadAsNewBuffer()
        {
            if (_payload == null)
            {
                return Array.Empty<byte>();
            }

            var result = new byte[_payload.Length];
            Array.Copy(_payload, 0, result, 0, _payload.Length);
            return result;
        }

        public override ReadOnlySequence<byte> PayloadAsReadOnlySequence()
        {
            var payload = _payload ?? Array.Empty<byte>();
            return new ReadOnlySequence<byte>(payload);
        }
    }
}
