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
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;
using Shouldly;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public partial class EmitClientBuilderBuilderTest
{
    private IClientBuilder<ISomeContract> _builder = null!;
    private Type _builderType = null!;
    private Mock<IClientMethodBinder> _methodBinder = null!;
    private Mock<CallInvoker> _callInvoker = null!;
    private Mock<IMarshallerFactory> _marshallerFactory = null!;
    private IClientCallInvoker _clientCallInvoker = null!;
    private EmitClientBuilderBuilder _sut = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        var description = ContractDescriptionBuilder.Build(typeof(ISomeContract));

        _sut = new EmitClientBuilderBuilder(description, typeof(ContractMock), typeof(ClientMock));
        _builderType = _sut.Build(ProxyAssembly.DefaultModule, nameof(EmitClientBuilderBuilderTest) + "ClientBuilder");
    }

    [SetUp]
    public void BeforeEachTest()
    {
        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
        _marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict);
        _builder = (IClientBuilder<ISomeContract>)Activator.CreateInstance(_builderType)!;
        _clientCallInvoker = new Mock<IClientCallInvoker>(MockBehavior.Strict).Object;

        _methodBinder = new Mock<IClientMethodBinder>(MockBehavior.Strict);
        _methodBinder
            .SetupGet(b => b.RequiresMetadata)
            .Returns(false);
        _methodBinder
            .SetupGet(b => b.MarshallerFactory)
            .Returns(_marshallerFactory.Object);
        _methodBinder
            .Setup(b => b.CreateCallInvoker())
            .Returns(_clientCallInvoker);
    }

    [Test]
    public void Build()
    {
        _builder.Initialize(_methodBinder.Object);

        var actual = _builder.Build(_callInvoker.Object);

        var mock = actual.ShouldBeOfType<ClientMock>();
        mock.CallInvoker.ShouldBe(_callInvoker.Object);
        mock.Contract.MarshallerFactory.ShouldBe(_marshallerFactory.Object);
        mock.ClientCallInvoker.ShouldBe(_clientCallInvoker);
    }

    [Test]
    public void Initialize()
    {
        var binderMethods = new List<(IMethod Method, Func<MethodInfo> ResolveContractMethodDefinition)>();

        _methodBinder
            .SetupGet(b => b.RequiresMetadata)
            .Returns(true);
        _methodBinder
            .SetupGet(b => b.MarshallerFactory)
            .Returns(DataContractMarshallerFactory.Default);
        _methodBinder
            .Setup(b => b.Add(It.IsNotNull<IMethod>(), It.IsNotNull<Func<MethodInfo>>()))
            .Callback<IMethod, Func<MethodInfo>>((method, resolver) => binderMethods.Add((method, resolver)));

        _builder.Initialize(_methodBinder.Object);

        var actual = _builder.Build(_callInvoker.Object);

        var mock = actual.ShouldBeOfType<ClientMock>();
        mock.CallInvoker.ShouldBe(_callInvoker.Object);
        mock.Contract.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
        mock.ClientCallInvoker.ShouldBe(_clientCallInvoker);

        binderMethods.Count.ShouldBe(1);
        binderMethods[0].Method.ShouldNotBeNull();
        binderMethods[0].ResolveContractMethodDefinition().ShouldBe(typeof(ISomeContract).InstanceMethod(nameof(ISomeContract.SomeOperation)));
    }
}