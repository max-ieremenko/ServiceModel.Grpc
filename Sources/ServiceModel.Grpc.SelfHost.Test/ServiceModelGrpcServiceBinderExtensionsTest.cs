// <copyright>
// Copyright 2022 Max Ieremenko
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
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.SelfHost;

[TestFixture]
public partial class ServiceModelGrpcServiceBinderExtensionsTest
{
    private DummyServiceBinder _binder = null!;
    private Mock<IServiceEndpointBinder<MultipurposeService>> _endpointBinder = null!;
    private Mock<IMarshallerFactory> _marshallerFactory = null!;
    private Mock<IServiceProvider> _serviceProvider = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _binder = new DummyServiceBinder();
        _endpointBinder = new Mock<IServiceEndpointBinder<MultipurposeService>>(MockBehavior.Strict);
        _marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
    }

    [Test]
    public void ReflectionBindFactory()
    {
        _binder.BindServiceModel(() => new MultipurposeService());

        _binder.Methods.ShouldNotBeEmpty();

        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.Concat));
        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.RepeatValue));
        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.SumValues));
        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.GreetAsync));
    }

    [Test]
    public void GeneratedCodeBindFactory()
    {
        _endpointBinder
            .Setup(b => b.Bind(It.IsNotNull<IServiceMethodBinder<MultipurposeService>>()))
            .Callback<IServiceMethodBinder<MultipurposeService>>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(_marshallerFactory.Object);
            });

        _binder.BindServiceModel(
            _endpointBinder.Object,
            () => new MultipurposeService(),
            options =>
            {
                options.MarshallerFactory = _marshallerFactory.Object;
            });

        _endpointBinder.VerifyAll();
        _binder.Methods.ShouldBeEmpty();
    }

    [Test]
    public void ReflectionBindServiceProvider()
    {
        var optionsAreHandled = false;

        _binder.BindServiceModel<MultipurposeService>(
            _serviceProvider.Object,
            options =>
            {
                options.ServiceProvider.ShouldBe(_serviceProvider.Object);
                optionsAreHandled = true;
            });

        optionsAreHandled.ShouldBeTrue();

        _binder.Methods.ShouldNotBeEmpty();

        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.Concat));
        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.RepeatValue));
        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.SumValues));
        _binder.Methods.ShouldContain(i => i.Name == nameof(IMultipurposeService.GreetAsync));
    }

    [Test]
    public void GeneratedCodeBindServiceProvider()
    {
        _endpointBinder
            .Setup(b => b.Bind(It.IsNotNull<IServiceMethodBinder<MultipurposeService>>()))
            .Callback<IServiceMethodBinder<MultipurposeService>>(binder =>
            {
                binder.MarshallerFactory.ShouldBe(_marshallerFactory.Object);
            });

        _binder.BindServiceModel(
            _endpointBinder.Object,
            _serviceProvider.Object,
            options =>
            {
                options.ServiceProvider.ShouldBe(_serviceProvider.Object);
                options.MarshallerFactory = _marshallerFactory.Object;
            });

        _endpointBinder.VerifyAll();
        _binder.Methods.ShouldBeEmpty();
    }
}