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
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Filters.Internal;
using Shouldly;

namespace ServiceModel.Grpc.Client.Internal;

[TestFixture]
public class ClientMethodBinderTest
{
    private ClientMethodBinder _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ClientMethodBinder(
            new Mock<IServiceProvider>(MockBehavior.Strict).Object,
            new Mock<IMarshallerFactory>(MockBehavior.Strict).Object,
            null);
    }

    [Test]
    public void CreateFilterHandlerFactoryNoFilters()
    {
        var grpcMethod = new Mock<IMethod>(MockBehavior.Strict);

        _sut.Add(grpcMethod.Object, () => throw new NotSupportedException());

        _sut.RequiresMetadata.ShouldBeFalse();
        _sut.CreateFilterHandlerFactory().ShouldBeNull();
    }

    [Test]
    public void CreateFilterHandlerFactoryNoMethods()
    {
        var filter = new FilterRegistration<IClientFilter>(1, _ => throw new NotSupportedException());

        _sut.AddFilters(new[] { filter });

        _sut.RequiresMetadata.ShouldBeTrue();
        _sut.CreateFilterHandlerFactory().ShouldBeNull();
    }

    [Test]
    public void CreateFilterHandlerFactory()
    {
        var filter1 = new Mock<IClientFilter>(MockBehavior.Strict);
        var filter2 = new Mock<IClientFilter>(MockBehavior.Strict);

        _sut.AddFilters(new[]
        {
            new FilterRegistration<IClientFilter>(2, _ => filter2.Object),
            new FilterRegistration<IClientFilter>(1, _ => filter1.Object)
        });

        var grpcMethod = new Mock<IMethod>(MockBehavior.Strict);
        grpcMethod
            .SetupGet(m => m.FullName)
            .Returns("the-method");

        var method = new Mock<MethodInfo>(MockBehavior.Strict);

        _sut.Add(grpcMethod.Object, () => method.Object);

        var actual = _sut.CreateFilterHandlerFactory().ShouldBeOfType<ClientCallFilterHandlerFactory>();

        actual.ServiceProvider.ShouldBe(_sut.ServiceProvider);
        actual.MethodMetadataByGrpc.Count.ShouldBe(1);

        var metadata = actual.MethodMetadataByGrpc[grpcMethod.Object];

        metadata.Method.FilterFactories.Length.ShouldBe(2);
        metadata.Method.FilterFactories[0](null!).ShouldBe(filter1.Object);
        metadata.Method.FilterFactories[1](null!).ShouldBe(filter2.Object);
        ReferenceEquals(metadata.Method.ContractMethodDefinition(), method.Object).ShouldBeTrue();

        metadata.AlternateMethod.ShouldBeNull();
    }

    [Test]
    public void CreateFilterHandlerFactorySynOverAsync()
    {
        _sut.AddFilters(new[]
        {
            new FilterRegistration<IClientFilter>(1, _ => new Mock<IClientFilter>(MockBehavior.Strict).Object)
        });

        var grpcMethod = new Mock<IMethod>(MockBehavior.Strict);
        grpcMethod
            .Setup(m => m.Type)
            .Returns(MethodType.Unary);
        grpcMethod
            .SetupGet(m => m.FullName)
            .Returns("the-method-async");

        var asyncMethod = new Mock<MethodInfo>(MockBehavior.Strict);
        asyncMethod
            .SetupGet(m => m.ReturnType)
            .Returns(typeof(Task));

        var syncMethod = new Mock<MethodInfo>(MockBehavior.Strict);
        syncMethod
            .SetupGet(m => m.ReturnType)
            .Returns(typeof(void));

        _sut.Add(grpcMethod.Object, () => syncMethod.Object);
        _sut.Add(grpcMethod.Object, () => asyncMethod.Object);

        var actual = _sut.CreateFilterHandlerFactory().ShouldBeOfType<ClientCallFilterHandlerFactory>();

        actual.MethodMetadataByGrpc.Count.ShouldBe(1);

        var metadata = actual.MethodMetadataByGrpc[grpcMethod.Object];
        ReferenceEquals(metadata.Method.ContractMethodDefinition(), asyncMethod.Object).ShouldBeTrue();
        ReferenceEquals(metadata.AlternateMethod?.ContractMethodDefinition.Invoke(), syncMethod.Object).ShouldBeTrue();
    }
}