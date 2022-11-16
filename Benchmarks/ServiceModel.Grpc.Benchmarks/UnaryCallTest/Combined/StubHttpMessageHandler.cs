// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Combined;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    public long PayloadSize { get; private set; }

    public static async ValueTask<long> GetPayloadSize(Func<GrpcChannel, Task> call)
    {
        using var httpHandler = new StubHttpMessageHandler();
        using var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = httpHandler });

        try
        {
            await call(channel).ConfigureAwait(false);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
        }

        return httpHandler.PayloadSize;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await using var stream = await request.Content!.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await stream.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
        PayloadSize = stream.Length;

        return CreateResponse();
    }

    private HttpResponseMessage CreateResponse()
    {
        var content = new ByteArrayContent(Array.Empty<byte>());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/grpc");

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content,
            Version = new Version(2, 0),
            TrailingHeaders =
            {
                { "grpc-status", StatusCode.Cancelled.ToString("D") }
            }
        };
    }
}