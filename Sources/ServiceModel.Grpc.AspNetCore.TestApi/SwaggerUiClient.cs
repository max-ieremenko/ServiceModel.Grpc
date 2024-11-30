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

using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ServiceModel.Grpc.AspNetCore.TestApi;

public sealed class SwaggerUiClient
{
    private readonly OpenApiDocument _document;
    private readonly string _serviceName;
    private readonly string _hostLocation;

    public SwaggerUiClient(
        OpenApiDocument document,
        string serviceName,
        string hostLocation)
    {
        _document = document;
        _serviceName = serviceName;
        _hostLocation = hostLocation;
    }

    public async Task<HttpResponseHeaders> InvokeAsync(string methodName, IDictionary<string, object> parameters, IDictionary<string, string>? headers = null)
    {
        var response = await PostAsync(methodName, parameters, headers).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        return response.Headers;
    }

    public async Task<T> InvokeAsync<T>(string methodName, IDictionary<string, object> parameters, IDictionary<string, string>? headers = null)
    {
        var response = await PostAsync(methodName, parameters, headers).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return JsonSerializer.CreateDefault().Deserialize<T>(new JsonTextReader(new StreamReader(content)))!;
    }

    public async Task<HttpResponseMessage> PostAsync(string methodName, IDictionary<string, object>? parameters = null, IDictionary<string, string>? headers = null)
    {
        var endpoint = _document.GetEndpoint(_serviceName, methodName);
        var contentType = _document.GetRequestContentType(endpoint);
        var url = new Uri(new Uri(_hostLocation), endpoint);

        using var client = new HttpClient();

        using var requestBody = new MemoryStream();
        using (var writer = new StreamWriter(requestBody, leaveOpen: true))
        {
            JsonSerializer.CreateDefault().Serialize(writer, parameters);
        }

        HttpResponseMessage response;

        requestBody.Position = 0;
        using (var request = new StreamContent(requestBody))
        {
            request.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            if (headers != null)
            {
                foreach (var entry in headers)
                {
                    request.Headers.Add(entry.Key, entry.Value);
                }
            }

            response = await client.PostAsync(url, request).ConfigureAwait(false);
        }

        var result = new HttpResponseMessage(response.StatusCode);
        foreach (var header in response.Headers)
        {
            result.Headers.Add(header.Key, header.Value);
        }

        var responseBody = new MemoryStream();
        await using (var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
        {
            await content.CopyToAsync(responseBody).ConfigureAwait(false);
        }

        responseBody.Position = 0;
        result.Content = new StreamContent(responseBody)
        {
            Headers =
            {
                ContentType = response.Content.Headers.ContentType
            }
        };

        return result;
    }
}