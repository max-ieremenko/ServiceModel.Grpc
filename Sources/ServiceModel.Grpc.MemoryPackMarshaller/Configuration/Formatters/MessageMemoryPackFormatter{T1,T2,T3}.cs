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
internal sealed class MessageMemoryPackFormatter<T1, T2, T3> : MemoryPackFormatter<Message<T1, T2, T3>>
{
    [MemoryPack.Internal.Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Message<T1, T2, T3>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(3);
        writer.WriteValue(value.Value1);
        writer.WriteValue(value.Value2);
        writer.WriteValue(value.Value3);
    }

    [MemoryPack.Internal.Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref Message<T1, T2, T3>? value)
    {
        if (!reader.TryReadObjectHeader(out var length))
        {
            value = default;
            return;
        }

        if (length > 3)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(Message<T1, T2, T3>), 3, length);
        }

        var result = value ?? new Message<T1, T2, T3>();
        if (length > 0)
        {
            result.Value1 = reader.ReadValue<T1>();
        }
        else if (value != null)
        {
            value.Value1 = default;
        }

        if (length > 1)
        {
            result.Value2 = reader.ReadValue<T2>();
        }
        else if (value != null)
        {
            value.Value2 = default;
        }

        if (length > 2)
        {
            result.Value3 = reader.ReadValue<T3>();
        }
        else if (value != null)
        {
            value.Value3 = default;
        }

        value = result;
    }
}