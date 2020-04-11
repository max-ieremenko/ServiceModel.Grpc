using System;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
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
            _clientBuilder.SetupProperty(b => b.DefaultCallOptions, null);

            _defaultOptions = new ServiceModelGrpcClientOptions { ClientBuilder = () => _clientBuilder.Object };

            _sut = new ClientFactory(_defaultOptions);
        }

        [Test]
        public void CreateClientBuilder()
        {
            _defaultOptions.MarshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict).Object;
            var clientOptions = new ServiceModelGrpcClientOptions
            {
                DefaultCallOptions = new CallOptions(new Metadata()),
                Logger = new Mock<ILogger>(MockBehavior.Strict).Object
            };

            var actual = _sut.CreateClientBuilder(clientOptions);

            actual.ShouldBe(_clientBuilder.Object);
            actual.MarshallerFactory.ShouldBe(_defaultOptions.MarshallerFactory);
            actual.DefaultCallOptions.ShouldNotBeNull();
            actual.DefaultCallOptions.Value.Headers.ShouldBe(clientOptions.DefaultCallOptions.Value.Headers);
            actual.Logger.ShouldBe(clientOptions.Logger);
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
    }
}
