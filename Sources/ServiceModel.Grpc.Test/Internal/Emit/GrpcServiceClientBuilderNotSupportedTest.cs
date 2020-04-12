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
        private Func<IInvalidContract> _factory;
        private IInvalidContract _contract;
        private Mock<CallInvoker> _callInvoker;
        private LoggerMock _logger;

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
        public void DisposeIsNotOperation()
        {
            var log = _logger.Errors.Find(i => i.Contains(nameof(IInvalidContract.Dispose)));
            log.ShouldNotBeNull();

            var ex = Assert.Throws<NotSupportedException>(() => _contract.Dispose());
            Console.WriteLine(ex.Message);

            ex.Message.ShouldBe(log);
        }

        [Test]
        public void InvalidContextOptions()
        {
            var log = _logger.Errors.Find(i => i.Contains(nameof(IInvalidContract.InvalidContextOptions)));
            log.ShouldNotBeNull();

            var ex = Assert.Throws<NotSupportedException>(() => _contract.InvalidContextOptions());
            Console.WriteLine(ex.Message);

            ex.Message.ShouldBe(log);
        }
    }
}
