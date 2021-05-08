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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Server
{
    internal sealed class StubHttpRequest
    {
        private static readonly MediaTypeHeaderValue ApplicationGrpc = new MediaTypeHeaderValue("application/grpc");
        private static readonly HttpMethod Method = new HttpMethod("POST");
        private static readonly Version Version = new Version("2.0");

        private readonly HttpClient _client;
        private readonly string _url;
        private readonly byte[] _payload;

        public StubHttpRequest(HttpClient client, string url, byte[] payload)
        {
            _client = client;
            _url = url;
            _payload = payload;
        }

        public long PayloadSize { get; private set; }

        public async Task SendAsync()
        {
            using (var request = CreateRequest())
            using (var response = await _client.SendAsync(request).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content!.ReadAsStreamAsync().ConfigureAwait(false);
                await stream.CopyToAsync(Stream.Null).ConfigureAwait(false);
                PayloadSize = stream.Length;
            }
        }

        private HttpRequestMessage CreateRequest()
        {
            var content = new ByteArrayContent(_payload);
            content.Headers.ContentType = ApplicationGrpc;

            var request = new HttpRequestMessage(Method, _url);
            request.Version = Version;
            request.Content = content;

            return request;
        }
    }
}
