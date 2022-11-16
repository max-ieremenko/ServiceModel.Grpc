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

using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.AspNetCore;

internal sealed class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloResult> Unary(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloResult
        {
            Message = "Hello " + request.Name + "!"
        });
    }

    public override async Task DuplexStreaming(
        IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloResult> responseStream,
        ServerCallContext context)
    {
        var greet = CompatibilityToolsTestExtensions.DeserializeMethodInput<string>(ProtobufMarshallerFactory.Default, context.RequestHeaders);
        await context.WriteResponseHeadersAsync(CompatibilityToolsTestExtensions.SerializeMethodOutput(ProtobufMarshallerFactory.Default, greet)).ConfigureAwait(false);

        while (await requestStream.MoveNext(context.CancellationToken).ConfigureAwait(false))
        {
            var message = greet + " " + requestStream.Current.Name + "!";
            await responseStream.WriteAsync(new HelloResult { Message = message }).ConfigureAwait(false);
        }
    }
}