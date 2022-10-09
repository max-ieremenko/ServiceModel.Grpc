// <copyright>
// Copyright 2021-2022 Max Ieremenko
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
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ServerCallFilterHandlerFactoryTest
{
    private Mock<IServiceProvider> _serviceProvider = null!;
    private MethodInfo _contractMethodDefinition = null!;
    private Func<IServiceProvider, IServerFilter>[] _filterFactories = null!;
    private Mock<IMultipurposeService> _service = null!;
    private Mock<ServerCallContext> _context = null!;
    private ServerCallFilterHandlerFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _contractMethodDefinition = typeof(IMultipurposeService).InstanceMethod(nameof(IMultipurposeService.GreetAsync));
        _filterFactories = new Func<IServiceProvider, IServerFilter>[1];
        _sut = new ServerCallFilterHandlerFactory(_serviceProvider.Object, _contractMethodDefinition, _filterFactories);

        _service = new Mock<IMultipurposeService>(MockBehavior.Strict);
        _context = new Mock<ServerCallContext>(MockBehavior.Strict);
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

        var actual = _sut.CreateHandler(_service.Object, _context.Object);

        actual.Context.ServerCallContext.ShouldBe(_context.Object);
        actual.Context.ContractMethodInfo.ShouldBe(_contractMethodDefinition);
        actual.Context.ServiceMethodInfo.ShouldNotBeNull();
        actual.Context.ServiceInstance.ShouldBe(_service.Object);
        actual.Context.ServiceProvider.ShouldBe(_serviceProvider.Object);

        actual.Context.Request.Count.ShouldBe(1);
        actual.Context.Request.Stream.ShouldBeNull(); // must be set by call handler

        actual.Context.Response.Count.ShouldBe(1);
        actual.Context.Response.Stream.ShouldNotBeNull();

        actual.Filters.Length.ShouldBe(1);
        actual.Filters[0].ShouldBe(filter.Object);
    }

    [Test]
    public void FilterFactoryReturnsNull()
    {
        _filterFactories[0] = provider => null!;

        var ex = Assert.Throws<InvalidOperationException>(() => _sut.CreateHandler(_service.Object, _context.Object));

        TestOutput.WriteLine(ex);
    }

    [Test]
    public void FilterFactoryThrows()
    {
        _filterFactories[0] = provider => throw new NotSupportedException("oops!");

        var ex = Assert.Throws<InvalidOperationException>(() => _sut.CreateHandler(_service.Object, _context.Object));

        TestOutput.WriteLine(ex);
        ex!.InnerException.ShouldBeOfType<NotSupportedException>();
        ex.InnerException.Message.ShouldContain("oops!");
    }
}