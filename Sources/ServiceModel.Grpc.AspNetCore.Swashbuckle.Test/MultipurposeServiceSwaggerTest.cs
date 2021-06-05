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

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.Swashbuckle.Internal;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.Swashbuckle
{
    [TestFixture]
    public class MultipurposeServiceSwaggerTest
    {
        private JObject _document = null!;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            var host = await new KestrelHost()
                .ConfigureServices(services =>
                {
                    services.AddServiceModelGrpcSwagger();
                    services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                        c.EnableAnnotations(true, true);
                        c.UseAllOfForInheritance();
                    });
                    services.AddMvc();
                })
                .ConfigureApp(app =>
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("v1/swagger.json", "My API V1");
                    });
                })
                .ConfigureEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<MultipurposeService>();
                })
                .StartAsync(HttpProtocols.Http1AndHttp2);

            await using (host)
            {
                using (var client = new HttpClient())
                {
                    var url = host.GetLocation("swagger/v1/swagger.json");
                    using (var response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            _document = (JObject)JsonSerializer.CreateDefault().Deserialize(new JsonTextReader(new StreamReader(stream)))!;
                        }
                    }
                }
            }
        }

        [Test]
        public void Sum5ValuesAsync()
        {
            var post = _document.Value<JObject>("paths")!.Value<JObject>("/IMultipurposeService/Sum5ValuesAsync")!.Value<JObject>("post")!;

            post.Value<JArray>("tags")!.Select(i => i.ToString()).ShouldBe(new[] { "IMultipurposeService" });

            var body = post.Value<JObject>("requestBody")!.Value<JObject>("content")!.Value<JObject>(ProtocolConstants.MediaTypeName)!;

            body.Value<JObject>("schema")!.Value<string>("type")!.ShouldBe("object");

            var properties = body.Value<JObject>("schema")!.Value<JObject>("properties")!;
            properties.Value<JObject>("x1")!.Value<string>("type").ShouldBe("integer");
            properties.Value<JObject>("x1")!.Value<string>("format").ShouldBe("int64");
            properties.Value<JObject>("x2")!.Value<string>("type").ShouldBe("integer");
            properties.Value<JObject>("x2")!.Value<string>("format").ShouldBe("int32");
        }
    }
}
