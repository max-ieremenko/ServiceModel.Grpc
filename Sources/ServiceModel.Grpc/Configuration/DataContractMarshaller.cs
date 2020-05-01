// <copyright>
// Copyright 2020 Max Ieremenko
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

namespace ServiceModel.Grpc.Configuration
{
    internal static class DataContractMarshaller<T>
    {
        public static readonly Marshaller<T> Default = new Marshaller<T>(Serialize, Deserialize);

        private static void Serialize(T value, SerializationContext context)
        {
            using (var buffer = context.AsStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(buffer, value);
            }

            context.Complete();
        }

        private static T Deserialize(DeserializationContext context)
        {
            using (var buffer = context.AsStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(buffer);
            }
        }
    }
}
