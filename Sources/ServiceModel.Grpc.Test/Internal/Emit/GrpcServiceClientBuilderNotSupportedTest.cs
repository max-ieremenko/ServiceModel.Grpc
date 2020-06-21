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
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class GrpcServiceClientBuilderNotSupportedTest
    {
        private Func<IInvalidContract> _factory = null!;
        private IInvalidContract _contract = null!;
        private Mock<CallInvoker> _callInvoker = null!;
        private LoggerMock _logger = null!;

        [OneTimeSetUp]
        public void BeforeAllTests()
        {
            _logger = new LoggerMock();

            var builder = new GrpcServiceClientBuilder
            {
                MarshallerFactory = DataContractMarshallerFactory.Default,
                Logger = _logger.Logger
            };

            var factory = builder.Build<IInvalidContract>(nameof(GrpcServiceClientBuilderNotSupportedTest));

            _factory = () => factory(_callInvoker.Object);
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
            var log = _logger.Errors.Find(i => i.Contains(nameof(IInvalidContract.InvalidSignature)));
            log.ShouldNotBeNull();

            var x = 0;
            var ex = Assert.Throws<NotSupportedException>(() => _contract.InvalidSignature(ref x, out _));
            Console.WriteLine(ex.Message);

            ex.Message.ShouldBe(log);
        }

        [Test]
        public void GenericMethod()
        {
            var log = _logger.Errors.Find(i => i.Contains(nameof(IInvalidContract.Generic)));
            log.ShouldNotBeNull();

            var ex = Assert.Throws<NotSupportedException>(() => _contract.Generic<int, string>(2));
            Console.WriteLine(ex.Message);

            ex.Message.ShouldBe(log);
        }

        [Test]
        public void DisposableIsNotServiceContract()
        {
            var log = _logger.Debug.Find(i => i.Contains(typeof(IDisposable).FullName!));
            log.ShouldNotBeNull();

            var ex = Assert.Throws<NotSupportedException>(() => _contract.Dispose());
            Console.WriteLine(ex.Message);

            ex.Message.ShouldBe(log);
        }
    }
}
