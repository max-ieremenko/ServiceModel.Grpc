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
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Client
{
    [TestFixture]
    public partial class ClientFactoryTest
    {
        private Mock<IClientBuilder<IDisposable>> _emitClientBuilder = null!;
        private ServiceModelGrpcClientOptions _defaultOptions = null!;
        private Mock<CallInvoker> _callInvoker = null!;
        private IClientErrorHandler _globalErrorHandler = null!;
        private IClientErrorHandler _localErrorHandler = null!;
        private Mock<IGenerator> _generator = null!;
        private ClientFactory _sut = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _emitClientBuilder = new Mock<IClientBuilder<IDisposable>>(MockBehavior.Strict);

            _defaultOptions = new ServiceModelGrpcClientOptions();

            _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);

            _globalErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;
            _localErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;

            _generator = new Mock<IGenerator>(MockBehavior.Strict);
            _generator.SetupProperty(g => g.Logger, null);
            _generator
                .Setup(g => g.GenerateClientBuilder<IDisposable>())
                .Returns(_emitClientBuilder.Object);

            _sut = new ClientFactory(_generator.Object, _defaultOptions);
        }

        [Test]
        public void AddClientDefaultOptions()
        {
            _emitClientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _sut.AddClient<IDisposable>();

            _emitClientBuilder.VerifyAll();
        }

        [Test]
        public void AddClientCustomOptions()
        {
            var marshaller = new Mock<IMarshallerFactory>(MockBehavior.Strict);
            Func<CallOptions> callOptions = () => throw new NotSupportedException();

            _emitClientBuilder
                .Setup(b => b.Initialize(marshaller.Object, callOptions));

            _sut.AddClient<IDisposable>(o =>
            {
                o.DefaultCallOptionsFactory = callOptions;
                o.MarshallerFactory = marshaller.Object;
            });

            _emitClientBuilder.VerifyAll();
        }

        [Test]
        public void AddClientWithBuilder()
        {
            var instance = new Mock<ISomeContract>(MockBehavior.Strict);

            var builder = new ManualClientBuilder
            {
                OnBuild = invoker =>
                {
                    invoker.ShouldBe(_callInvoker.Object);
                    return instance.Object;
                }
            };

            _sut.AddClient(builder);
            _sut.CreateClient<ISomeContract>(_callInvoker.Object).ShouldBe(instance.Object);

            builder.MarshallerFactory.ShouldBe(DataContractMarshallerFactory.Default);
            builder.DefaultCallOptionsFactory.ShouldBeNull();
        }

        [Test]
        public void AddClientGenericInterface()
        {
            var clientBuilder = new Mock<IClientBuilder<IComparable<int>>>(MockBehavior.Strict);
            clientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _generator
                .Setup(g => g.GenerateClientBuilder<IComparable<int>>())
                .Returns(clientBuilder.Object)
                .Verifiable();

            _sut.AddClient<IComparable<int>>();

            _generator.Verify();
            clientBuilder.VerifyAll();
        }

        [Test]
        public void CreateClientWithoutRegistration()
        {
            var instance = new Mock<IDisposable>(MockBehavior.Strict);

            _emitClientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _emitClientBuilder
                .Setup(b => b.Build(_callInvoker.Object))
                .Returns(instance.Object);

            _sut.CreateClient<IDisposable>(_callInvoker.Object).ShouldBe(instance.Object);
        }

        [Test]
        public void DoubleClientRegistration()
        {
            _emitClientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _sut.AddClient<IDisposable>();

            Assert.Throws<InvalidOperationException>(() => _sut.AddClient<IDisposable>());
        }

        [Test]
        [TestCase(typeof(object))] // class
        [TestCase(typeof(IGenerator))] // not public
        public void InvalidContracts(Type contractType)
        {
            var addClient = (Action<Action<ServiceModelGrpcClientOptions>?>)_sut
                .GetType()
                .InstanceMethod(nameof(_sut.AddClient), typeof(Action<ServiceModelGrpcClientOptions>))
                .MakeGenericMethod(contractType)
                .CreateDelegate(typeof(Action<Action<ServiceModelGrpcClientOptions>?>), _sut);

            var ex = Assert.Throws<NotSupportedException>(() => addClient(null));

            Console.WriteLine(ex);
        }

        [Test]
        public void GlobalErrorHandler()
        {
            _defaultOptions.ErrorHandler = _globalErrorHandler;

            _emitClientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _emitClientBuilder
                .Setup(b => b.Build(It.IsNotNull<CallInvoker>()))
                .Callback<CallInvoker>(i =>
                {
                    var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                    interceptor
                        .ShouldBeOfType<ClientNativeInterceptor>()
                        .CallInterceptor
                        .ShouldBeOfType<ClientCallErrorInterceptor>()
                        .ErrorHandler
                        .ShouldBe(_globalErrorHandler);

                    callInvoker.ShouldBe(_callInvoker.Object);
                })
                .Returns(new Mock<IDisposable>(MockBehavior.Strict).Object);

            _sut.CreateClient<IDisposable>(_callInvoker.Object);

            _emitClientBuilder.VerifyAll();
        }

        [Test]
        public void ServiceErrorHandler()
        {
            _emitClientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _emitClientBuilder
                .Setup(b => b.Build(It.IsNotNull<CallInvoker>()))
                .Callback<CallInvoker>(i =>
                {
                    var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                    interceptor
                        .ShouldBeOfType<ClientNativeInterceptor>()
                        .CallInterceptor
                        .ShouldBeOfType<ClientCallErrorInterceptor>()
                        .ErrorHandler
                        .ShouldBe(_localErrorHandler);

                    callInvoker.ShouldBe(_callInvoker.Object);
                })
                .Returns(new Mock<IDisposable>(MockBehavior.Strict).Object);

            _sut.AddClient<IDisposable>(options => options.ErrorHandler = _localErrorHandler);
            _sut.CreateClient<IDisposable>(_callInvoker.Object);

            _emitClientBuilder.VerifyAll();
        }

        [Test]
        public void ServiceErrorHandlerOverrideGlobal()
        {
            _defaultOptions.ErrorHandler = _globalErrorHandler;

            _emitClientBuilder
                .Setup(b => b.Initialize(DataContractMarshallerFactory.Default, null));

            _emitClientBuilder
                .Setup(b => b.Build(It.IsNotNull<CallInvoker>()))
                .Callback<CallInvoker>(i =>
                {
                    var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                    interceptor
                        .ShouldBeOfType<ClientNativeInterceptor>()
                        .CallInterceptor
                        .ShouldBeOfType<ClientCallErrorInterceptor>()
                        .ErrorHandler
                        .ShouldBe(_localErrorHandler);

                    callInvoker.ShouldBe(_callInvoker.Object);
                })
                .Returns(new Mock<IDisposable>(MockBehavior.Strict).Object);

            _sut.AddClient<IDisposable>(options => options.ErrorHandler = _localErrorHandler);
            _sut.CreateClient<IDisposable>(_callInvoker.Object);

            _emitClientBuilder.VerifyAll();
        }
    }
}
