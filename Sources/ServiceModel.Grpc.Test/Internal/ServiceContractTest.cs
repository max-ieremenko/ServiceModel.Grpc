using System;
using System.ServiceModel;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public partial class ServiceContractTest
    {
        [Test]
        [TestCase(typeof(object), false)]
        [TestCase(typeof(NativeGrpcService), true)]
        public void IsNativeGrpcService(Type serviceType, bool expected)
        {
            ServiceContract.IsNativeGrpcService(serviceType).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(IServiceProvider), false)]
        [TestCase(typeof(IServiceContract), true)]
        public void IsServiceContractInterface(Type serviceType, bool expected)
        {
            ServiceContract.IsServiceContractInterface(serviceType).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(IServiceContract), nameof(IServiceContract.Empty), true)]
        [TestCase(typeof(IServiceContract), nameof(IServiceContract.Ignore), false)]
        public void IsServiceOperation(Type serviceType, string methodName, bool expected)
        {
            var method = serviceType.InstanceMethod(methodName);
         
            ServiceContract.IsServiceOperation(method).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(IServiceContract), "IServiceContract")]
        public void GetServiceName(Type serviceType, string expected)
        {
            ServiceContract.GetServiceName(serviceType).ShouldBe(expected);
        }

        [Test]
        [TestCase("OverrideName", "OverrideNamespace", "OverrideNamespace.OverrideName")]
        [TestCase("OverrideName", null, "OverrideName")]
        [TestCase(null, "OverrideNamespace", "OverrideNamespace.IServiceContract")]
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

            ServiceContract.GetServiceName(typeof(IServiceContract), attribute).ShouldBe(expected);
        }

        [Test]
        [TestCase(nameof(IServiceContract.Empty), "Empty")]
        public void GetServiceOperationName(string methodName, string expected)
        {
            var method = typeof(IServiceContract).GetMethod(methodName);

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
