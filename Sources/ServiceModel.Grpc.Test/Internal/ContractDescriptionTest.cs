using System;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public partial class ContractDescriptionTest
    {
        [Test]
        [TestCase(typeof(IDuplicateOperationName))]
        [TestCase(typeof(IService))]
        public void DuplicateOperationName(Type serviceType)
        {
            var sut = new ContractDescription(serviceType);

            sut.Interfaces.Count.ShouldBe(0);
            sut.Services.Count.ShouldNotBe(0);

            foreach (var service in sut.Services)
            {
                service.Methods.Count.ShouldBe(0);
                service.Operations.Count.ShouldBe(0);
                service.NotSupportedOperations.Count.ShouldNotBe(0);

                Console.WriteLine(service.NotSupportedOperations[0].Error);
                service.NotSupportedOperations[0].Error.ShouldNotBeNullOrEmpty();
            }
        }
    }
}
