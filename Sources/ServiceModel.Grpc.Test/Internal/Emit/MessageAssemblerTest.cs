﻿using System;
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

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class MessageAssemblerTest
    {
        private Mock<MethodInfo> _method;

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
        [TestCase(typeof(Message))]
        [TestCase(typeof(Message<int>), typeof(int))]
        [TestCase(typeof(Message<string, int?>), typeof(string), typeof(int?))]
        [TestCase(typeof(Message<int>), typeof(IAsyncEnumerable<int>))]
        public void TestRequestType(Type expected, params Type[] dataParameters)
        {
            MethodSetupParameters(dataParameters);

            var sut = new MessageAssembler(_method.Object);
            
            sut.RequestType.ShouldBe(expected);
            sut.RequestTypeInput.ShouldBe(Enumerable.Range(0, dataParameters.Length));
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