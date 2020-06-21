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
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Client
{
    [TestFixture]
    public class ClientFactoryTest
    {
        private Mock<IServiceClientBuilder> _clientBuilder = null!;
        private ServiceModelGrpcClientOptions _defaultOptions = null!;
        private Mock<CallInvoker> _callInvoker = null!;
        private IClientErrorHandler _globalErrorHandler = null!;
        private IClientErrorHandler _localErrorHandler = null!;
        private ClientFactory _sut = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _clientBuilder = new Mock<IServiceClientBuilder>(MockBehavior.Strict);
            _clientBuilder.SetupProperty(b => b.MarshallerFactory, null!);
            _clientBuilder.SetupProperty(b => b.Logger, null);
            _clientBuilder.SetupProperty(b => b.DefaultCallOptionsFactory, null);

            _defaultOptions = new ServiceModelGrpcClientOptions { ClientBuilder = () => _clientBuilder.Object };

            _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);

            _globalErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;
            _localErrorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict).Object;

            _sut = new ClientFactory(_defaultOptions);
        }

        [Test]
        public void AddClient()
        {
            var instance = new Mock<IDisposable>(MockBehavior.Strict);

            Func<CallInvoker, IDisposable> factory = i =>
            {
                i.ShouldBe(_callInvoker.Object);
                return instance.Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IDisposable>();

            _sut.CreateClient<IDisposable>(_callInvoker.Object).ShouldBe(instance.Object);
        }

        [Test]
        public void RegisterClientGenericInterface()
        {
            Func<CallInvoker, IComparable<int>> factory = _ => throw new NotSupportedException();

            _clientBuilder
                .Setup(b => b.Build<IComparable<int>>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IComparable<int>>();
        }

        [Test]
        public void CreateClientWithoutRegistration()
        {
            var instance = new Mock<IDisposable>(MockBehavior.Strict);

            Func<CallInvoker, IDisposable> factory = i =>
            {
                i.ShouldBe(_callInvoker.Object);
                return instance.Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.CreateClient<IDisposable>(_callInvoker.Object).ShouldBe(instance.Object);
        }

        [Test]
        public void DoubleClientRegistration()
        {
            Func<CallInvoker, IDisposable> factory = _ => throw new NotSupportedException();

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IDisposable>();

            Assert.Throws<InvalidOperationException>(() => _sut.AddClient<IDisposable>());
        }

        [Test]
        [TestCase(typeof(object))] // class
        [TestCase(typeof(IServiceClientBuilder))] // not public
        public void InvalidContracts(Type contractType)
        {
            var addClient = (Action<Action<ServiceModelGrpcClientOptions>?>)_sut
                .GetType()
                .InstanceMethod(nameof(_sut.AddClient))
                .MakeGenericMethod(contractType)
                .CreateDelegate(typeof(Action<Action<ServiceModelGrpcClientOptions>?>), _sut);

            var ex = Assert.Throws<NotSupportedException>(() => addClient(null));

            Console.WriteLine(ex);
        }

        [Test]
        public void GlobalErrorHandler()
        {
            _defaultOptions.ErrorHandler = _globalErrorHandler;

            Func<CallInvoker, IDisposable> factory = i =>
            {
                var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                interceptor
                    .ShouldBeOfType<ClientNativeInterceptor>()
                    .CallInterceptor
                    .ShouldBeOfType<ClientCallErrorInterceptor>()
                    .ErrorHandler
                    .ShouldBe(_globalErrorHandler);

                callInvoker.ShouldBe(_callInvoker.Object);
                return new Mock<IDisposable>(MockBehavior.Strict).Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.CreateClient<IDisposable>(_callInvoker.Object);
        }

        [Test]
        public void ServiceErrorHandler()
        {
            Func<CallInvoker, IDisposable> factory = i =>
            {
                var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                interceptor
                    .ShouldBeOfType<ClientNativeInterceptor>()
                    .CallInterceptor
                    .ShouldBeOfType<ClientCallErrorInterceptor>()
                    .ErrorHandler
                    .ShouldBe(_localErrorHandler);

                callInvoker.ShouldBe(_callInvoker.Object);
                return new Mock<IDisposable>(MockBehavior.Strict).Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IDisposable>(options => options.ErrorHandler = _localErrorHandler);
            _sut.CreateClient<IDisposable>(_callInvoker.Object);
        }

        [Test]
        public void ServiceErrorHandlerOverrideGlobal()
        {
            _defaultOptions.ErrorHandler = _globalErrorHandler;

            Func<CallInvoker, IDisposable> factory = i =>
            {
                var (interceptor, callInvoker) = i.ShouldBeIntercepted();

                interceptor
                    .ShouldBeOfType<ClientNativeInterceptor>()
                    .CallInterceptor
                    .ShouldBeOfType<ClientCallErrorInterceptor>()
                    .ErrorHandler
                    .ShouldBe(_localErrorHandler);

                callInvoker.ShouldBe(_callInvoker.Object);
                return new Mock<IDisposable>(MockBehavior.Strict).Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IDisposable>(options => options.ErrorHandler = _localErrorHandler);
            _sut.CreateClient<IDisposable>(_callInvoker.Object);
        }
    }
}
