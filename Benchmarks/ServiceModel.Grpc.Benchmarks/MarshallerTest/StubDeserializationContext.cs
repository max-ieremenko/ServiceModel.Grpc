﻿// <copyright>
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

namespace ServiceModel.Grpc.Benchmarks.MarshallerTest;

internal sealed class StubDeserializationContext : DeserializationContext
{
    private readonly byte[] _payload;

    public StubDeserializationContext(byte[] payload)
    {
        _payload = payload;
    }

    public override int PayloadLength => _payload.Length;

    public override byte[] PayloadAsNewBuffer() => throw new NotSupportedException();

    public override ReadOnlySequence<byte> PayloadAsReadOnlySequence() => new ReadOnlySequence<byte>(_payload);
}