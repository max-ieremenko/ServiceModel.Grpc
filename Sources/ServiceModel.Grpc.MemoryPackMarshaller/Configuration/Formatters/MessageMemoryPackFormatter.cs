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

using MemoryPack;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Configuration.Formatters;

[MemoryPack.Internal.Preserve]
internal sealed class MessageMemoryPackFormatter : MemoryPackFormatter<Message>
{
    [MemoryPack.Internal.Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Message? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
        }
        else
        {
            writer.WriteObjectHeader(0);
        }
    }

    [MemoryPack.Internal.Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref Message? value)
    {
        if (!reader.TryReadObjectHeader(out var length))
        {
            value = default;
            return;
        }

        if (length != 0)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(Message), 0, length);
        }

        if (value == null)
        {
            value = new();
        }
    }
}