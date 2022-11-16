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

using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.DesignTime.Generator.Test.SelfHost;

[TestFixture]
[ExportGrpcService(typeof(HeadersService), GenerateSelfHostExtensions = true)]
[ImportGrpcService(typeof(IHeadersService))]
public partial class HeadersHandlingTest : HeadersHandlingTestBase
{
    private const int Port = 8080;
    private Server _server = null!;
    private GrpcChannel _channel = null!;

    [OneTimeSetUp]
    public void BeforeAll()
    {
        var provider = new ServiceCollection().AddTransient<HeadersService>().BuildServiceProvider();

        _server = new Server
        {
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        _channel = new GrpcChannel("localhost", Port, ChannelCredentials.Insecure);

        AddHeadersService(_server.Services, provider);

        var clientFactory = new ClientFactory();
        AddHeadersServiceClient(clientFactory, options => options.DefaultCallOptionsFactory = () => new CallOptions(DefaultMetadata));

        DomainService = clientFactory.CreateClient<IHeadersService>(_channel);

        _server.Start();
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _channel.ShutdownAsync().ConfigureAwait(false);
        await _server.ShutdownAsync().ConfigureAwait(false);
    }
}