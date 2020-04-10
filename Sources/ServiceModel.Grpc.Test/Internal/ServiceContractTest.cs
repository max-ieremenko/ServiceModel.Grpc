using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public partial class ServiceContractTest
    {
        [Test]
        [TestCase(typeof(IServiceProvider), new Type[0])]
        [TestCase(typeof(ServiceContract1), new[] { typeof(IServiceContract1) })]
        [TestCase(typeof(ServiceContract2), new[] { typeof(IServiceContract1), typeof(IServiceContract2) })]
        [TestCase(typeof(Contract1), new[] { typeof(IServiceContract1) })]
        [TestCase(typeof(IServiceContract1), new[] { typeof(IServiceContract1) })]
        [TestCase(typeof(IServiceContract2), new[] { typeof(IServiceContract1), typeof(IServiceContract2) })]
        [TestCase(typeof(IContract1), new[] { typeof(IServiceContract1) })]
        public void GetServiceContractInterfaces(Type serviceType, Type[] expected)
        {
            var actual = ServiceContract.GetServiceContractInterfaces(serviceType);

            actual.ShouldBe(expected, ignoreOrder: true);
        }

        [Test]
        [TestCase(typeof(IServiceContract1), new[] { nameof(IServiceContract1.Empty) })]
        [TestCase(typeof(IServiceContract2), new string[0])]
        public void GetServiceOperations(Type serviceType, string[] expected)
        {
            var actual = ServiceContract.GetServiceOperations(serviceType);

            actual.Select(i => i.Name).ShouldBe(expected, ignoreOrder: true);
        }

        [Test]
        [TestCase(typeof(IServiceContract1), "ServiceModel.Grpc.Internal.ServiceContractTest.IServiceContract1")]
        [TestCase(typeof(IServiceContract2), "ServiceModel.Grpc.Internal.ServiceContractTest.IServiceContract2")]
        public void GetServiceName(Type serviceType, string expected)
        {
            ServiceContract.GetServiceName(serviceType).ShouldBe(expected);
        }

        [Test]
        [TestCase("OverrideName", "OverrideNamespace", "OverrideNamespace.OverrideName")]
        [TestCase("OverrideName", null, "ServiceModel.Grpc.Internal.ServiceContractTest.OverrideName")]
        [TestCase(null, "OverrideNamespace", "OverrideNamespace.IServiceContract1")]
        public void GetServiceNameByAttribute(string name, string @namespace, string expected)
        {
            var attribute = new ServiceContractAttribute();
            if (name != null)
            {
                attribute.Name = name;
            }

            if (@namespace != null)
            {
                attribute.Namespace = @namespace;
            }

            ServiceContract.GetServiceName(typeof(IServiceContract1), attribute).ShouldBe(expected);
        }

        [Test]
        [TestCase(nameof(IServiceContract1.Empty), "Empty")]
        public void GetServiceOperationName(string methodName, string expected)
        {
            var method = typeof(IServiceContract1).GetMethod(methodName);

            ServiceContract.GetServiceOperationName(method).ShouldBe(expected);
        }

        [Test]
        [TestCase("OverrideName", "OverrideName")]
        [TestCase(null, "Empty")]
        public void GetServiceOperationNameByAttribute(string name, string expected)
        {
            var attribute = new OperationContractAttribute();
            if (name != null)
            {
                attribute.Name = name;
            }

            ServiceContract.GetServiceOperationName("Empty", attribute).ShouldBe(expected);
        }
    }
}
