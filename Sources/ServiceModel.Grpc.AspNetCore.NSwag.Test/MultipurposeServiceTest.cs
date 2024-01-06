// <copyright>
// Copyright 2020-2021 Max Ieremenko
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

using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

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
            })
            .ConfigureApp(app =>
            {
                app.UseOpenApi(); // serve OpenAPI/Swagger documents
                app.UseSwaggerUi(); // serve Swagger UI
                app.UseReDoc(); // serve ReDoc UI

                app.UseServiceModelGrpcSwaggerGateway();
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
}