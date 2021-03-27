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
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public abstract class ClientBuilderNotSupportedTestBase
    {
        private IInvalidContract _contract = null!;

        protected Func<IInvalidContract> Factory { get; set; } = null!;

        protected Mock<CallInvoker> CallInvoker { get; private set; } = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            CallInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            _contract = Factory();
        }

        [Test]
        public void InvalidSignature()
        {
            var x = 0;
            var ex = Assert.Throws<NotSupportedException>(() => _contract.InvalidSignature(ref x, out _));

            ex.ShouldNotBeNull();
            Console.WriteLine(ex.Message);

            ex.Message.ShouldContain(nameof(IInvalidContract.InvalidSignature));
        }

        [Test]
        public void GenericMethod()
        {
            var ex = Assert.Throws<NotSupportedException>(() => _contract.Generic<int, string>(2));

            ex.ShouldNotBeNull();
            Console.WriteLine(ex.Message);

            ex.Message.ShouldContain(nameof(IInvalidContract.Generic));
        }

        [Test]
        public void DisposableIsNotServiceContract()
        {
            var ex = Assert.Throws<NotSupportedException>(() => _contract.Dispose());

            ex.ShouldNotBeNull();
            Console.WriteLine(ex.Message);

            ex.Message.ShouldContain(typeof(IDisposable).FullName!);
        }
    }
}
