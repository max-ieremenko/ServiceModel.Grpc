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
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ServiceMethodFilterRegistrationTest
{
    private Mock<IServiceProvider> _serviceProvider = null!;
    private Mock<IOperationDescription> _operation = null!;
    private ServiceMethodFilterRegistration _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

        _operation = new Mock<IOperationDescription>(MockBehavior.Strict);
        _operation
            .Setup(o => o.GetRequestAccessor())
            .Returns((IMessageAccessor?)null!);
        _operation
            .Setup(o => o.GetRequestStreamAccessor())
            .Returns((IStreamAccessor?)null!);
        _operation
            .Setup(o => o.GetResponseAccessor())
            .Returns((IMessageAccessor?)null!);
        _operation
            .Setup(o => o.GetResponseStreamAccessor())
            .Returns((IStreamAccessor?)null!);

        _sut = new ServiceMethodFilterRegistration(_serviceProvider.Object);
    }

    [Test]
    public void NoFilters()
    {
        var metadata = new[]
        {
            new object()
        };

        for (var i = 0; i < 2; i++)
        {
            var actual = _sut.CreateHandlerFactory(metadata, _operation.Object);

            actual.ShouldBeNull();
        }
    }

    [Test]
    public void GlobalFilters()
    {
        var filter1 = new Mock<IServerFilter>(MockBehavior.Strict);
        var filter2 = new Mock<IServerFilter>(MockBehavior.Strict);
        var filter3 = new Mock<IServerFilter>(MockBehavior.Strict);

        _sut.Add(
        [
            new FilterRegistration<IServerFilter>(2, _ => filter2.Object),
            new FilterRegistration<IServerFilter>(3, _ => filter3.Object),
            new FilterRegistration<IServerFilter>(1, _ => filter1.Object)
        ]);

        for (var i = 0; i < 2; i++)
        {
            var actual = _sut.CreateHandlerFactory(Array.Empty<object>(), _operation.Object);

            actual.ShouldNotBeNull();
            actual.ServiceProvider.ShouldBe(_serviceProvider.Object);
            actual.Operation.ShouldBe(_operation.Object);

            actual.FilterFactories.Length.ShouldBe(3);
            actual.FilterFactories[0](_serviceProvider.Object).ShouldBe(filter1.Object);
            actual.FilterFactories[1](_serviceProvider.Object).ShouldBe(filter2.Object);
            actual.FilterFactories[2](_serviceProvider.Object).ShouldBe(filter3.Object);
        }
    }

    [Test]
    public void AttributeFilters()
    {
        var filter1 = new TestServerFilterAttribute(1);
        var filter2 = new Mock<IServerFilter>(MockBehavior.Strict);
        var filter3 = new TestServerFilterAttribute(3);

        var metadata = new[]
        {
            new object(),
            new TestServerFilterRegistrationAttribute(2, filter2.Object),
            new object(),
            filter1,
            filter3
        };

        for (var i = 0; i < 2; i++)
        {
            var actual = _sut.CreateHandlerFactory(metadata, _operation.Object);

            actual.ShouldNotBeNull();
            actual.ServiceProvider.ShouldBe(_serviceProvider.Object);
            actual.Operation.ShouldBe(_operation.Object);

            actual.FilterFactories.Length.ShouldBe(3);
            actual.FilterFactories[0](_serviceProvider.Object).ShouldBe(filter1);
            actual.FilterFactories[1](_serviceProvider.Object).ShouldBe(filter2.Object);
            actual.FilterFactories[2](_serviceProvider.Object).ShouldBe(filter3);
        }
    }

    [Test]
    public void GlobalAndAttributeFilters()
    {
        var filter1 = new Mock<IServerFilter>(MockBehavior.Strict);
        var filter2 = new Mock<IServerFilter>(MockBehavior.Strict);
        var filter3 = new TestServerFilterAttribute(3);

        _sut.Add(new[]
        {
            new FilterRegistration<IServerFilter>(2, _ => filter2.Object)
        });

        var metadata = new object[]
        {
            filter3,
            new TestServerFilterRegistrationAttribute(1, filter1.Object)
        };

        for (var i = 0; i < 2; i++)
        {
            var actual = _sut.CreateHandlerFactory(metadata, _operation.Object);

            actual.ShouldNotBeNull();
            actual.ServiceProvider.ShouldBe(_serviceProvider.Object);
            actual.Operation.ShouldBe(_operation.Object);

            actual.FilterFactories.Length.ShouldBe(3);
            actual.FilterFactories[0](_serviceProvider.Object).ShouldBe(filter1.Object);
            actual.FilterFactories[1](_serviceProvider.Object).ShouldBe(filter2.Object);
            actual.FilterFactories[2](_serviceProvider.Object).ShouldBe(filter3);
        }
    }

    private sealed class TestServerFilterRegistrationAttribute : ServerFilterRegistrationAttribute
    {
        private readonly IServerFilter _filter;

        public TestServerFilterRegistrationAttribute(int order, IServerFilter filter)
            : base(order)
        {
            _filter = filter;
        }

        public override IServerFilter CreateFilter(IServiceProvider serviceProvider) => _filter;
    }

    private sealed class TestServerFilterAttribute : ServerFilterAttribute
    {
        public TestServerFilterAttribute(int order)
            : base(order)
        {
        }

        public override ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
        {
            throw new NotImplementedException();
        }
    }
}