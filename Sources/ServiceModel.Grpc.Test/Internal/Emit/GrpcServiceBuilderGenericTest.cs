﻿// <copyright>
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
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class GrpcServiceBuilderGenericTest
    {
        private Type _channelType;
        private Mock<IGenericContract<int, string>> _service;

        [OneTimeSetUp]
        public void BeforeAllTest()
        {
            var sut = new GrpcServiceBuilder(typeof(IGenericContract<int, string>), DataContractMarshallerFactory.Default, nameof(GrpcServiceBuilderGenericTest));

            foreach (var method in ReflectionTools.GetMethods(typeof(IGenericContract<int, string>)))
            {
                sut.BuildCall(new MessageAssembler(method), method.Name);
            }

            _channelType = sut.BuildType();
        }

        [SetUp]
        public void BeforeEachTest()
        {
            _service = new Mock<IGenericContract<int, string>>(MockBehavior.Strict);
        }

        [Test]
        public async Task Invoke()
        {
            var call = _channelType
                .StaticMethod(nameof(IGenericContract<int, string>.Invoke))
                .CreateDelegate<UnaryServerMethod<IGenericContract<int, string>, Message<int, string>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);

            _service
                .Setup(s => s.Invoke(3, "4"))
                .Returns("34");

            var actual = await call(_service.Object, new Message<int, string>(3, "4"), serverContext.Object);

            actual.Value1.ShouldBe("34");
            _service.VerifyAll();
        }
    }
}