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

internal sealed class MessageMessagePackFormatter : IMessagePackFormatter<Message?>
{
#if DEBUG
    public int SerializeCallsCounter { get; set; }

    public int DeserializeCallsCounter { get; set; }
#endif

    public void Serialize(ref MessagePackWriter writer, Message? value, MessagePackSerializerOptions options)
    {
#if DEBUG
        SerializeCallsCounter++;
#endif

        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(0);
    }

    public Message Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
#if DEBUG
        DeserializeCallsCounter++;
#endif

        if (reader.TryReadNil())
        {
            return null!;
        }

        reader.Skip();
        return new Message();
    }
}