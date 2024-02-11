// <copyright>
// Copyright 2024 Max Ieremenko
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

#if !NET7_0
using System;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Client.DependencyInjection.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Client.DependencyInjection;

[TestFixture]
public class ClientKeyedServiceCollectionExtensions80Test
{
    private object _key = null!;
    private CallInvoker _defaultCallInvoker = null!;
    private CallInvoker _keyedCallInvoker = null!;
    private IMarshallerFactory _defaultMarshallerFactory = null!;
    private IMarshallerFactory _keyedMarshallerFactory = null!;
    private Mock<IClientFactory> _defaultClientFactory = null!;
    private Mock<IClientFactory> _keyedClientFactory = null!;
    private IContract _defaultClient = null!;
    private IContract _keyedClient = null!;
    private ServiceCollection _services = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _key = new object();

        _defaultCallInvoker = new Mock<CallInvoker>(MockBehavior.Strict).Object;
        _keyedCallInvoker = new Mock<CallInvoker>(MockBehavior.Strict).Object;

        _defaultMarshallerFactory = new Mock<IMarshallerFactory>().Object;
        _keyedMarshallerFactory = new Mock<IMarshallerFactory>().Object;

        _defaultClientFactory = new Mock<IClientFactory>(MockBehavior.Strict);
        _keyedClientFactory = new Mock<IClientFactory>(MockBehavior.Strict);

        _defaultClient = new Mock<IContract>(MockBehavior.Strict).Object;
        _keyedClient = new Mock<IContract>(MockBehavior.Strict).Object;

        _services = new ServiceCollection();
    }

    [Test]
    public void FactoryOptions()
    {
        _services
            .AddServiceModelGrpcClientFactory((options, _) =>
            {
                options.MarshallerFactory = _defaultMarshallerFactory;
            });

        _services
            .AddKeyedServiceModelGrpcClientFactory(
                _key,
                (options, _) =>
                {
                    options.MarshallerFactory = _keyedMarshallerFactory;
                });

        OverrideClientFactory(
            null,
            options =>
            {
                options.MarshallerFactory.ShouldBe(_defaultMarshallerFactory);
            });

        OverrideClientFactory(
            _key,
            options =>
            {
                options.MarshallerFactory.ShouldBe(_keyedMarshallerFactory);
            });

        var provider = _services.BuildProviderWithValidations();

        provider.GetRequiredKeyedService<IClientFactory>(null).ShouldBe(_defaultClientFactory.Object);
        provider.GetRequiredKeyedService<IClientFactory>(_key).ShouldBe(_keyedClientFactory.Object);
    }

    [Test]
    public void ClientWithInvokerFromFactory()
    {
        _services
            .AddServiceModelGrpcClientFactory()
            .ConfigureDefaultChannel(ChannelProviderFactory.Singleton(_defaultCallInvoker))
            .AddClient<IContract>();

        _services
            .AddKeyedServiceModelGrpcClientFactory(_key)
            .ConfigureDefaultChannel(ChannelProviderFactory.Singleton(_keyedCallInvoker))
            .AddClient<IContract>();

        OverrideClientFactory(null);
        OverrideClientFactory(_key);
        OverrideClient(null);
        OverrideClient(_key);

        var provider = _services.BuildProviderWithValidations();

        provider.GetRequiredService<IContract>().ShouldBe(_defaultClient);
        provider.GetRequiredKeyedService<IContract>(_key).ShouldBe(_keyedClient);
    }

    [Test]
    public void ClientWithPrivateInvoker()
    {
        _services
            .AddServiceModelGrpcClientFactory()
            .AddClient<IContract>(channel: ChannelProviderFactory.Singleton(_defaultCallInvoker));

        _services
            .AddKeyedServiceModelGrpcClient<IContract>(_key, channel: ChannelProviderFactory.Singleton(_keyedCallInvoker));

        OverrideClientFactory(null);
        OverrideClientFactory(_key);
        OverrideClient(null);
        OverrideClient(_key);

        var provider = _services.BuildProviderWithValidations();

        provider.GetRequiredService<IContract>().ShouldBe(_defaultClient);
        provider.GetRequiredKeyedService<IContract>(_key).ShouldBe(_keyedClient);
    }

    [Test]
    public void ClientWithUserOptions()
    {
        _services.AddServiceModelGrpcClient<IContract>(
            (options, provider) =>
            {
                options.MarshallerFactory = _defaultMarshallerFactory;
            },
            ChannelProviderFactory.Singleton(_defaultCallInvoker));

        _services.AddKeyedServiceModelGrpcClient<IContract>(
            _key,
            (options, provider) =>
            {
                options.MarshallerFactory = _keyedMarshallerFactory;
            },
            ChannelProviderFactory.Singleton(_keyedCallInvoker));

        OverrideClientFactory(null);
        OverrideClientFactory(_key);
        OverrideClient(
            null,
            options =>
            {
                options.MarshallerFactory.ShouldBe(_defaultMarshallerFactory);
            });
        OverrideClient(
            _key,
            options =>
            {
                options.MarshallerFactory.ShouldBe(_keyedMarshallerFactory);
            });

        var provider = _services.BuildProviderWithValidations();

        provider.GetRequiredService<IContract>().ShouldBe(_defaultClient);
        provider.GetRequiredKeyedService<IContract>(_key).ShouldBe(_keyedClient);
    }

    [Test]
    public void AnyKeyRegistration()
    {
        _services
            .AddKeyedServiceModelGrpcClientFactory(
                KeyedService.AnyKey,
                (options, _) =>
                {
                    options.MarshallerFactory = _keyedMarshallerFactory;
                })
            .ConfigureDefaultChannel(ChannelProviderFactory.Singleton(_keyedCallInvoker))
            .AddClient<IContract>();

        OverrideClientFactory(
            KeyedService.AnyKey,
            options =>
            {
                options.MarshallerFactory.ShouldBe(_keyedMarshallerFactory);
            });
        OverrideClient(KeyedService.AnyKey);

        var provider = _services.BuildProviderWithValidations();

        provider.GetRequiredKeyedService<IClientFactory>("foo").ShouldBe(_keyedClientFactory.Object);
        provider.GetRequiredKeyedService<IClientFactory>("bar").ShouldBe(_keyedClientFactory.Object);

        provider.GetRequiredKeyedService<IContract>("foo").ShouldBe(_keyedClient);
        provider.GetRequiredKeyedService<IContract>("bar").ShouldBe(_keyedClient);
    }

    [Test]
    public void AnyKeyRegistrationDefaultInvoker()
    {
        _services.AddKeyedSingleton(KeyedService.AnyKey, _keyedCallInvoker);

        _services
            .AddKeyedServiceModelGrpcClientFactory(KeyedService.AnyKey)
            .AddClient<IContract>();

        OverrideClientFactory(KeyedService.AnyKey);
        OverrideClient(KeyedService.AnyKey);

        var provider = _services.BuildProviderWithValidations();

        provider.GetRequiredKeyedService<IClientFactory>("foo").ShouldBe(_keyedClientFactory.Object);
        provider.GetRequiredKeyedService<IContract>("foo").ShouldBe(_keyedClient);
    }

    private void OverrideClientFactory(object? key, Action<ServiceModelGrpcClientOptions>? configure = null)
    {
        var clientFactory = key == null ? _defaultClientFactory : _keyedClientFactory;
        Func<ServiceModelGrpcClientOptions, IClientFactory> test = options =>
        {
            options.ShouldNotBeNull();
            configure?.Invoke(options);
            return clientFactory.Object;
        };

        _services.AddKeyedSingleton(key, (provider, k) => new ClientFactoryResolver(KeyedServiceExtensions.GetOptionsKey(key)).Create(provider, test));
    }

    private void OverrideClient(object? key, Action<ServiceModelGrpcClientOptions>? configure = null)
    {
        var clientFactory = key == null ? _defaultClientFactory : _keyedClientFactory;
        var callInvoker = key == null ? _defaultCallInvoker : _keyedCallInvoker;
        var client = key == null ? _defaultClient : _keyedClient;

        clientFactory
            .Setup(f => f.AddClient<IContract>(It.IsAny<Action<ServiceModelGrpcClientOptions>>()))
            .Callback<Action<ServiceModelGrpcClientOptions>>(setup =>
            {
                if (configure != null)
                {
                    setup.ShouldNotBeNull();

                    var options = new ServiceModelGrpcClientOptions();
                    setup(options);
                    configure(options);
                }
            });

        clientFactory
            .Setup(f => f.CreateClient<IContract>(callInvoker))
            .Returns(client);
    }
}
#endif