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
using ServiceModel.Grpc.Benchmarks.Domain;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Combined;

internal sealed class MagicOnionCombinedCallTest : IUnaryCallTest
{
    private readonly SomeObject _payload;
    private readonly TestServerHost _host;
    private ITestServiceMagicOnion _proxy = null!;

    public MagicOnionCombinedCallTest(SomeObject payload)
    {
        _payload = payload;

        _host = new TestServerHost()
            .ConfigureServices(services =>
            {
                services.AddGrpc();
                services.AddMagicOnion();
            })
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService<TestServiceMagicOnionStub>();
            });
    }

    public async Task StartAsync()
    {
        await _host.StartAsync().ConfigureAwait(false);
        _proxy = MagicOnion.Client.MagicOnionClient.Create<ITestServiceMagicOnion>(_host.GetGrpcChannel());
    }

    public async Task PingPongAsync()
    {
        var call = _proxy.PingPong(_payload);
        await call.ResponseAsync.ConfigureAwait(false);
        call.Dispose();
    }

    public ValueTask DisposeAsync() => _host.DisposeAsync();
}