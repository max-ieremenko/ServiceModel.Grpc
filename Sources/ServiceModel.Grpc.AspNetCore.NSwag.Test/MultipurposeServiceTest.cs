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

using System.IO.Compression;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.AspNetCore.TestApi.Domain;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore.NSwag;

[TestFixture]
public class MultipurposeServiceTest
{
    private KestrelHost _host = null!;
    private SwaggerUiClient _client = null!;

    [OneTimeSetUp]
    public async Task BeforeAll()
    {
        _host = await new KestrelHost()
            .ConfigureServices(services =>
            {
                services.AddGrpc(options =>
                {
                    options.ResponseCompressionAlgorithm = options.CompressionProviders[0].EncodingName;
                    options.ResponseCompressionLevel = CompressionLevel.Optimal;
                });
                services.AddServiceModelGrpcSwagger();
                services.AddOpenApiDocument();
                services.AddMvc();
                services.AddTransient<CustomResponseMiddleware>();
            })
            .ConfigureApp(app =>
            {
                app.UseOpenApi(); // serve OpenAPI/Swagger documents
                app.UseSwaggerUi(); // serve Swagger UI
                app.UseReDoc(); // serve ReDoc UI

                app.UseServiceModelGrpcSwaggerGateway();

                app.UseMiddleware<CustomResponseMiddleware>();
            })
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<MultipurposeService>();
            })
            .StartAsync(HttpProtocols.Http1)
            .ConfigureAwait(false);

        var document = await OpenApiDocument
            .DownloadAsync(_host.GetLocation("swagger/v1/swagger.json"))
            .ConfigureAwait(false);

        _client = new SwaggerUiClient(document, nameof(IMultipurposeService), _host.GetLocation());
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task Concat()
    {
        var headers = new Dictionary<string, string>
        {
            { "value", "b" }
        };
        var parameters = new Dictionary<string, object>
        {
            { "value", "a" }
        };

        var actual = await _client.InvokeAsync<string>(nameof(IMultipurposeService.Concat), parameters, headers).ConfigureAwait(false);

        actual.ShouldBe("ab");
    }

    [Test]
    public async Task ConcatAsync()
    {
        var headers = new Dictionary<string, string>
        {
            { "value", "b" }
        };
        var parameters = new Dictionary<string, object>
        {
            { "value", "a" }
        };

        var actual = await _client.InvokeAsync<string>(nameof(IMultipurposeService.ConcatAsync), parameters, headers).ConfigureAwait(false);

        actual.ShouldBe("ab");
    }

    [Test]
    public async Task Sum5ValuesAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "x1", 1 },
            { "x2", 2 },
            { "x3", 3 },
            { "x4", 4 },
            { "x5", 5 }
        };

        var actual = await _client.InvokeAsync<int>(nameof(IMultipurposeService.Sum5ValuesAsync), parameters).ConfigureAwait(false);

        actual.ShouldBe(15);
    }

    [Test]
    public async Task BlockingCallAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "x", 1 },
            { "y", "a" }
        };

        var actual = await _client.InvokeAsync<string>(nameof(IMultipurposeService.BlockingCallAsync), parameters);

        actual.ShouldBe("a1");
    }

    [Test]
    [TestCase(HttpStatusCode.Unauthorized, "application/grpc")]
    [TestCase(HttpStatusCode.OK, "text/plain")]
    public async Task NonGrpcResponseAsync(HttpStatusCode statusCode, string contentType)
    {
        var headers = new Dictionary<string, string>
        {
            { CustomResponseMiddleware.HeaderResponseStatusCode, statusCode.ToString() },
            { CustomResponseMiddleware.HeaderContentType, contentType },
            { CustomResponseMiddleware.HeaderResponseBody, "some message" }
        };

        var parameters = new Dictionary<string, object>
        {
            { "x", 1 },
            { "y", "a" }
        };

        var response = await _client.PostAsync(nameof(IMultipurposeService.BlockingCallAsync), parameters, headers);

        response.StatusCode.ShouldBe(statusCode);
        response.Content.Headers.ContentType!.MediaType.ShouldBe(contentType);
        (await response.Content.ReadAsStringAsync()).ShouldBe("some message");
    }
}