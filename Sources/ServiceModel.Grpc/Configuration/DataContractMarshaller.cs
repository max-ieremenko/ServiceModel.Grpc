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

using System.IO;
using System.Runtime.Serialization;
using Grpc.Core;

namespace ServiceModel.Grpc.Configuration
{
    internal static class DataContractMarshaller<T>
    {
        public static readonly Marshaller<T> Default = new Marshaller<T>(Serialize, Deserialize);

        private static byte[] Serialize(T value)
        {
            if (value == null)
            {
                return null;
            }

            using (var buffer = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(buffer, value);

                return buffer.ToArray();
            }
        }

        private static T Deserialize(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return default;
            }

            using (var buffer = new MemoryStream(value))
            {
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(buffer);
            }
        }
    }
}
