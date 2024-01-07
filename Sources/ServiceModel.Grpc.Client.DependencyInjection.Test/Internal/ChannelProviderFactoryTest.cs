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

using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

[TestFixture]
public class ChannelProviderFactoryTest
{
    private CallInvoker _callInvoker = null!;
    private ChannelBase _channel = null!;
    private Mock<IKeyedServiceProvider> _serviceProvider = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict).Object;

        var channel = new Mock<ChannelBase>(MockBehavior.Strict, "dummy target");
        channel
            .Setup(c => c.CreateCallInvoker())
            .Returns(_callInvoker);
        _channel = channel.Object;

        _serviceProvider = new Mock<IKeyedServiceProvider>(MockBehavior.Strict);
    }

    [Test]
    public void TransientCallInvoker()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(CallInvoker)))
            .Returns(_callInvoker);
        var sut = ChannelProviderFactory.Transient(provider => provider.GetRequiredService<CallInvoker>());

        var actual = sut.GetCallInvoker(_serviceProvider.Object, null);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void TransientKeyedCallInvoker()
    {
        var key = new object();

        _serviceProvider
            .Setup(p => p.GetRequiredKeyedService(typeof(CallInvoker), key))
            .Returns(_callInvoker);
        var sut = ChannelProviderFactory.KeyedTransient((provider, k) => provider.GetRequiredKeyedService<CallInvoker>(k));

        var actual = sut.GetCallInvoker(_serviceProvider.Object, key);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void TransientChannel()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(ChannelBase)))
            .Returns(_channel);
        var sut = ChannelProviderFactory.Transient(provider => provider.GetRequiredService<ChannelBase>());

        var actual = sut.GetCallInvoker(_serviceProvider.Object, null);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void TransientKeyedChannel()
    {
        var key = new object();

        _serviceProvider
            .Setup(p => p.GetRequiredKeyedService(typeof(ChannelBase), key))
            .Returns(_channel);
        var sut = ChannelProviderFactory.KeyedTransient((provider, k) => provider.GetRequiredKeyedService<ChannelBase>(k));

        var actual = sut.GetCallInvoker(_serviceProvider.Object, key);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void SingletonCallInvoker()
    {
        var sut = ChannelProviderFactory.Singleton(_callInvoker);

        var actual = sut.GetCallInvoker(null!, null);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void SingletonChannel()
    {
        var sut = ChannelProviderFactory.Singleton(_channel);

        var actual = sut.GetCallInvoker(null!, null);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void DefaultCallInvoker()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(ChannelBase)))
            .Returns((ChannelBase)null!);
        _serviceProvider
            .Setup(p => p.GetService(typeof(CallInvoker)))
            .Returns(_callInvoker);
        var sut = ChannelProviderFactory.Default();

        var actual = sut.GetCallInvoker(_serviceProvider.Object, null);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void DefaultKeyedCallInvoker()
    {
        var key = new object();

        _serviceProvider
            .Setup(p => p.GetKeyedService(typeof(ChannelBase), key))
            .Returns((ChannelBase)null!);
        _serviceProvider
            .Setup(p => p.GetKeyedService(typeof(CallInvoker), key))
            .Returns(_callInvoker);
        var sut = ChannelProviderFactory.Default();

        var actual = sut.GetCallInvoker(_serviceProvider.Object, key);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void DefaultChannel()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(ChannelBase)))
            .Returns(_channel);
        var sut = ChannelProviderFactory.Default();

        var actual = sut.GetCallInvoker(_serviceProvider.Object, null);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void DefaultKeyedChannel()
    {
        var key = new object();

        _serviceProvider
            .Setup(p => p.GetKeyedService(typeof(ChannelBase), key))
            .Returns(_channel);
        var sut = ChannelProviderFactory.Default();

        var actual = sut.GetCallInvoker(_serviceProvider.Object, key);

        actual.ShouldBe(_callInvoker);
    }
}