// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.IO.Pipelines;
using Grpc.Core;

namespace ServiceModel.Grpc.Benchmarks.MarshallerTest;

internal sealed class StubSerializationContext : SerializationContext
{
    private readonly IBufferWriter<byte> _buffer = PipeWriter.Create(Stream.Null);

    public override IBufferWriter<byte> GetBufferWriter() => _buffer;

    public override void Complete(byte[] payload) => throw new NotSupportedException();

    public override void Complete()
    {
    }

    public override void SetPayloadLength(int payloadLength) => throw new NotSupportedException();
}