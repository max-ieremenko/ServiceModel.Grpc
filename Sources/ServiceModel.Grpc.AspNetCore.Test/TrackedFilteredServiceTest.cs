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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore;

[TestFixture]
public class TrackedFilteredServiceTest : TrackedFilteredServiceTestBase
{
    private KestrelHost _host = null!;

    [OneTimeSetUp]
    public async Task BeforeAll()
    {
        _host = await new KestrelHost()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new TrackingServerFilter("global-server"));

                services.AddServiceModelGrpc(options =>
                {
                    options.Filters.Add(1, provider => provider.GetRequiredService<TrackingServerFilter>());
                });
                services.AddServiceModelGrpcServiceOptions<TrackedFilteredService>(options =>
                {
                    options.Filters.Add(2, _ => new TrackingServerFilter("service-options"));
                });
            })
            .ConfigureClientFactory(ConfigureClientFactory)
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TrackedFilteredService>();
            })
            .StartAsync()
            .ConfigureAwait(false);

        _host.ClientFactory.AddClient<IFilteredService>(options =>
        {
            options.Filters.Add(2, new TrackingClientFilter("client-options"));
        });

        InitializeClient(_host.ClientFactory, _host.Channel);
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }
}