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

using Grpc.Core;
using Grpc.Core.Interceptors;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.SelfHost;

[TestFixture]
public partial class ServerInterceptorTest
{
    private ServerHost _host = null!;
    private IMultipurposeService _domainService = null!;

    [OneTimeSetUp]
    public void BeforeAll()
    {
        _host = new ServerHost(GrpcChannelType.GrpcCore);

        _host.Services.AddServiceModelSingleton<IMultipurposeService>(
            new MultipurposeService(),
            options =>
            {
                options.ConfigureServiceDefinition = definition => definition.Intercept(new HackInterceptor());
            });
        _host.Start();

        _domainService = new ClientFactory().CreateClient<IMultipurposeService>(_host.Channel);
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task BlockingCallAsync()
    {
        var result = await _domainService.BlockingCallAsync(default, 1, "dummy").ConfigureAwait(false);

        result.ShouldBe("dummy_h_1");
    }
}