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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore.NSwag;

[TestFixture]
public class HeadersHandlingTest
{
    private KestrelHost _host = null!;
    private SwaggerUiClient _client = null!;

    [OneTimeSetUp]
    public async Task BeforeAll()
    {
        _host = await new KestrelHost()
            .ConfigureServices(services =>
            {
                services.AddServiceModelGrpcSwagger();
                services.AddOpenApiDocument();
                services.AddMvc();

                services.AddTransient<IHeadersService, HeadersService>();
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
                endpoints.MapGrpcService<IHeadersService>();
            })
            .StartAsync(HttpProtocols.Http1)
            .ConfigureAwait(false);

        var document = await OpenApiDocument
            .DownloadAsync(_host.GetLocation("swagger/v1/swagger.json"))
            .ConfigureAwait(false);

        _client = new SwaggerUiClient(document, nameof(IHeadersService), _host.GetLocation());
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task UnaryCall()
    {
        var headers = new Dictionary<string, string>
        {
            { HeadersService.DefaultHeaderName, HeadersService.DefaultHeaderValue },
            { HeadersService.CallHeaderName, HeadersService.CallHeaderValue }
        };
        var parameters = new Dictionary<string, object>();

        var response = await _client.InvokeAsync(nameof(IHeadersService.UnaryCall), parameters, headers).ConfigureAwait(false);

        response.GetValues(HeadersService.DefaultHeaderName).ShouldBe(new[] { HeadersService.DefaultHeaderValue });
        response.GetValues(HeadersService.CallHeaderName).ShouldBe(new[] { HeadersService.CallHeaderValue });
        response.GetValues(HeadersService.CallTrailerName).ShouldBe(new[] { HeadersService.CallTrailerValue });
    }

    [Test]
    public async Task UnaryCallAsync()
    {
        var headers = new Dictionary<string, string>
        {
            { HeadersService.DefaultHeaderName, HeadersService.DefaultHeaderValue },
            { HeadersService.CallHeaderName, HeadersService.CallHeaderValue }
        };
        var parameters = new Dictionary<string, object>();

        var response = await _client.InvokeAsync(nameof(IHeadersService.UnaryCallAsync), parameters, headers).ConfigureAwait(false);

        response.GetValues(HeadersService.DefaultHeaderName).ShouldBe(new[] { HeadersService.DefaultHeaderValue });
        response.GetValues(HeadersService.CallHeaderName).ShouldBe(new[] { HeadersService.CallHeaderValue });
        response.GetValues(HeadersService.CallTrailerName).ShouldBe(new[] { HeadersService.CallTrailerValue });
    }
}