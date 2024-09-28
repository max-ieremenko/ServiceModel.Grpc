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
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Configuration.Formatters;

internal sealed class MessageMessagePackFormatter<T1, T2> : IMessagePackFormatter<Message<T1, T2>>
{
#if DEBUG
    public int SerializeCallsCounter { get; set; }

    public int DeserializeCallsCounter { get; set; }
#endif

    public void Serialize(ref MessagePackWriter writer, Message<T1, T2>? value, MessagePackSerializerOptions options)
    {
#if DEBUG
        SerializeCallsCounter++;
#endif

        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var formatterResolver = options.Resolver;
        writer.WriteArrayHeader(3);
        writer.WriteNil();
        formatterResolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.Value1!, options);
        formatterResolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.Value2!, options);
    }

    public Message<T1, T2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
#if DEBUG
        DeserializeCallsCounter++;
#endif

        if (reader.TryReadNil())
        {
            return null!;
        }

        options.Security.DepthStep(ref reader);
        var formatterResolver = options.Resolver;
        var length = reader.ReadArrayHeader();

        var result = new Message<T1, T2>();
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
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;
        return result;
    }
}