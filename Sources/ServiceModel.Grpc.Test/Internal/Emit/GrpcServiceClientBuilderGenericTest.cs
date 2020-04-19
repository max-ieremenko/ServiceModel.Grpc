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
        private Func<IGenericContract<int, string>> _factory;
        private IGenericContract<int, string> _contract;
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
