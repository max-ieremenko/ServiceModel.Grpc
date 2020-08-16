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
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    [TestFixture]
    public partial class SyntaxToolsTest
    {
        private static readonly Compilation Compilation = CSharpCompilation
            .Create(
                nameof(SyntaxToolsTest),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DisplayNameAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(SyntaxToolsTest).Assembly.Location)
                });

        [Test]
        [TestCase(typeof(I1), true)]
        [TestCase(typeof(SyntaxToolsTest), false)]
        public void IsInterface(Type type, bool expected)
        {
            var symbol = Compilation.GetTypeByMetadataName(type.FullName);
            symbol.ShouldNotBeNull();

            SyntaxTools.IsInterface(symbol).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(I1), typeof(ServiceContractAttribute), true)]
        [TestCase(typeof(SyntaxToolsTest), typeof(TestFixtureAttribute), true)]
        [TestCase(typeof(SyntaxToolsTest), typeof(ServiceContractAttribute), false)]
        public void GetCustomAttribute(Type type, Type attributeType, bool expected)
        {
            var symbol = Compilation.GetTypeByMetadataName(type.FullName);
            symbol.ShouldNotBeNull();

            var actual = SyntaxTools.GetCustomAttribute(symbol, attributeType.FullName);
            if (expected)
            {
                actual.ShouldNotBeNull();
            }
            else
            {
                actual.ShouldBeNull();
            }
        }

        [Test]
        public void ExpandInterface()
        {
            var i1Symbol = Compilation.GetTypeByMetadataName(typeof(I1).FullName);
            i1Symbol.ShouldNotBeNull();

            var actual = SyntaxTools.ExpandInterface(i1Symbol);

            actual.Count.ShouldBe(2);
            actual.ShouldContain(i1Symbol);

            var disposableSymbol = actual.First(i => !SymbolEqualityComparer.Default.Equals(i, i1Symbol));
            disposableSymbol.Name.ShouldBe(nameof(IDisposable));
        }

        [Test]
        public void GetInstanceMethods()
        {
            var i1Symbol = Compilation.GetTypeByMetadataName(typeof(I1).FullName);
            i1Symbol.ShouldNotBeNull();

            var methods = SyntaxTools.GetInstanceMethods(i1Symbol).ToList();

            methods.Count.ShouldBe(2);
        }

        [Test]
        [TestCase(typeof(I1), "ServiceModel.Grpc.DesignTime.Internal.SyntaxToolsTest")]
        [TestCase(typeof(SyntaxToolsTest), "ServiceModel.Grpc.DesignTime.Internal")]
        public void GetNamespace(Type type, string expected)
        {
            var symbol = Compilation.GetTypeByMetadataName(type.FullName);
            symbol.ShouldNotBeNull();

            SyntaxTools.GetNamespace(symbol).ShouldBe(expected);
        }

        [Test]
        [TestCaseSource(nameof(GetFullNameCases))]
        public void GetFullName(ITypeSymbol type, string expected)
        {
            SyntaxTools.GetFullName(type).ShouldBe(expected);
        }

        [Test]
        [TestCase(typeof(I1), typeof(object), true)]
        [TestCase(typeof(I1), typeof(IDisposable), true)]
        [TestCase(typeof(I1), typeof(I1), true)]
        [TestCase(typeof(I1), typeof(IAliasSymbol), false)]
        public void IsAssignableFrom(Type type, Type expected, bool result)
        {
            var symbol = Compilation.GetTypeByMetadataName(type.FullName);
            symbol.ShouldNotBeNull();

            SyntaxTools.IsAssignableFrom(symbol, expected).ShouldBe(result);
        }

        private static IEnumerable<TestCaseData> GetFullNameCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(FullNameCases).FullName);
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                var attribute = SyntaxTools.GetCustomAttribute(method, typeof(DisplayNameAttribute).FullName);
                var expected = (string)attribute!.ConstructorArguments[0].Value!;
                expected.ShouldNotBeNull();

                yield return new TestCaseData(method.ReturnType, expected);
            }
        }
    }
}
