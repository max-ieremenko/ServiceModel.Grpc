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
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.SelfHost.Internal;

[TestFixture]
public class ServiceProviderExtensionsTest
{
    private Mock<IServiceProvider> _serviceProvider = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
    }

    [Test]
    public void ResolveFromProvider()
    {
        var service = new Mock<IDisposable>(MockBehavior.Strict);

        _serviceProvider
            .Setup(p => p.GetService(typeof(IDisposable)))
            .Returns(service.Object);

        _serviceProvider.Object.GetServiceRequired(typeof(IDisposable)).ShouldBe(service.Object);
    }

    [Test]
    public void FailToResolve()
    {
        _serviceProvider
            .Setup(p => p.GetService(typeof(IDisposable)))
            .Returns(null!);

        Assert.Throws<InvalidOperationException>(() => _serviceProvider.Object.GetServiceRequired(typeof(IDisposable)));
    }
}