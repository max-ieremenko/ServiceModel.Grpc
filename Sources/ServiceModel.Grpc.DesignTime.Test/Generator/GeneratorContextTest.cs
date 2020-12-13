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

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    [TestFixture]
    public class GeneratorContextTest
    {
        private Mock<AnalyzerConfigOptions> _globalOptions = null!;
        private GeneratorExecutionContext _context;

        [SetUp]
        public void BeforeEachTest()
        {
            _globalOptions = new Mock<AnalyzerConfigOptions>(MockBehavior.Strict);

            var optionsProvider = new Mock<AnalyzerConfigOptionsProvider>(MockBehavior.Strict);
            optionsProvider
                .SetupGet(p => p.GlobalOptions)
                .Returns(_globalOptions.Object);

            var ctor = typeof(GeneratorExecutionContext)
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(i => i.GetParameters().Length == 6);

            _context = (GeneratorExecutionContext)ctor.Invoke(new object?[]
            {
                null, // Compilation
                null, // ParseOptions
                ImmutableArray<AdditionalText>.Empty,
                optionsProvider.Object,
                null, // ISyntaxReceiver
                CancellationToken.None
            });
        }

        [Test]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("false", false)]
        [TestCase("true", true)]
        public void LaunchDebugger(string? value, bool expected)
        {
            _globalOptions
                .Setup(o => o.TryGetValue("build_property.servicemodelgrpcdesigntime_launchdebugger", out value))
                .Returns(value != null);

            GeneratorContext.LaunchDebugger(_context).ShouldBe(expected);
        }

        [Test]
        [TestCase(null, ".smgrpcdtg.cs")]
        [TestCase(".11.cs", ".11.cs")]
        public void GetOutputFileName(string? optionsFileExtension, string expectedFileExtension)
        {
            _globalOptions
                .Setup(o => o.TryGetValue("build_property.servicemodelgrpcdesigntime_csextension", out optionsFileExtension))
                .Returns(optionsFileExtension != null);

            var node = SyntaxFactory.ClassDeclaration("ClassName");
            var actual = GeneratorContext
                .GetOutputFileName(_context, node, "HintName");

            actual.ShouldStartWith("ClassName.");
            actual.ShouldEndWith(expectedFileExtension);
        }

        [Test]
        public void AddOutput()
        {
            var csExtension = string.Empty;
            _globalOptions
                .Setup(o => o.TryGetValue("build_property.servicemodelgrpcdesigntime_csextension", out csExtension))
                .Returns(false);

            var designTime = "true";
            _globalOptions
                .Setup(o => o.TryGetValue("build_property.servicemodelgrpcdesigntime_designtime", out designTime))
                .Returns(true);

            var projectDir = "dummy";
            _globalOptions
                .Setup(o => o.TryGetValue("build_property.projectdir", out projectDir))
                .Returns(true);

            using var output = new TempDirectory();

            var intermediatePath = output.Location;
            _globalOptions
                .Setup(o => o.TryGetValue("build_property.intermediateoutputpath", out intermediatePath))
                .Returns(true);

            var node = SyntaxFactory.ClassDeclaration("ClassName");
            var source = SourceText.From("public class A {}");

            var sut = new GeneratorContext(_context);
            sut.AddOutput(node, "hint_name", source);

            DirectoryAssert.Exists(output.Location);
            Directory.GetFiles(output.Location).Length.ShouldBe(1);

            var fileName = Path.GetFileName(Directory.GetFiles(output.Location)[0]);
            fileName.ShouldBe(fileName.ToLowerInvariant());

            FileAssert.Exists(Path.Combine(output.Location, fileName));
        }
    }
}
