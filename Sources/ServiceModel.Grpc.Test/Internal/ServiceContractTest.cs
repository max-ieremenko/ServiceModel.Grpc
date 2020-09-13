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
using System.Collections.Generic;
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
        [TestCase(typeof(IGenericServiceContract<>), false)]
        [TestCase(typeof(IGenericServiceContract<int>), true)]
        public void IsServiceContractInterface(Type serviceType, bool expected)
        {
            ServiceContract.IsServiceContractInterface(serviceType).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(IServiceContract), nameof(IServiceContract.Empty), true)]
        [TestCase(typeof(IServiceContract), nameof(IServiceContract.Ignore), false)]
        [TestCase(typeof(IGenericServiceContract<int>), nameof(IGenericServiceContract<int>.Invoke), true)]
        public void IsServiceOperation(Type serviceType, string methodName, bool expected)
        {
            var method = serviceType.InstanceMethod(methodName);

            ServiceContract.IsServiceOperation(method).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(IServiceContract), "IServiceContract")]
        [TestCase(typeof(IGenericServiceContract<int>), "IGenericServiceContract-Int32")]
        [TestCase(typeof(IGenericServiceContract<SomeData>), "IGenericServiceContract-Some-Data")]
        [TestCase(typeof(IGenericServiceContract<int?>), "IGenericServiceContract-Nullable-Int32")]
        [TestCase(typeof(IGenericServiceContract<int?[][]>), "IGenericServiceContract-ArrayArrayNullable-Int32")]
        [TestCase(typeof(IGenericServiceContract<string?>), "IGenericServiceContract-String")]
        [TestCase(typeof(IGenericServiceContract<string[]>), "IGenericServiceContract-ArrayString")]
        [TestCase(typeof(IGenericServiceContract<string[,]>), "IGenericServiceContract-Array2String")]
        [TestCase(typeof(IGenericServiceContract<IList<string>?>), "IGenericServiceContract-IList-String")]
        [TestCase(typeof(IGenericServiceContract<IList<int?>>), "IGenericServiceContract-IList-Nullable-Int32")]
        public void GetServiceName(Type serviceType, string expected)
        {
            ServiceContract.GetServiceName(serviceType).ShouldBe(expected);
        }

        [Test]
        [TestCase("OverrideName", "OverrideNamespace", "OverrideNamespace.OverrideName")]
        [TestCase("OverrideName", null, "OverrideName")]
        [TestCase(null, "OverrideNamespace", "OverrideNamespace.IServiceContract")]
        public void GetServiceNameByAttribute(string? name, string? @namespace, string expected)
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

            var (typeName, attributeNamespace, attributeName) = ServiceContract.GetServiceNonGenericName(typeof(IServiceContract), attribute);

            typeName.ShouldBe(nameof(IServiceContract));
            attributeNamespace.ShouldBe(@namespace);
            attributeName.ShouldBe(name);
        }

        [Test]
        [TestCase(nameof(IServiceContract.Empty), "Empty")]
        public void GetServiceOperationName(string methodName, string expected)
        {
            var method = typeof(IServiceContract).GetMethod(methodName);

            ServiceContract.GetServiceOperationName(method!).ShouldBe(expected);
        }

        [Test]
        [TestCase("OverrideName", "OverrideName")]
        [TestCase(null, "Empty")]
        public void GetServiceOperationNameByAttribute(string? name, string expected)
        {
            var attribute = new OperationContractAttribute();
            if (name != null)
            {
                attribute.Name = name;
            }

            ServiceContract.GetServiceOperationName("Empty", attribute).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(IServiceContract), false)]
        [TestCase(typeof(IGenericServiceContract<int>), false)]
        [TestCase(typeof(NativeGrpcService), false)]
        [TestCase(typeof(object), true)]
        public void IsServiceInstanceType(Type type, bool expected)
        {
            ServiceContract.IsServiceInstanceType(type).ShouldBe(expected);
        }
    }
}
