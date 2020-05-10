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

using System;
using System.Runtime.CompilerServices;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.IO;

namespace ServiceModel.Grpc.Configuration
{
    internal static class MarshallerFactoryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMarshallerFactory ThisOrDefault(this IMarshallerFactory factory)
        {
            return factory ?? DataContractMarshallerFactory.Default;
        }

        public static byte[] SerializeHeader(this IMarshallerFactory factory, object value)
        {
            factory.AssertNotNull(nameof(factory));
            value.AssertNotNull(nameof(value));

            if (value is byte[] buffer)
            {
                return buffer;
            }

            return typeof(MarshallerFactoryExtensions)
                .StaticMethod(nameof(SerializeInternal))
                .MakeGenericMethod(value.GetType())
                .CreateDelegate<Func<IMarshallerFactory, object, byte[]>>()
                .Invoke(factory, value);
        }

        public static object DeserializeHeader(this IMarshallerFactory factory, Type valueType, byte[] valueContent)
        {
            valueType.AssertNotNull(nameof(valueType));

            if (valueType == typeof(byte[]))
            {
                return valueContent;
            }

            return typeof(MarshallerFactoryExtensions)
                .StaticMethod(nameof(DeserializeInternal))
                .MakeGenericMethod(valueType)
                .CreateDelegate<Func<IMarshallerFactory, byte[], object>>()
                .Invoke(factory, valueContent);
        }

        private static byte[] SerializeInternal<T>(IMarshallerFactory factory, object value)
        {
            using (var context = new DefaultSerializationContext())
            {
                factory.CreateMarshaller<T>().ContextualSerializer((T)value, context);
                return context.GetContent();
            }
        }

        private static object DeserializeInternal<T>(IMarshallerFactory factory, byte[] content)
        {
            return factory.CreateMarshaller<T>().ContextualDeserializer(new DefaultDeserializationContext(content));
        }
    }
}
