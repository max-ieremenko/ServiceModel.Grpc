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

using System.Net.Mime;
using System.Text.Json;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.TestApi;

public class OpenApiDocument
{
    public OpenApiDocument(JsonElement content)
    {
        Content = content;
    }

    public JsonElement Content { get; }

    public static async Task<OpenApiDocument> DownloadAsync(string location)
    {
        JsonElement content;

        using (var client = new HttpClient())
        {
            using (var response = await client.GetAsync(location).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    content = JsonSerializer.Deserialize<JsonElement>(stream);
                }
            }
        }

        return new OpenApiDocument(content);
    }

    public string GetEndpoint(string serviceName, string methodName) => $"/{serviceName}/{methodName}";

    public string GetRequestContentType(string endpoint)
    {
        var body = Content.GetProperty("paths").GetProperty(endpoint).GetProperty("post").GetProperty("requestBody");

        var contentType = body.GetProperty("content").EnumerateObject().First().Name;
        contentType.ShouldBe(MediaTypeNames.Application.Json + "+servicemodel.grpc", "Body is empty or content type is not set.");

        return contentType;
    }
}