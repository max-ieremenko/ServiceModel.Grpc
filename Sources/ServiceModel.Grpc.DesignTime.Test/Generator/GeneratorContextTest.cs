// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

            var additionalSourcesCtor = typeof(GeneratorExecutionContext)
                .Assembly
                .GetType("Microsoft.CodeAnalysis.AdditionalSourcesCollection", true, false)
                !.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(i => i.GetParameters().Length == 1);

            var additionalSources = additionalSourcesCtor.Invoke(new object[] { ".cs" });

            var ctor = typeof(GeneratorExecutionContext)
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(i => i.GetParameters().Length == 7);

            _context = (GeneratorExecutionContext)ctor.Invoke(new object?[]
            {
                null, // Compilation compilation
                null, // ParseOptions parseOptions
                ImmutableArray<AdditionalText>.Empty, // ImmutableArray<AdditionalText> additionalTexts
                optionsProvider.Object, // AnalyzerConfigOptionsProvider optionsProvider
                null, // ISyntaxReceiver syntaxReceiver
                additionalSources, // AdditionalSourcesCollection additionalSources
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
        public void GetOutputFileName()
        {
            var sut = new GeneratorContext(_context);
            var node = SyntaxFactory.ClassDeclaration("ClassName");

            var actual = sut.GetOutputFileName(node, "HintName");
            actual.ShouldBe("ClassName.HintName");

            // duplicate
            actual = sut.GetOutputFileName(node, "HintName");
            actual.ShouldBe("ClassName.HintName1");
        }
    }
}
