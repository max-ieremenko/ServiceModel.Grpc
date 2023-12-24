// <copyright>
// Copyright 2023 Max Ieremenko
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
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Client.DependencyInjection.Internal;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Client.DependencyInjection;

[TestFixture]
public class ClientServiceCollectionExtensionsTest
{
    private CallInvoker _callInvoker = null!;
    private Mock<IClientFactory> _clientFactory = null!;
    private IClientBuilder<IContract> _clientBuilder = null!;
    private Mock<IContract> _client = null!;
    private ServiceCollection _services = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict).Object;
        _client = new Mock<IContract>(MockBehavior.Strict);
        _clientFactory = new Mock<IClientFactory>(MockBehavior.Strict);
        _clientBuilder = new Mock<IClientBuilder<IContract>>(MockBehavior.Strict).Object;

        _services = new ServiceCollection();
    }

    [Test]
    public void FactoryWithDefaultOptions()
    {
        _services.AddServiceModelGrpcClientFactory();

        OverrideClientFactory(options =>
        {
            options.ServiceProvider.ShouldNotBeNull();
        });

        var provider = _services.BuildServiceProvider();
        provider.GetService<IOptions<ServiceModelGrpcClientOptions>>().ShouldBeNull();

        provider.GetService<IClientFactory>().ShouldBe(_clientFactory.Object);
    }

    [Test]
    public void FactoryWithUserOptions()
    {
        var marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict).Object;
        var errorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;
        var stack = new List<string>();

        _services.AddServiceModelGrpcClientFactory((options, provider) =>
        {
            options.ShouldNotBeNull();
            provider.ShouldNotBeNull();

            options.MarshallerFactory = marshallerFactory;

            stack.Add("1");
        });

        _services.AddServiceModelGrpcClientFactory((options, provider) =>
        {
            options.ShouldNotBeNull();
            provider.ShouldNotBeNull();

            options.MarshallerFactory.ShouldBe(marshallerFactory);
            options.ErrorHandler = errorHandler;

            stack.Add("2");
        });

        OverrideClientFactory(options =>
        {
            options.MarshallerFactory.ShouldBe(marshallerFactory);
            options.ErrorHandler.ShouldBe(errorHandler);
        });

        var provider = _services.BuildServiceProvider();
        provider.GetService<IOptions<ServiceModelGrpcClientOptions>>().ShouldNotBeNull();

        stack.ShouldBeEmpty();

        provider.GetService<IClientFactory>().ShouldBe(_clientFactory.Object);
        stack.ShouldBe(new[] { "1", "2" });
    }

    [Test]
    public void OverrideCustomFactoryRegistration()
    {
        _services.AddTransient<IClientFactory>(_ => throw new InvalidOperationException());

        _services.AddServiceModelGrpcClientFactory();

        var provider = _services.BuildServiceProvider();
        provider.GetRequiredService<IClientFactory>().ShouldBeOfType<ClientFactory>();
    }

    [Test]
    public void ClientWithDefaultOptionsAndDefaultInvoker()
    {
        _services.AddServiceModelGrpcClient<IContract>();

        _services.AddSingleton(_callInvoker);
        OverrideClientFactory();
        OverrideClient();

        var provider = _services.BuildServiceProvider();

        provider.GetRequiredService<IContract>().ShouldBe(_client.Object);
    }

    [Test]
    public void ClientWithInvokerFromFactory()
    {
        _services
            .AddServiceModelGrpcClientFactory()
            .ConfigureDefaultChannel(ChannelProviderFactory.Singleton(_callInvoker))
            .AddClient<IContract>();

        OverrideClientFactory();
        OverrideClient();

        var provider = _services.BuildServiceProvider();

        provider.GetRequiredService<IContract>().ShouldBe(_client.Object);
    }

    [Test]
    public void ClientWithPrivateInvoker()
    {
        _services.AddServiceModelGrpcClient<IContract>(channel: ChannelProviderFactory.Singleton(_callInvoker));

        OverrideClientFactory();
        OverrideClient();

        var provider = _services.BuildServiceProvider();

        provider.GetRequiredService<IContract>().ShouldBe(_client.Object);
    }

    [Test]
    public void ClientWithUserOptions()
    {
        var marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict).Object;
        var errorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;
        var stack = new List<string>();

        _services.AddServiceModelGrpcClient<IContract>((options, provider) =>
        {
            provider.ShouldNotBeNull();
            options.MarshallerFactory = marshallerFactory;
            stack.Add("1");
        });

        _services.AddServiceModelGrpcClient<IContract>((options, provider) =>
        {
            provider.ShouldNotBeNull();
            options.ErrorHandler = errorHandler;
            stack.Add("2");
        });

        _services.AddSingleton(_callInvoker);
        OverrideClientFactory();
        OverrideClient(options =>
        {
            options.MarshallerFactory.ShouldBe(marshallerFactory);
            options.ErrorHandler.ShouldBe(errorHandler);
        });

        var provider = _services.BuildServiceProvider();

        provider.GetRequiredService<IContract>().ShouldBe(_client.Object);
        stack.ShouldBe(new[] { "1", "2" });
    }

    [Test]
    public void ClientWithBuilder()
    {
        ClientServiceCollectionExtensions.AddServiceModelGrpcClientBuilder(_services, _clientBuilder, null, null);

        _services.AddSingleton(_callInvoker);
        OverrideClientFactory();
        OverrideClientBuilder();

        var provider = _services.BuildServiceProvider();

        provider.GetRequiredService<IContract>().ShouldBe(_client.Object);
    }

    private void OverrideClientFactory(Action<ServiceModelGrpcClientOptions>? configure = null)
    {
        var resolver = _services
            .Single(i => i.ServiceType == typeof(IClientFactory))
            .ImplementationFactory
            ?.Target
            .ShouldBeOfType<ClientFactoryResolver>();

        Func<ServiceModelGrpcClientOptions, IClientFactory> test = options =>
        {
            options.ShouldNotBeNull();
            configure?.Invoke(options);
            return _clientFactory.Object;
        };

        _services.AddSingleton(provider => resolver!.Build(provider, test));
    }

    private void OverrideClientBuilder()
    {
        _clientFactory
            .Setup(f => f.AddClient(_clientBuilder, null))
            .Callback(() =>
            {
                _clientFactory
                    .Setup(f => f.CreateClient<IContract>(_callInvoker))
                    .Returns(_client.Object);
            });
    }

    private void OverrideClient(Action<ServiceModelGrpcClientOptions>? configure = null)
    {
        if (configure != null)
        {
            _clientFactory
                .Setup(f => f.AddClient<IContract>(It.IsNotNull<Action<ServiceModelGrpcClientOptions>>()))
                .Callback<Action<ServiceModelGrpcClientOptions>>(setup =>
                {
                    var options = new ServiceModelGrpcClientOptions();
                    setup(options);
                    configure(options);
                });
        }

        _clientFactory
            .Setup(f => f.CreateClient<IContract>(_callInvoker))
            .Returns(_client.Object);
    }
}