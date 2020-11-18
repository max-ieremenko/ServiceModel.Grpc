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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Internal.CSharp
{
    [TestFixture]
    public partial class CSharpServiceEndpointBinderBuilderTest
    {
        private static readonly Compilation Compilation = CSharpCompilation
            .Create(
                nameof(CSharpServiceEndpointBinderBuilderTest),
                references: AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(i => !i.IsDynamic && !string.IsNullOrEmpty(i.Location))
                    .Select(i => MetadataReference.CreateFromFile(i.Location)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        [Test]
        [TestCaseSource(nameof(GetWriteNewAttributeCases))]
        public void WriteNewAttribute(AttributeData attribute, string expected)
        {
            var output = new CodeStringBuilder();
            CSharpServiceEndpointBinderBuilder.WriteNewAttribute(output, attribute);

            output.ToString().ShouldBe(expected);
        }

        private static IEnumerable<TestCaseData> GetWriteNewAttributeCases()
        {
            var type = typeof(WriteNewAttributeCases);
            var symbol = Compilation.GetTypeByMetadataName(type);

            foreach (var method in SyntaxTools.GetInstanceMethods(symbol))
            {
                var attributes = CSharpServiceEndpointBinderBuilder.FilterAttributes(method.GetAttributes()).ToList();
                attributes.Count.ShouldBe(1);

                var expected = (string)type
                    .GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    ?.Invoke(new WriteNewAttributeCases(), Array.Empty<object>())!;

                yield return new TestCaseData(attributes[0], expected)
                {
                    TestName = method.Name
                };
            }
        }
    }
}
