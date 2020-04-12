using System;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Hosting
{
    [TestFixture]
    public class GrpcServiceFactoryBaseTest
    {
        private LoggerMock _logger;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            _logger = new LoggerMock();

            var service = new Mock<IInvalidContract>(MockBehavior.Strict);

            var sutType = typeof(GrpcServiceFactoryBase<>).MakeGenericType(service.Object.GetType());
            var sutMockType = typeof(Mock<>).MakeGenericType(sutType);

            var sutMock = (Mock)Activator.CreateInstance(sutMockType, _logger.Logger, DataContractMarshallerFactory.Default);

            sutType.InstanceMethod("Bind").Invoke(sutMock.Object, Array.Empty<object>());
        }

        [Test]
        public void InvalidSignature()
        {
            var log = _logger.Errors.Find(i => i.Contains(nameof(IInvalidContract.InvalidSignature)));
            log.ShouldNotBeNull();
            Console.WriteLine(log);
        }

        [Test]
        public void DisposeIsNotOperation()
        {
            var log = _logger.Debug.Find(i => i.Contains(nameof(IInvalidContract.Dispose)));
            log.ShouldNotBeNull();
        }
    }
}
