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
using System.Linq;
using System.ServiceModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    [TestFixture]
    public partial class ServiceContractTest
    {
        private Compilation _compilation = null!;

        [OneTimeSetUp]
        public void BeforeAllTest()
        {
            _compilation = CSharpCompilation
                .Create(
                    nameof(ServiceContractTest),
                    references: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                        MetadataReference.CreateFromFile(GetType().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(ServiceContractAttribute).Assembly.Location)
                    });
        }

        [Test]
        [TestCase(typeof(I1), "I1")]
        [TestCase(typeof(I2), "Service2")]
        [TestCase(typeof(I3), "Test.Service2")]
        [TestCase(typeof(IGeneric1<double>), "IGeneric1-Double")]
        [TestCase(typeof(IGeneric2<double, int>), "Service2-Double-Int32")]
        [TestCase(typeof(IGeneric1<IGeneric2<double, int>>), "IGeneric1-IGeneric2-Double-Int32")]
        [TestCase(typeof(IGeneric1<SomeData>), "IGeneric1-Some-Data")]
        [TestCase(typeof(IGeneric1<int?>), "IGeneric1-Nullable-Int32")]
        [TestCase(typeof(IGeneric1<int?[][]>), "IGeneric1-ArrayArrayNullable-Int32")]
        [TestCase(typeof(IGeneric1<string?>), "IGeneric1-String")]
        [TestCase(typeof(IGeneric1<string[]>), "IGeneric1-ArrayString")]
        [TestCase(typeof(IGeneric1<string[][]>), "IGeneric1-ArrayArrayString")]
        [TestCase(typeof(IGeneric1<string[,]>), "IGeneric1-Array2String")]
        [TestCase(typeof(IGeneric1<IList<string>?>), "IGeneric1-IList-String")]
        [TestCase(typeof(IGeneric1<IList<int?>>), "IGeneric1-IList-Nullable-Int32")]
        public void GetServiceName(Type type, string expected)
        {
            var symbol = _compilation.GetTypeByMetadataName(type);

            ServiceContract.GetServiceName(symbol).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(I1), nameof(I1.Operation), "Operation")]
        [TestCase(typeof(I2), nameof(I2.Operation), "Method")]
        public void GetServiceOperationName(Type type, string methodName, string expected)
        {
            var symbol = _compilation.GetTypeByMetadataName(type);
            symbol.ShouldNotBeNull();

            var method = SyntaxTools.GetInstanceMethods(symbol).First(i => i.Name == methodName);

            ServiceContract.GetServiceOperationName(method).ShouldBe(expected);
        }
    }
}
