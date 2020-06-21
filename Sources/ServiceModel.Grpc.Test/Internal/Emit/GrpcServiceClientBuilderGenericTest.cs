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
    public class GrpcServiceClientBuilderGenericTest
    {
        private Func<IGenericContract<int, string>> _factory = null!;
        private IGenericContract<int, string> _contract = null!;
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

            var factory = builder.Build<IGenericContract<int, string>>(nameof(GrpcServiceClientBuilderGenericTest));

            _factory = () => factory(_callInvoker.Object);
        }

        [SetUp]
        public void BeforeEachTest()
        {
            _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            _contract = _factory();
        }

        [Test]
        public void Invoke()
        {
            Console.WriteLine(_contract.GetType().InstanceMethod(nameof(IGenericContract<int, string>.Invoke)).Disassemble());

            _callInvoker.SetupBlockingUnaryCallInOut(3, "4", "34");

            _contract.Invoke(3, "4").ShouldBe("34");

            _callInvoker.VerifyAll();
        }
    }
}
