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
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class EmitServiceBuilderGenericTest
    {
        private Type _channelType = null!;
        private object _channel = null!;
        private Mock<IGenericContract<int, string>> _service = null!;

        [OneTimeSetUp]
        public void BeforeAllTest()
        {
            var description = new ContractDescription(typeof(IGenericContract<int, string>));
            var contractType = new EmitContractBuilder(description).Build(ProxyAssembly.DefaultModule, nameof(EmitServiceBuilderGenericTest) + "Contract");

            var sut = new EmitServiceBuilder(ProxyAssembly.DefaultModule, nameof(EmitServiceBuilderGenericTest) + "Service", contractType);
            foreach (var interfaceDescription in description.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    sut.BuildOperation(operation, interfaceDescription.InterfaceType);
                }
            }

            _channelType = sut.BuildType();
            var contract = EmitContractBuilder.CreateFactory(contractType)(DataContractMarshallerFactory.Default);
            _channel = EmitServiceBuilder.CreateFactory(_channelType, contractType)(contract);
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
                .InstanceMethod(nameof(IGenericContract<int, string>.Invoke))
                .CreateDelegate<UnaryServerMethod<IGenericContract<int, string>, Message<int, string>, Message<string>>>(_channel);
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
