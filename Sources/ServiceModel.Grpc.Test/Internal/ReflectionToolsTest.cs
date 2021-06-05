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
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using NUnit.Framework;
using Shouldly;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public partial class ReflectionToolsTest
    {
        [Test]
        [TestCase(typeof(Task), true)]
        [TestCase(typeof(Task<int>), true)]
        [TestCase(typeof(ValueTask), true)]
        [TestCase(typeof(ValueTask<bool>), true)]
        [TestCase(typeof(IAsyncEnumerable<int>), false)]
        public void IsTask(Type type, bool expected)
        {
            ReflectionTools.IsTask(type).ShouldBe(expected);
        }

        [Test]
        [TestCaseSource(nameof(GetImplementationOfMethodCases))]
        public void ImplementationOfMethod(Type declaringType, MethodInfo method, string expected)
        {
            var actual = ReflectionTools.ImplementationOfMethod(typeof(Implementation), declaringType, method);

            actual.ShouldNotBeNull();

            actual.GetCustomAttribute<DescriptionAttribute>()!.Description.ShouldBe(expected);
        }

        [Test]
        [TestCaseSource(nameof(GetShortAssemblyQualifiedNameTestCases))]
        public void GetShortAssemblyQualifiedName(Type type)
        {
            var name = type.GetShortAssemblyQualifiedName();
            Console.WriteLine();
            Console.WriteLine(name);

            var actual = Type.GetType(name, true, false);
            actual.ShouldBe(type);
        }

        [Test]
        [TestCaseSource(nameof(GetUserFriendlyNameTestCases))]
        public void GetUserFriendlyName(Type type, string expected)
        {
            var name = type.GetUserFriendlyName();
            Console.WriteLine();
            Console.WriteLine(name);

            name.ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(ValueTuple<string>), true)]
        [TestCase(typeof((string, string)), true)]
        [TestCase(typeof((string, string, int)), true)]
        [TestCase(typeof(ValueTuple), false)]
        [TestCase(typeof(Tuple<string, string>), false)]
        [TestCase(typeof(object), false)]
        [TestCase(typeof((string, string)?), false)]
        public void IsValueTuple(Type type, bool expected)
        {
            ReflectionTools.IsValueTuple(type).ShouldBe(expected);
        }

        private static IEnumerable<TestCaseData> GetImplementationOfMethodCases()
        {
            var i1 = typeof(I1);
            yield return new TestCaseData(
                i1,
                i1.GetMethod(nameof(I1.Overload), Array.Empty<Type>()),
                "I1.Overload");

            yield return new TestCaseData(
                i1,
                i1.GetMethod(nameof(I1.Overload), new[] { typeof(int) }),
                "I1.Overload(int)");

            var i2 = typeof(I2);
            yield return new TestCaseData(
                i2,
                i2.GetMethod(nameof(I2.Overload), new[] { typeof(int) }),
                "I2.Overload(int)");
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
                typeof(ReflectionToolsTest),
                typeof(ReflectionToolsTest[]),
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

        private static IEnumerable<TestCaseData> GetUserFriendlyNameTestCases()
        {
            var cases = new Dictionary<Type, string>
            {
                { typeof(int), "Int32" },
                { typeof(int[]), "Int32[]" },
                { typeof(string), "String" },
                { typeof(string[]), "String[]" },
                { typeof(byte[]), "Byte[]" },
                { typeof(Guid), "Guid" },
                { typeof(Tuple<int>), "Tuple<Int32>" },
                { typeof(Tuple<int[]>), "Tuple<Int32[]>" },
                { typeof(Tuple<int>[]), "Tuple<Int32>[]" },
                { typeof(Tuple<Tuple<int, long>, string>), "Tuple<Tuple<Int32, Int64>, String>" },
                { typeof(Tuple<Tuple<int, long>, string>[]), "Tuple<Tuple<Int32, Int64>, String>[]" },
                { typeof(ValueTuple<int>), "ValueTuple<Int32>" },
                { typeof(ValueTuple<Tuple<int, long>, string>), "ValueTuple<Tuple<Int32, Int64>, String>" },
                { typeof(Interceptor.AsyncClientStreamingCallContinuation<Tuple<object>, string>), "Interceptor.AsyncClientStreamingCallContinuation<Tuple<Object>, String>" },
                { typeof(Interceptor.AsyncClientStreamingCallContinuation<Tuple<object>, string>[]), "Interceptor.AsyncClientStreamingCallContinuation<Tuple<Object>, String>[]" }
            };

            return cases.Select(i => new TestCaseData(i.Key, i.Value) { TestName = i.Value });
        }
    }
}
