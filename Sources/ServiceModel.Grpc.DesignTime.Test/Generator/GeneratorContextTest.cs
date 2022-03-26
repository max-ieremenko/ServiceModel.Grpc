// <copyright>
// Copyright 2020-2022 Max Ieremenko
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

using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    [TestFixture]
    public class GeneratorContextTest
    {
        private GeneratorContext _sut = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new GeneratorContext(null!, null!, CancellationToken.None, null!);
        }

        [Test]
        public void GetOutputFileName()
        {
            var node = SyntaxFactory.ClassDeclaration("ClassName");

            var actual = _sut.GetOutputFileName(node, "HintName");
            actual.ShouldBe("ClassName.HintName.g.cs");

            // duplicate
            actual = _sut.GetOutputFileName(node, "HintName");
            actual.ShouldBe("ClassName.HintName1.g.cs");
        }
    }
}
