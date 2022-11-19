// <copyright>
// Copyright 2020 Max Ieremenko
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
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit;

[TestFixture]
public partial class EmitClientBuilderBuilderTest
{
    private IClientBuilder<ISomeContract> _builder = null!;
    private Type _builderType = null!;
    private Mock<CallInvoker> _callInvoker = null!;
    private Mock<IMarshallerFactory> _marshallerFactory = null!;
    private Func<CallOptions> _callOptionsFactory = null!;
    private EmitClientBuilderBuilder _sut = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        var description = new ContractDescription(typeof(ISomeContract));

        var moduleBuilder = ProxyAssembly.CreateModule(nameof(EmitClientBuilderBuilderTest));

        _sut = new EmitClientBuilderBuilder(description, typeof(ContractMock), typeof(ClientMock));
        _builderType = _sut.Build(moduleBuilder);
    }

    [SetUp]
    public void BeforeEachTest()
    {
        _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
        _marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict);
        _callOptionsFactory = () => throw new NotSupportedException();
        _builder = (IClientBuilder<ISomeContract>)Activator.CreateInstance(_builderType)!;
    }

    [Test]
    public void Build()
    {
        _builder.Initialize(_marshallerFactory.Object, _callOptionsFactory);

        var actual = _builder.Build(_callInvoker.Object);

        var mock = actual.ShouldBeOfType<ClientMock>();
        mock.CallInvoker.ShouldBe(_callInvoker.Object);
        mock.Contract.MarshallerFactory.ShouldBe(_marshallerFactory.Object);
        mock.DefaultCallOptionsFactory.ShouldBe(_callOptionsFactory);
    }

    [Test]
    public void Initialize()
    {
        _builder.Initialize(_marshallerFactory.Object, _callOptionsFactory);
        _builder.Initialize(DataContractMarshallerFactory.Default, null);

        var actual = _builder.Build(_callInvoker.Object);

        var mock = actual.ShouldBeOfType<ClientMock>();
        mock.CallInvoker.ShouldBe(_callInvoker.Object);
        mock.Contract.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
        mock.DefaultCallOptionsFactory.ShouldBeNull();
    }
}