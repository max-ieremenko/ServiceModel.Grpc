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
using Grpc.Core;
using ServiceModel.Grpc.Internal.IO;

namespace ServiceModel.Grpc.Channel
{
    internal static class CompatibilityTools
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Metadata SerializeMethodInputHeader<T>(Marshaller<T> marshaller, T value)
        {
            return new Metadata
            {
                { CallContext.HeaderNameMethodInput, SerializeValue(marshaller, value) }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeserializeMethodInputHeader<T>(Marshaller<T> marshaller, Metadata? requestHeaders)
        {
            return DeserializeHeader(marshaller, requestHeaders, CallContext.HeaderNameMethodInput);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Metadata SerializeMethodOutputHeader<T>(Marshaller<T> marshaller, T value)
        {
            return new Metadata
            {
                { CallContext.HeaderNameMethodOutput, SerializeValue(marshaller, value) }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeserializeMethodOutputHeader<T>(Marshaller<T> marshaller, Metadata? responseHeaders)
        {
            return DeserializeHeader(marshaller, responseHeaders, CallContext.HeaderNameMethodOutput);
        }

        public static byte[] SerializeValue<T>(Marshaller<T> marshaller, T value)
        {
            byte[] payload;
            using (var serializationContext = new DefaultSerializationContext())
            {
                marshaller.ContextualSerializer(value, serializationContext);
                payload = serializationContext.GetContent();
            }

            return payload;
        }

        public static T DeserializeValue<T>(Marshaller<T> marshaller, byte[] payload)
        {
            return marshaller.ContextualDeserializer(new DefaultDeserializationContext(payload));
        }

        private static T DeserializeHeader<T>(Marshaller<T> marshaller, Metadata? headers, string headerName)
        {
            var header = headers.FindHeader(headerName, true);
            if (header == null)
            {
                throw new InvalidOperationException("Fail to resolve header parameters, {0} header not found.".FormatWith(headerName));
            }

            return marshaller.ContextualDeserializer(new DefaultDeserializationContext(header.ValueBytes));
        }
    }
}
