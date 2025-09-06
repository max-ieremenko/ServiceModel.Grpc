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

using MessagePack;
using MessagePack.Formatters;

namespace ServiceModel.Grpc.Domain;

internal sealed class TestMessage4MessagePackFormatter<T1, T2, T3, T4> : IMessagePackFormatter<TestMessage<T1, T2, T3, T4>?>
{
    public int SerializeCallsCounter { get; set; }

    public int DeserializeCallsCounter { get; set; }

    public void Serialize(ref MessagePackWriter writer, TestMessage<T1, T2, T3, T4>? value, MessagePackSerializerOptions options)
    {
        SerializeCallsCounter++;

        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var formatterResolver = options.Resolver;
        writer.WriteArrayHeader(5);
        writer.WriteNil();
        formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Value1!, options);
        formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Value2!, options);
        formatterResolver.GetFormatterWithVerify<T3>().Serialize(ref writer, value.Value3!, options);
        formatterResolver.GetFormatterWithVerify<T4>().Serialize(ref writer, value.Value4!, options);
    }

    public TestMessage<T1, T2, T3, T4> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        DeserializeCallsCounter++;

        if (reader.TryReadNil())
        {
            return null!;
        }

        options.Security.DepthStep(ref reader);
        var formatterResolver = options.Resolver;
        var length = reader.ReadArrayHeader();

        var result = new TestMessage<T1, T2, T3, T4>();
        for (var i = 0; i < length; i++)
        {
            switch (i)
            {
                case 1:
                    result.Value1 = formatterResolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                    break;
                case 2:
                    result.Value2 = formatterResolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                    break;
                case 3:
                    result.Value3 = formatterResolver.GetFormatterWithVerify<T3>().Deserialize(ref reader, options);
                    break;
                case 4:
                    result.Value4 = formatterResolver.GetFormatterWithVerify<T4>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;
        return result;
    }
}