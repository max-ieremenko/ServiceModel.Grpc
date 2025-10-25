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
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Benchmarks.Api;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Combined;

internal sealed class ServiceModelGrpcProtoCombinedCallTest : IUnaryCallTest
{
    private readonly SomeObjectProto _payload;
    private readonly TestServerHost _host;
    private ITestService _proxy = null!;

    public ServiceModelGrpcProtoCombinedCallTest(SomeObjectProto payload)
    {
        _payload = payload;

        _host = new TestServerHost()
            .ConfigureServices(services =>
            {
                services.AddServiceModelGrpc(options => options.DefaultMarshallerFactory = GoogleProtoMarshallerFactory.Default);
            })
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TestServiceStub>();
            });
    }

    public async Task StartAsync()
    {
        await _host.StartAsync().ConfigureAwait(false);

        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions { MarshallerFactory = GoogleProtoMarshallerFactory.Default });
        _proxy = clientFactory.CreateClient<ITestService>(_host.GetGrpcChannel());
    }

    public Task PingPongAsync() => _proxy.PingPongProto(_payload);

    public ValueTask DisposeAsync() => _host.DisposeAsync();
}