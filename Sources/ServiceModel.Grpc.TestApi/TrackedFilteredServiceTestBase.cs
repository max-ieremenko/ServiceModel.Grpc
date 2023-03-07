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

using System;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi;

public abstract class TrackedFilteredServiceTestBase
{
    private IFilteredService DomainService { get; set; } = null!;

    [Test]
    public void UnarySync()
    {
        var track = DomainService.UnarySync(new[] { nameof(IFilteredService.UnarySync) });

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.UnarySync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation",
            "method-after",
            "service-after",
            "service-options-after",
            "global-server-after",
            "client-options-after",
            "global-client-after"
        });
    }

    [Test]
    public async Task UnaryAsync()
    {
        var track = await DomainService
            .UnaryAsync(new[] { nameof(IFilteredService.UnaryAsync) })
            .ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.UnaryAsync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation",
            "method-after",
            "service-after",
            "service-options-after",
            "global-server-after",
            "client-options-after",
            "global-client-after"
        });
    }

    [Test]
    public async Task ClientStreamAsync()
    {
        var stream = new[] { 1, 2, 3 }.AsAsyncEnumerable();
        var track = await DomainService
            .ClientStreamAsync(stream, new[] { nameof(IFilteredService.ClientStreamAsync) })
            .ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.ClientStreamAsync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation 6",
            "method-after",
            "service-after",
            "service-options-after",
            "global-server-after",
            "client-options-after",
            "global-client-after"
        });
    }

    [Test]
    public async Task ServerStreamSync()
    {
        var stream = DomainService.ServerStreamSync(new[] { nameof(IFilteredService.ServerStreamSync) });

        var track = await stream.ToListAsync().ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.ServerStreamSync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation",
            "client-options-after",
            "global-client-after"
        });
    }

    [Test]
    public async Task ServerStreamAsync()
    {
        var (stream, track) = await DomainService
            .ServerStreamAsync(new[] { nameof(IFilteredService.ServerStreamAsync) })
            .ConfigureAwait(false);

        var data = await stream.ToListAsync().ConfigureAwait(false);
        data.ShouldBe(new[] { 3, 2, 1 });

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.ServerStreamAsync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation",
            "method-after",
            "service-after",
            "service-options-after",
            "global-server-after",
            "client-options-after",
            "global-client-after"
        });
    }

    [Test]
    public async Task DuplexStreamAsync()
    {
        var inputStream = new[] { 1, 2, 3 }.AsAsyncEnumerable();
        var (outStream, track) = await DomainService
            .DuplexStreamAsync(inputStream, new[] { nameof(IFilteredService.DuplexStreamAsync) })
            .ConfigureAwait(false);

        var data = await outStream.ToListAsync().ConfigureAwait(false);
        data.ShouldBe(new[] { 3, 2, 1 });

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.DuplexStreamAsync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation 6",
            "method-after",
            "service-after",
            "service-options-after",
            "global-server-after",
            "client-options-after",
            "global-client-after"
        });
    }

    [Test]
    public async Task DuplexStreamSync()
    {
        var stream = DomainService.DuplexStreamSync(
            Array.Empty<string>().AsAsyncEnumerable(),
            new[] { nameof(IFilteredService.DuplexStreamSync) });

        var track = await stream.ToListAsync().ConfigureAwait(false);

        track.ShouldBe(new[]
        {
            nameof(IFilteredService.DuplexStreamSync),
            "global-client-before",
            "client-options-before",
            "global-server-before",
            "service-options-before",
            "service-before",
            "method-before",
            "implementation",
            "client-options-after",
            "global-client-after"
        });
    }

    protected void ConfigureClientFactory(ServiceModelGrpcClientOptions configuration)
    {
        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

        serviceProvider
            .Setup(p => p.GetService(typeof(TrackingClientFilter)))
            .Returns(new TrackingClientFilter("global-client"));

        configuration.ServiceProvider = serviceProvider.Object;

        configuration.Filters.Add(
            1,
            provider => provider.GetService(typeof(TrackingClientFilter)).ShouldBeOfType<TrackingClientFilter>());
    }

    protected void InitializeClient(IClientFactory clientFactory, ChannelBase channel)
    {
        DomainService = clientFactory.CreateClient<IFilteredService>(channel);
    }
}