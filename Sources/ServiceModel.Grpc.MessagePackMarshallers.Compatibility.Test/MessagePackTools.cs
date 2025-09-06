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
using MessagePack.Resolvers;
using ServiceModel.Grpc.Domain;

namespace ServiceModel.Grpc;

internal static class MessagePackTools
{
    public static readonly MessagePackSerializerOptions Options = CreateOptions();

    public static byte[] Serialize<T>(T? value) => MessagePackSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(byte[] payload) => MessagePackSerializer.Deserialize<T>(payload, Options);

    private static MessagePackSerializerOptions CreateOptions()
    {
        var resolver = CompositeResolver.Create(GeneratedMessagePackResolver.Instance, StandardResolver.Instance);
        return MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}