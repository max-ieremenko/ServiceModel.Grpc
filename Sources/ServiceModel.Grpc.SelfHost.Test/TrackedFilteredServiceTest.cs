﻿// <copyright>
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
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.SelfHost;

[TestFixture]
public class TrackedFilteredServiceTest : TrackedFilteredServiceTestBase
{
    private ServerHost _host = null!;

    [OneTimeSetUp]
    public void BeforeAll()
    {
        _host = new ServerHost();

        _host.Services.AddServiceModelSingleton(
            new TrackedFilteredService(),
            options =>
            {
                options.ServiceProvider = new Mock<IServiceProvider>().Object;
                options.Filters.Add(1, _ => new TrackingServerFilter("global-server"));
                options.Filters.Add(2, _ => new TrackingServerFilter("service-options"));
            });
        _host.Start();

        var defaultOptions = new ServiceModelGrpcClientOptions();
        ConfigureClientFactory(defaultOptions);

        var clientFactory = new ClientFactory(defaultOptions);
        clientFactory.AddClient<IFilteredService>(options =>
        {
            options.Filters.Add(2, new TrackingClientFilter("client-options"));
        });

        InitializeClient(clientFactory, _host.Channel);
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _host.DisposeAsync().ConfigureAwait(false);
    }
}