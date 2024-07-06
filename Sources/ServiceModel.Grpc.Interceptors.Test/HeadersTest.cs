// <copyright>
// Copyright Max Ieremenko
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
using Grpc.Core.Interceptors;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Interceptors;

[TestFixture]
public class HeadersTest
{
    [Test]
    [TestCaseSource(nameof(GetShortAssemblyQualifiedNameTestCases))]
    public void GetShortAssemblyQualifiedName(Type type)
    {
        var name = type.GetShortAssemblyQualifiedName();

        var actual = Type.GetType(name, true, false);
        actual.ShouldBe(type);
    }

    private static IEnumerable<TestCaseData> GetShortAssemblyQualifiedNameTestCases()
    {
        var cases = new[]
        {
            typeof(int),
            typeof(int[]),
            typeof(string),
            typeof(string[]),
            typeof(byte[]),
            typeof(Guid),
            typeof(HeadersTest),
            typeof(HeadersTest[]),
            typeof(Tuple<int>),
            typeof(Tuple<int>[]),
            typeof(Tuple<int[]>),
            typeof(Tuple<Tuple<int, long>, string>),
            typeof(Tuple<Tuple<int, long>, string>[]),
            typeof(ValueTuple<int>),
            typeof(ValueTuple<Tuple<int, long>, string>),
            typeof(Interceptor.AsyncClientStreamingCallContinuation<Tuple<object>, string>),
            typeof(Interceptor.AsyncClientStreamingCallContinuation<Tuple<object>, string>[])
        };

        return cases.Select(i => new TestCaseData(i));
    }
}