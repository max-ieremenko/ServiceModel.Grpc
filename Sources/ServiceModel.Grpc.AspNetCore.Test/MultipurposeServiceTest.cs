// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore;

[TestFixture(GrpcChannelType.GrpcCore)]
[TestFixture(GrpcChannelType.GrpcDotNet)]
public class MultipurposeServiceTest : MultipurposeServiceTestBase
{
    private readonly GrpcChannelType _channelType;
    private KestrelHost _host = null!;
    private Greeter.GreeterClient _greeterService = null!;

    public MultipurposeServiceTest(GrpcChannelType channelType)
    {
        _channelType = channelType;
    }

    [OneTimeSetUp]
    public async Task BeforeAll()
    {
        _host = await new KestrelHost()
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<MultipurposeService>();
            })
            .WithChannelType(_channelType)
            .StartAsync()
            .ConfigureAwait(false);

        DomainService = _host.ClientFactory.CreateClient<IMultipurposeService>(_host.Channel);
        _greeterService = new Greeter.GreeterClient(_host.Channel);
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task NativeGreet()
    {
        var actual = await _greeterService.UnaryAsync(new HelloRequest { Name = "world" }).ResponseAsync.ConfigureAwait(false);

        actual.Message.ShouldBe("Hello world!");
    }
}