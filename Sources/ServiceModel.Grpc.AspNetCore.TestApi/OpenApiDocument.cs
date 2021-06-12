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

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.TestApi
{
    public class OpenApiDocument
    {
        public OpenApiDocument(JObject content)
        {
            Content = content;
        }

        public JObject Content { get; }

        public static async Task<OpenApiDocument> DownloadAsync(string location)
        {
            JObject content;

            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(location))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        content = (JObject)JsonSerializer.CreateDefault().Deserialize(new JsonTextReader(new StreamReader(stream)))!;
                    }
                }
            }

            return new OpenApiDocument(content);
        }

        public string GetEndpoint(string serviceName, string methodName)
        {
            return string.Format("/{0}/{1}", serviceName, methodName);
        }

        public string GetRequestContentType(string endpoint)
        {
            var post = Content.Value<JObject>("paths")!.Value<JObject>(endpoint)!.Value<JObject>("post")!;

            post.ShouldNotBeNull(endpoint);

            var body = post.Value<JObject>("requestBody");
            if (body == null)
            {
                return MediaTypeNames.Application.Json + "+servicemodel.grpc";
            }

            var contentType = body.Value<JObject>("content")!.Properties().First().Name;

            return contentType;
        }
    }
}
