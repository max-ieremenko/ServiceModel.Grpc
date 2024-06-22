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
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ClientMethodFilterRegistrationTest
{
    private Mock<IServiceProvider> _serviceProvider = null!;
    private ClientMethodFilterRegistration _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _sut = new ClientMethodFilterRegistration();
    }

    [Test]
    public void CreateFactory()
    {
        var filter1 = new Mock<IClientFilter>(MockBehavior.Strict);
        var filter2 = new Mock<IClientFilter>(MockBehavior.Strict);

        _sut.Registrations.Add(new FilterRegistration<IClientFilter>(2, _ => filter2.Object));
        _sut.Registrations.Add(new FilterRegistration<IClientFilter>(1, _ => filter1.Object));

        var grpcMethod = new Mock<IMethod>(MockBehavior.Strict);
        grpcMethod
            .SetupGet(m => m.FullName)
            .Returns("the-method");

        var description = new Mock<IOperationDescription>(MockBehavior.Strict);

        _sut.AddMethod(grpcMethod.Object, description.Object);

        var actual = _sut.CreateFactory(_serviceProvider.Object).ShouldBeOfType<ClientCallFilterHandlerFactory>();

        actual.ServiceProvider.ShouldBe(_serviceProvider.Object);
        actual.MethodMetadataByGrpc.Count.ShouldBe(1);

        var metadata = actual.MethodMetadataByGrpc[grpcMethod.Object];

        metadata.Operation.FilterFactories.Length.ShouldBe(2);
        metadata.Operation.FilterFactories[0](null!).ShouldBe(filter1.Object);
        metadata.Operation.FilterFactories[1](null!).ShouldBe(filter2.Object);

        metadata.Operation.Operation.ShouldBe(description.Object);
        metadata.AlternateOperation.ShouldBeNull();
    }

    [Test]
    public void CreateFactorySynOverAsync()
    {
        _sut.Registrations.Add(new FilterRegistration<IClientFilter>(1, _ => new Mock<IClientFilter>(MockBehavior.Strict).Object));

        var grpcMethod = new Mock<IMethod>(MockBehavior.Strict);
        grpcMethod
            .Setup(m => m.Type)
            .Returns(MethodType.Unary);
        grpcMethod
            .SetupGet(m => m.FullName)
            .Returns("the-method-async");

        var asyncDescription = new Mock<IOperationDescription>(MockBehavior.Strict);
        asyncDescription
            .Setup(d => d.IsAsync())
            .Returns(true);

        var syncDescription = new Mock<IOperationDescription>(MockBehavior.Strict);
        syncDescription
            .Setup(d => d.IsAsync())
            .Returns(false);

        _sut.AddMethod(grpcMethod.Object, syncDescription.Object);
        _sut.AddMethod(grpcMethod.Object, asyncDescription.Object);

        var actual = _sut.CreateFactory(_serviceProvider.Object).ShouldBeOfType<ClientCallFilterHandlerFactory>();

        actual.MethodMetadataByGrpc.Count.ShouldBe(1);

        var metadata = actual.MethodMetadataByGrpc[grpcMethod.Object];
        metadata.Operation.Operation.ShouldBe(asyncDescription.Object);

        metadata.AlternateOperation.ShouldNotBeNull();
        metadata.AlternateOperation.Operation.ShouldBe(syncDescription.Object);
    }
}