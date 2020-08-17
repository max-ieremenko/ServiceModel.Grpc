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
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Internal.CSharp
{
    [TestFixture]
    public class CodeStringBuilderTest
    {
        private CodeStringBuilder _sut = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new CodeStringBuilder();
        }

        [Test]
        public void Indent1()
        {
            _sut.AppendLine("{");

            using (_sut.Indent())
            {
                _sut.Append("x").Append(" = ").AppendLine("1;");
            }

            _sut.Append("}");

            Console.WriteLine("----------");
            Console.WriteLine(_sut.ToString());
            Console.WriteLine("----------");
            _sut.ToString().ShouldBe(@"{
    x = 1;
}");
        }
    }
}
