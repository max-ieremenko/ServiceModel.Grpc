// <copyright>
// Copyright 2020-2024 Max Ieremenko
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
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

[TestFixture]
public partial class ServiceContractTest
{
    private readonly CSharpCompilation _compilation = CSharpCompilationExtensions.CreateDefault();

    [Test]
    [TestCaseSource(nameof(GetGetServiceNameCases))]
    public void GetServiceName(Type type, string expected)
    {
        var symbol = _compilation.ResolveTypeSymbol(type);

        ServiceContract.GetServiceName(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(I1), nameof(I1.Operation), "Operation")]
    [TestCase(typeof(I2), nameof(I2.Operation), "Method")]
    public void GetServiceOperationName(Type type, string methodName, string expected)
    {
        var symbol = _compilation.ResolveTypeSymbol(type);
        symbol.ShouldNotBeNull();

        var method = SyntaxTools.GetInstanceMethods(symbol).First(i => i.Name == methodName);

        ServiceContract.GetServiceOperationName(method).ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetGetServiceNameCases()
    {
        var cases = new[]
        {
            (typeof(I1), "I1"),
            (typeof(I2), "Service2"),
            (typeof(I3), "Test.Service2"),
            (typeof(IGeneric1<double>), "IGeneric1-Double"),
            (typeof(IGeneric2<double, int>), "Service2-Double-Int32"),
            (typeof(IGeneric1<IGeneric2<double, int>>), "IGeneric1-IGeneric2-Double-Int32"),
            (typeof(IGeneric1<SomeData>), "IGeneric1-Some-Data"),
            (typeof(IGeneric1<int?>), "IGeneric1-Nullable-Int32"),
            (typeof(IGeneric1<int?[][]>), "IGeneric1-ArrayArrayNullable-Int32"),
            (typeof(IGeneric1<string?>), "IGeneric1-String"),
            (typeof(IGeneric1<string[]>), "IGeneric1-ArrayString"),
            (typeof(IGeneric1<string[][]>), "IGeneric1-ArrayArrayString"),
            (typeof(IGeneric1<string[,]>), "IGeneric1-Array2String"),
            (typeof(IGeneric1<IList<string>?>), "IGeneric1-IList-String"),
            (typeof(IGeneric1<IList<int?>>), "IGeneric1-IList-Nullable-Int32")
        };

        foreach (var item in cases)
        {
            yield return new TestCaseData(item.Item1, item.Item2) { TestName = "GetServiceName." + item.Item2 };
        }
    }
}