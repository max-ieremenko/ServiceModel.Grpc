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

using System.Runtime.Serialization;
using MemoryPack;

namespace ServiceModel.Grpc.Configuration;

public partial class MemoryPackMarshallerFactoryTest
{
    [DataContract(Name = "m", Namespace = "s")]
    internal sealed class Message<T1, T2, T3, T4>
    {
        public Message()
        {
        }

        public Message(T1? value1, T2? value2, T3? value3, T4? value4)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }

        [DataMember(Name = "v1", Order = 1)]
        public T1? Value1 { get; set; }

        [DataMember(Name = "v2", Order = 2)]
        public T2? Value2 { get; set; }

        [DataMember(Name = "v3", Order = 3)]
        public T3? Value3 { get; set; }

        [DataMember(Name = "v4", Order = 4)]
        public T4? Value4 { get; set; }
    }

    [MemoryPack.Internal.Preserve]
    internal sealed class MessageMemoryPackFormatter<T1, T2, T3, T4> : MemoryPackFormatter<Message<T1, T2, T3, T4>>
    {
        [MemoryPack.Internal.Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Message<T1, T2, T3, T4>? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(4);
            writer.WriteValue(value.Value1);
            writer.WriteValue(value.Value2);
            writer.WriteValue(value.Value3);
            writer.WriteValue(value.Value4);
        }

        [MemoryPack.Internal.Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref Message<T1, T2, T3, T4>? value)
        {
            if (!reader.TryReadObjectHeader(out var length))
            {
                value = default;
                return;
            }

            if (length > 4)
            {
                MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(Message<T1, T2, T3, T4>), 4, length);
            }

            var result = value ?? new Message<T1, T2, T3, T4>();
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

            if (length > 3)
            {
                result.Value4 = reader.ReadValue<T4>();
            }
            else if (value != null)
            {
                value.Value4 = default;
            }

            value = result;
        }
    }
}