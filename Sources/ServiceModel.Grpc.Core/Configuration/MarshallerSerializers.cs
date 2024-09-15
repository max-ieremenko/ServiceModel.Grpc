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

using System.Buffers;

namespace ServiceModel.Grpc.Configuration;

internal static class MarshallerSerializers
{
    [RequiresDynamicCode("The native code for the serialization might not be available at runtime.")]
    [RequiresUnreferencedCode("The trimming may not validate that the requirements of 'valueType' are met.")]
    public static IMarshallerSerializer Get(Type valueType) =>
        (IMarshallerSerializer)Activator.CreateInstance(typeof(MarshallerSerializer<>).MakeGenericType(valueType))!;

    private sealed class MarshallerSerializer<T> : IMarshallerSerializer
    {
        public byte[] Serialize(IMarshallerFactory factory, object value) => MarshallerExtensions.Serialize(factory.CreateMarshaller<T>(), (T)value);

        public object Deserialize(IMarshallerFactory factory, byte[] payload) => MarshallerExtensions.Deserialize(factory.CreateMarshaller<T>(), payload)!;

        public object Deserialize(IMarshallerFactory factory, in ReadOnlySequence<byte> payload) => MarshallerExtensions.Deserialize(factory.CreateMarshaller<T>(), payload)!;
    }
}