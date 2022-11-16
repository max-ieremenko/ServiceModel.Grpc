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
using Google.Protobuf;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Client;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly byte[] _responsePayload;

    public StubHttpMessageHandler(IMarshallerFactory marshallerFactory, object response)
    {
        _responsePayload = MessageSerializer.Create(marshallerFactory, response);
    }

    public StubHttpMessageHandler(IMessage response)
    {
        _responsePayload = MessageSerializer.Create(response);
    }

    public long PayloadSize { get; private set; }

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
        var content = new ByteArrayContent(_responsePayload);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/grpc");

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content,
            Version = new Version(2, 0),
            TrailingHeaders =
            {
                { "grpc-status", StatusCode.OK.ToString("D") }
            }
        };
    }
}