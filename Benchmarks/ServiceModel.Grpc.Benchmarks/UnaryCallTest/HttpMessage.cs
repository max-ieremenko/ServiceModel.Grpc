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

using System.Net;
using System.Net.Http.Headers;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest;

internal static class HttpMessage
{
    private static readonly byte[] NullBuffer = new byte[10 * 1024];
    private static readonly MediaTypeHeaderValue ApplicationGrpc = new("application/grpc");

    public static HttpRequestMessage CreateRequest(string requestUri, byte[] payload)
    {
        var content = new ByteArrayContent(payload);
        content.Headers.ContentType = ApplicationGrpc;

        return new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Version = HttpVersion.Version20,
            Content = content
        };
    }

    public static HttpResponseMessage CreateResponse(byte[] payload, string status)
    {
        var content = new ByteArrayContent(payload);
        content.Headers.ContentType = ApplicationGrpc;

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content,
            Version = HttpVersion.Version20,
            TrailingHeaders =
            {
                { "grpc-status", status }
            }
        };
    }

    public static async Task<int> ReadAsync(HttpContent content, CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var length = (int)stream.Length;
        if (length > NullBuffer.Length)
        {
            throw new InvalidOperationException("HttpMessage.NullBuffer is too small.");
        }

        await stream.ReadExactlyAsync(NullBuffer.AsMemory(0, length), cancellationToken).ConfigureAwait(false);
        return length;
    }
}