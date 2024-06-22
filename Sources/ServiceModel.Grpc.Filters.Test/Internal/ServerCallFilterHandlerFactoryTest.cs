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

using System;
using System.Reflection;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ServerCallFilterHandlerFactoryTest
{
    private Mock<IServiceProvider> _serviceProvider = null!;
    private Mock<IOperationDescription> _operation = null!;
    private MethodInfo _contractMethod = null!;
    private MethodInfo _implementationMethod = null!;
    private Func<IServiceProvider, IServerFilter>[] _filterFactories = null!;
    private object _service = null!;
    private Mock<ServerCallContext> _context = null!;
    private ServerCallFilterHandlerFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _filterFactories = new Func<IServiceProvider, IServerFilter>[1];

        _service = new object();
        _contractMethod = new Mock<MethodInfo>().Object;
        _implementationMethod = new Mock<MethodInfo>().Object;

        _operation = new Mock<IOperationDescription>(MockBehavior.Strict);
        _operation
            .Setup(d => d.GetRequestAccessor())
            .Returns(new Mock<IMessageAccessor>(MockBehavior.Strict).Object);
        _operation
            .Setup(d => d.GetRequestStreamAccessor())
            .Returns(new Mock<IStreamAccessor>(MockBehavior.Strict).Object);
        _operation
            .Setup(d => d.GetResponseAccessor())
            .Returns(new Mock<IMessageAccessor>(MockBehavior.Strict).Object);
        _operation
            .Setup(d => d.GetResponseStreamAccessor())
            .Returns(new Mock<IStreamAccessor>(MockBehavior.Strict).Object);
        _operation
            .Setup(d => d.GetContractMethod())
            .Returns(_contractMethod);
        _operation
            .Setup(d => d.GetImplementationMethod(_service))
            .Returns(_implementationMethod);

        _context = new Mock<ServerCallContext>(MockBehavior.Strict);
        _sut = new ServerCallFilterHandlerFactory(_serviceProvider.Object, _operation.Object, _filterFactories);
    }

    [Test]
    public void CreateHandler()
    {
        var filter = new Mock<IServerFilter>(MockBehavior.Strict);
        _filterFactories[0] = provider =>
        {
            provider.ShouldBe(_serviceProvider.Object);
            return filter.Object;
        };

        var actual = _sut.CreateHandler(_service, _context.Object);

        actual.Context.ServerCallContext.ShouldBe(_context.Object);
        actual.Context.ContractMethodInfo.ShouldBe(_contractMethod);
        actual.Context.ServiceMethodInfo.ShouldBe(_implementationMethod);
        actual.Context.ServiceInstance.ShouldBe(_service);
        actual.Context.ServiceProvider.ShouldBe(_serviceProvider.Object);

        actual.Context.Request.ShouldNotBeNull();
        actual.Context.Request.Stream.ShouldBeNull(); // must be set by call handler

        actual.Context.Response.ShouldNotBeNull();

        actual.Filters.Length.ShouldBe(1);
        actual.Filters[0].ShouldBe(filter.Object);
    }

    [Test]
    public void FilterFactoryReturnsNull()
    {
        _filterFactories[0] = _ => null!;

        Assert.Throws<InvalidOperationException>(() => _sut.CreateHandler(_service, _context.Object));
    }

    [Test]
    public void FilterFactoryThrows()
    {
        _filterFactories[0] = _ => throw new NotSupportedException("oops!");

        var ex = Assert.Throws<InvalidOperationException>(() => _sut.CreateHandler(_service, _context.Object));

        ex.ShouldNotBeNull();
        ex.InnerException.ShouldBeOfType<NotSupportedException>();
        ex.InnerException.Message.ShouldContain("oops!");
    }
}