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

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Server;

internal sealed class StubHttpRequest
{
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly byte[] _payload;

    public StubHttpRequest(HttpClient client, string url, byte[] payload)
    {
        _client = client;
        _url = url;
        _payload = payload;
    }

    public async Task SendAsync()
    {
        using (var request = HttpMessage.CreateRequest(_url, _payload))
        using (var response = await _client.SendAsync(request).ConfigureAwait(false))
        {
            response.EnsureSuccessStatusCode();

            await HttpMessage.ReadAsync(response.Content, default).ConfigureAwait(false);
        }
    }
}