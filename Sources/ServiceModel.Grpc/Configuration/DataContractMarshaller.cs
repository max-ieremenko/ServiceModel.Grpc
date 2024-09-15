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
using Grpc.Core;

namespace ServiceModel.Grpc.Configuration;

[RequiresDynamicCode("Data Contract Serialization and Deserialization might require types that cannot be statically analyzed.")]
[RequiresUnreferencedCode("Data Contract Serialization and Deserialization might require types that cannot be statically analyzed.")]
internal static class DataContractMarshaller<T>
{
    public static readonly Marshaller<T> Default = new Marshaller<T>(Serialize, Deserialize);

    // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datacontractserializer?view=net-5.0#thread-safety
    private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(T), ResolveKnownTypes());

    private static void Serialize(T value, SerializationContext context)
    {
        using (var buffer = context.AsStream())
        {
            Serializer.WriteObject(buffer, value);
        }

        context.Complete();
    }

    private static T Deserialize(DeserializationContext context)
    {
        using (var buffer = context.AsStream())
        {
            return (T)Serializer.ReadObject(buffer)!;
        }
    }

    private static Type[] ResolveKnownTypes()
    {
        var root = typeof(T);
        if (!root.IsGenericType)
        {
            return [];
        }

        var types = new HashSet<Type>(root.GenericTypeArguments);

        var result = new Type[types.Count];
        types.CopyTo(result, 0);
        return result;
    }
}