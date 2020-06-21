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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using Shouldly;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public class MessageAssemblerTest
    {
        private Mock<MethodInfo> _method = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _method = new Mock<MethodInfo>(MockBehavior.Strict);

            _method
                .SetupGet(m => m.DeclaringType)
                .Returns(GetType());
            _method
                .SetupGet(m => m.Name)
                .Returns("Test");
            _method
                .SetupGet(m => m.ReturnType)
                .Returns(typeof(void));
            _method
                .Setup(m => m.GetParameters())
                .Returns(Array.Empty<ParameterInfo>());
            _method
                .SetupGet(m => m.IsGenericMethod)
                .Returns(false);
        }

        [Test]
        [TestCase(typeof(void), typeof(Message))]
        [TestCase(typeof(Task), typeof(Message))]
        [TestCase(typeof(ValueTask), typeof(Message))]
        [TestCase(typeof(string), typeof(Message<string>))]
        [TestCase(typeof(int), typeof(Message<int>))]
        [TestCase(typeof(int?), typeof(Message<int?>))]
        [TestCase(typeof(Task<string>), typeof(Message<string>))]
        [TestCase(typeof(Task<int>), typeof(Message<int>))]
        [TestCase(typeof(ValueTask<int?>), typeof(Message<int?>))]
        [TestCase(typeof(IAsyncEnumerable<int>), typeof(Message<int>))]
        [TestCase(typeof(Task<IAsyncEnumerable<int>>), typeof(Message<int>))]
        public void TestResponseType(Type returnType, Type expected)
        {
            _method
                .SetupGet(m => m.ReturnType)
                .Returns(returnType);

            new MessageAssembler(_method.Object).ResponseType.ShouldBe(expected);
        }

        [Test]
        [TestCase(MethodType.Unary, typeof(void))]
        [TestCase(MethodType.Unary, typeof(int))]
        [TestCase(MethodType.Unary, typeof(Task<int>))]
        [TestCase(MethodType.Unary, typeof(Task<int>), typeof(string))]
        [TestCase(MethodType.ClientStreaming, typeof(void), typeof(IAsyncEnumerable<string>))]
        [TestCase(MethodType.ServerStreaming, typeof(IAsyncEnumerable<string>))]
        [TestCase(MethodType.ServerStreaming, typeof(Task<IAsyncEnumerable<string>>))]
        [TestCase(MethodType.DuplexStreaming, typeof(IAsyncEnumerable<string>), typeof(IAsyncEnumerable<int>))]
        public void TestOperationType(MethodType expected, Type returnType, params Type[] dataParameters)
        {
            _method
                .SetupGet(m => m.ReturnType)
                .Returns(returnType);
            MethodSetupParameters(dataParameters);

            new MessageAssembler(_method.Object).OperationType.ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(CallContext))]
        [TestCase(typeof(CallOptions))]
        [TestCase(typeof(ServerCallContext))]
        [TestCase(typeof(Task<CallOptions>))]
        [TestCase(typeof(Stream))]
        [TestCase(typeof(Task<Stream>))]
        public void NotSupportedResponseType(Type returnType)
        {
            _method
                .SetupGet(m => m.ReturnType)
                .Returns(returnType);

            Assert.Throws<NotSupportedException>(() => new MessageAssembler(_method.Object));
        }

        [Test]
        [TestCase(typeof(Message), null, new int[0])]
        [TestCase(typeof(Message<int>), null, new[] { 0 }, typeof(int))]
        [TestCase(typeof(Message<string, int?>), null, new[] { 0, 1 }, typeof(string), typeof(int?))]
        [TestCase(typeof(Message<int>), null, new[] { 0 }, typeof(IAsyncEnumerable<int>))]
        [TestCase(typeof(Message<int>), typeof(Message<int, string>), new[] { 0 }, typeof(IAsyncEnumerable<int>), typeof(int), typeof(string))]
        [TestCase(typeof(Message<int>), typeof(Message<string>), new[] { 1 }, typeof(string), typeof(IAsyncEnumerable<int>))]
        public void TestRequestType(Type expectedRequestType, Type expectedHeaderRequestType, int[] expectedRequestTypeInput, params Type[] dataParameters)
        {
            MethodSetupParameters(dataParameters);

            var sut = new MessageAssembler(_method.Object);

            sut.RequestType.ShouldBe(expectedRequestType);
            sut.RequestTypeInput.ShouldBe(expectedRequestTypeInput);

            sut.HeaderRequestType.ShouldBe(expectedHeaderRequestType);
            sut.HeaderRequestTypeInput.ShouldBe(dataParameters.Select((_, i) => i).Except(expectedRequestTypeInput).ToArray());
        }

        [Test]
        [TestCase(null)]
        [TestCase(typeof(CallOptions))]
        [TestCase(typeof(CallContext))]
        [TestCase(typeof(ServerCallContext))]
        [TestCase(typeof(CancellationToken))]
        public void TestContextInput(Type contextParameter)
        {
            MethodSetupParameters(contextParameter == null ? Array.Empty<Type>() : new[] { contextParameter });

            var actual = new MessageAssembler(_method.Object).ContextInput;

            actual.ShouldNotBeNull();

            if (contextParameter == null)
            {
                actual.ShouldBeEmpty();
            }
            else
            {
                actual.ShouldBe(new[] { 0 });
            }
        }

        [Test]
        [TestCase(typeof(Task))]
        [TestCase(typeof(Stream))]
        public void NotSupportedParameters(params Type[] parameters)
        {
            MethodSetupParameters(parameters);

            Assert.Throws<NotSupportedException>(() => new MessageAssembler(_method.Object));
        }

        [Test]
        public void GenericNotSupported()
        {
            MethodSetupParameters(Array.Empty<Type>());
            _method
                .SetupGet(m => m.IsGenericMethod)
                .Returns(true);

            Assert.Throws<NotSupportedException>(() => new MessageAssembler(_method.Object));
        }

        private void MethodSetupParameters(Type[] parameterTypes)
        {
            var parameters = new ParameterInfo[parameterTypes.Length];

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                var parameterType = parameterTypes[i];

                var parameter = new Mock<ParameterInfo>(MockBehavior.Strict);
                parameter
                    .SetupGet(p => p.Attributes)
                    .Returns(ParameterAttributes.None);
                parameter
                    .SetupGet(p => p.ParameterType)
                    .Returns(parameterType);

                parameters[i] = parameter.Object;
            }

            _method
                .Setup(m => m.GetParameters())
                .Returns(parameters);
        }
    }
}
