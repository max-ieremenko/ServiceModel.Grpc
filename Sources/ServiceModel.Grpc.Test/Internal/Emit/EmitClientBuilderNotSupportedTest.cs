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
using ServiceModel.Grpc.Configuration;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class EmitClientBuilderNotSupportedTest
    {
        private Func<IInvalidContract> _factory = null!;
        private IInvalidContract _contract = null!;
        private Mock<CallInvoker> _callInvoker = null!;

        [OneTimeSetUp]
        public void BeforeAllTests()
        {
            var description = new ContractDescription(typeof(IInvalidContract));

            var moduleBuilder = ProxyAssembly.CreateModule(nameof(EmitClientBuilderNotSupportedTest));

            var contractBuilder = new EmitContractBuilder(description);
            var contractType = contractBuilder.Build(moduleBuilder);
            var contractFactory = EmitContractBuilder.CreateFactory(contractType);

            var sut = new EmitClientBuilder(description, contractType);
            var clientType = sut.Build(moduleBuilder);
            var clientFactory = sut.CreateFactory<IInvalidContract>(clientType);

            _factory = () => clientFactory(_callInvoker.Object, contractFactory(DataContractMarshallerFactory.Default), null);
        }

        [SetUp]
        public void BeforeEachTest()
        {
            _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            _contract = _factory();
        }

        [Test]
        public void InvalidSignature()
        {
            var x = 0;
            var ex = Assert.Throws<NotSupportedException>(() => _contract.InvalidSignature(ref x, out _));
            Console.WriteLine(ex.Message);

            ex.Message.ShouldContain(nameof(IInvalidContract.InvalidSignature));
        }

        [Test]
        public void GenericMethod()
        {
            var ex = Assert.Throws<NotSupportedException>(() => _contract.Generic<int, string>(2));
            Console.WriteLine(ex.Message);

            ex.Message.ShouldContain(nameof(IInvalidContract.Generic));
        }

        [Test]
        public void DisposableIsNotServiceContract()
        {
            var ex = Assert.Throws<NotSupportedException>(() => _contract.Dispose());
            Console.WriteLine(ex.Message);

            ex.Message.ShouldContain(typeof(IDisposable).FullName);
        }
    }
}
