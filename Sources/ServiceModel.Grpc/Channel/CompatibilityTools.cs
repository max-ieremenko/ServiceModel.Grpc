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
using System.Linq;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.IO;

namespace ServiceModel.Grpc.Channel
{
    internal static class CompatibilityTools
    {
        public static Metadata MethodInputAsHeader<T>(IMarshallerFactory marshallerFactory, T value)
        {
            return CallOptionsBuilder.GetMethodInputHeader(
                marshallerFactory.CreateMarshaller<Message<T>>(),
                new Message<T>(value));
        }

        public static Metadata MethodInputAsHeader<T1, T2>(IMarshallerFactory marshallerFactory, T1 value1, T2 value2)
        {
            return CallOptionsBuilder.GetMethodInputHeader(
                marshallerFactory.CreateMarshaller<Message<T1, T2>>(),
                new Message<T1, T2>(value1, value2));
        }

        public static T GetMethodInputFromHeader<T>(IMarshallerFactory marshallerFactory, Metadata requestHeaders)
        {
            var input = GetMethodInputFromHeader(marshallerFactory.CreateMarshaller<Message<T>>(), requestHeaders);
            return input.Value1;
        }

        public static (T1 Value1, T2 Value2) GetMethodInputFromHeader<T1, T2>(IMarshallerFactory marshallerFactory, Metadata requestHeaders)
        {
            var input = GetMethodInputFromHeader(marshallerFactory.CreateMarshaller<Message<T1, T2>>(), requestHeaders);
            return (input.Value1, input.Value2);
        }

        internal static T GetMethodInputFromHeader<T>(Marshaller<T> marshaller, Metadata requestHeaders)
        {
            var header = requestHeaders?.FirstOrDefault(i => i.IsBinary && CallContext.HeaderNameMethodInput.Equals(i.Key, StringComparison.OrdinalIgnoreCase));
            if (header == null)
            {
                throw new InvalidOperationException("Fail to resolve header parameters, {0} header not found.".FormatWith(CallContext.HeaderNameMethodInput));
            }

            return marshaller.ContextualDeserializer(new DefaultDeserializationContext(header.ValueBytes));
        }
    }
}
