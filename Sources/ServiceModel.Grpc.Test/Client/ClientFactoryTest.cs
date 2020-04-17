﻿using System;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Internal;
using Shouldly;

namespace ServiceModel.Grpc.Client
{
    [TestFixture]
    public class ClientFactoryTest
    {
        private Mock<IServiceClientBuilder> _clientBuilder;
        private ServiceModelGrpcClientOptions _defaultOptions;
        private ClientFactory _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _clientBuilder = new Mock<IServiceClientBuilder>(MockBehavior.Strict);
            _clientBuilder.SetupProperty(b => b.MarshallerFactory, null);
            _clientBuilder.SetupProperty(b => b.Logger, null);
            _clientBuilder.SetupProperty(b => b.DefaultCallOptionsFactory, null);

            _defaultOptions = new ServiceModelGrpcClientOptions { ClientBuilder = () => _clientBuilder.Object };

            _sut = new ClientFactory(_defaultOptions);
        }

        [Test]
        public void AddClient()
        {
            var callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            var instance = new Mock<IDisposable>(MockBehavior.Strict);

            Func<CallInvoker, IDisposable> factory = i =>
            {
                i.ShouldBe(callInvoker.Object);
                return instance.Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IDisposable>();

            _sut.CreateClient<IDisposable>(callInvoker.Object).ShouldBe(instance.Object);
        }

        [Test]
        public void RegisterClientGenericInterface()
        {
            Func<CallInvoker, IComparable<int>> factory = _ => null;

            _clientBuilder
                .Setup(b => b.Build<IComparable<int>>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.AddClient<IComparable<int>>();
        }

        [Test]
        public void CreateClientWithoutRegistration()
        {
            var callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            var instance = new Mock<IDisposable>(MockBehavior.Strict);

            Func<CallInvoker, IDisposable> factory = i =>
            {
                i.ShouldBe(callInvoker.Object);
                return instance.Object;
            };

            _clientBuilder
                .Setup(b => b.Build<IDisposable>(It.IsNotNull<string>()))
                .Returns(factory);

            _sut.CreateClient<IDisposable>(callInvoker.Object).ShouldBe(instance.Object);
        }

        [Test]
        public void DoubleClientRegistration()
        {
            Func<CallInvoker, IDisposable> factory = _ => null;

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
            var addClient = (Action<Action<ServiceModelGrpcClientOptions>>)_sut
                .GetType()
                .InstanceMethod(nameof(_sut.AddClient))
                .MakeGenericMethod(contractType)
                .CreateDelegate(typeof(Action<Action<ServiceModelGrpcClientOptions>>), _sut);

            var ex = Assert.Throws<NotSupportedException>(() => addClient(null));

            Console.WriteLine(ex);
        }
    }
}