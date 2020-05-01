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
            var methodInputAsHeader = (Func<IMarshallerFactory, string, Metadata>)typeof(CallContext)
                .Assembly
                .GetType("ServiceModel.Grpc.Channel.CompatibilityTools", true, false)
                .GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static)
                .Where(i => i.Name == "MethodInputAsHeader")
                .First(i => i.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(string))
                .CreateDelegate(typeof(Func<IMarshallerFactory, string, Metadata>));

            return methodInputAsHeader(ProtobufMarshallerFactory.Default, value);
        }

        private static string GetHelloAllHeader(ServerCallContext context)
        {
            var methodInputFromHeader = (Func<IMarshallerFactory, Metadata, string>)typeof(CallContext)
                .Assembly
                .GetType("ServiceModel.Grpc.Channel.CompatibilityTools", true, false)
                .GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static)
                .Where(i => i.Name == "GetMethodInputFromHeader")
                .First(i => i.GetGenericArguments().Length == 1)
                .MakeGenericMethod(typeof(string))
                .CreateDelegate(typeof(Func<IMarshallerFactory, Metadata, string>));

            return methodInputFromHeader(ProtobufMarshallerFactory.Default, context.RequestHeaders);
        }
    }
}
