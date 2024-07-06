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
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

[TestFixture]
public class ChannelProviderFactoryTest
{
    private CallInvoker _callInvoker = null!;
    private ChannelBase _channel = null!;
    private Mock<IServiceProvider> _serviceProvider = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict).Object;

        var channel = new Mock<ChannelBase>(MockBehavior.Strict, "dummy target");
        channel
            .Setup(c => c.CreateCallInvoker())
            .Returns(_callInvoker);
        _channel = channel.Object;

        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
    }

    [Test]
    public void TransientCallInvoker()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(CallInvoker)))
            .Returns(_callInvoker);
        var sut = ChannelProviderFactory.Transient(provider => provider.GetRequiredService<CallInvoker>());

        var actual = sut.GetCallInvoker(_serviceProvider.Object);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void TransientChannel()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(ChannelBase)))
            .Returns(_channel);
        var sut = ChannelProviderFactory.Transient(provider => provider.GetRequiredService<ChannelBase>());

        var actual = sut.GetCallInvoker(_serviceProvider.Object);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void SingletonCallInvoker()
    {
        var sut = ChannelProviderFactory.Singleton(_callInvoker);

        var actual = sut.GetCallInvoker(null!);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void SingletonChannel()
    {
        var sut = ChannelProviderFactory.Singleton(_channel);

        var actual = sut.GetCallInvoker(null!);

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

        var actual = sut.GetCallInvoker(_serviceProvider.Object);

        actual.ShouldBe(_callInvoker);
    }

    [Test]
    public void DefaultChannel()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(ChannelBase)))
            .Returns(_channel);
        var sut = ChannelProviderFactory.Default();

        var actual = sut.GetCallInvoker(_serviceProvider.Object);

        actual.ShouldBe(_callInvoker);
    }
}