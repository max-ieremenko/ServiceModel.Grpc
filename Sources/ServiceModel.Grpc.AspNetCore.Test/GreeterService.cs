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
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.AspNetCore
{
    internal sealed class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloResult> Hello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloResult
            {
                Message = "Hello " + request.Name + "!"
            });
        }

        public override async Task HelloAll(
            IAsyncStreamReader<HelloRequest> requestStream,
            IServerStreamWriter<HelloResult> responseStream,
            ServerCallContext context)
        {
            var greet = GetHelloAllHeader(context);

            while (await requestStream.MoveNext(context.CancellationToken))
            {
                var message = greet + " " + requestStream.Current.Name + "!";
                await responseStream.WriteAsync(new HelloResult { Message = message });
            }
        }

        internal static Metadata CreateHelloAllHeader(string value)
        {
            var messageType = typeof(CallContext)
                .Assembly
                .GetType("ServiceModel.Grpc.Channel.Message`1", true, false)
                .MakeGenericType(typeof(string));

            var message = messageType
                .GetConstructor(new[] { typeof(string) })
                .Invoke(new object[] { value });

            var marshaller = typeof(IMarshallerFactory)
                .GetMethod(nameof(IMarshallerFactory.CreateMarshaller), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .MakeGenericMethod(messageType)
                .Invoke(ProtobufMarshallerFactory.Default, Array.Empty<object>());

            var serializer = (Delegate)marshaller
                .GetType()
                .GetProperty("Serializer", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .GetMethod
                .Invoke(marshaller, Array.Empty<object>());

            var headerValue = (byte[])serializer.DynamicInvoke(message);

            return new Metadata
            {
                { "smgrpc-method-input-bin", headerValue }
            };
        }

        private static string GetHelloAllHeader(ServerCallContext context)
        {
            var messageType = typeof(CallContext)
                .Assembly
                .GetType("ServiceModel.Grpc.Channel.Message`1", true, false)
                .MakeGenericType(typeof(string));

            var marshaller = typeof(IMarshallerFactory)
                .GetMethod(nameof(IMarshallerFactory.CreateMarshaller), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .MakeGenericMethod(messageType)
                .Invoke(ProtobufMarshallerFactory.Default, Array.Empty<object>());

            var message = typeof(CallContext)
                .Assembly
                .GetType("ServiceModel.Grpc.Internal.Emit.ServerChannelAdapter", true, false)
                .GetMethod("GetMethodInputHeader", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .MakeGenericMethod(messageType)
                .Invoke(null, new[] { marshaller, context });

            var value1 = messageType
                .GetProperty("Value1", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .GetMethod
                .Invoke(message, Array.Empty<object>());

            return (string)value1;
        }
    }
}
