// <copyright>
// Copyright 2022 Max Ieremenko
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

[TestFixture]
public class CSharpMessageBuilderTest
{
    private AssemblyLoadContext _loadContext = null!;

    [OneTimeSetUp]
    public void BeforeAll()
    {
        _loadContext = new AssemblyLoadContext(nameof(CSharpMessageBuilderTest), isCollectible: true);
    }

    [OneTimeTearDown]
    public void AfterAll()
    {
        _loadContext.Unload();
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void TypeAttributes(int propertiesCount)
    {
        var messageType = FindOrCompileMessage(propertiesCount);

        messageType.IsPublic.ShouldBeTrue();
        messageType.IsClass.ShouldBeTrue();
        messageType.IsSealed.ShouldBeTrue();
        messageType.IsGenericTypeDefinition.ShouldBeTrue();
    }

    private static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new TestCaseData(4)
        {
            TestName = "4 args"
        };

        yield return new TestCaseData(5)
        {
            TestName = "5 args"
        };
    }

    private Type FindOrCompileMessage(int propertiesCount)
    {
        var ns = typeof(CSharpMessageBuilderTest).Namespace!;
        var assemblyName = typeof(CSharpMessageBuilderTest) + propertiesCount.ToString(CultureInfo.InvariantCulture);

        var assembly = _loadContext.Assemblies.FirstOrDefault(i => i.GetName().Name == assemblyName);
        if (assembly == null)
        {
            assembly = CompileMessage(assemblyName, ns, propertiesCount);
        }

        var messageTypeName = ns + ".Message" + "`" + propertiesCount.ToString(CultureInfo.InvariantCulture);

        return assembly.GetType(messageTypeName, true, false)!;
    }

    private Assembly CompileMessage(string assemblyName, string ns, int propertiesCount)
    {
        var output = new CodeStringBuilder();
        output.Append("namespace ").Append(ns).AppendLine(";");

        new CSharpMessageBuilder(propertiesCount).GenerateMemberDeclaration(output);

        var syntaxTree = SyntaxFactory.ParseSyntaxTree(
            output.AsStringBuilder().ToString(),
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(DataContractAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ExcludeFromCodeCoverageAttribute).Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var peStream = new MemoryStream();
        var emitResult = compilation.Emit(peStream);
        emitResult.Success.ShouldBeTrue();

        peStream.Position = 0;
        return _loadContext.LoadFromStream(peStream);
    }
}