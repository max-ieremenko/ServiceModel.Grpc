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

using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal.IO;

[TestFixture]
public class BufferWriterTest
{
    private BufferWriter<int> _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new BufferWriter<int>(1);
    }

    [TearDown]
    public void AfterEachTest()
    {
        _sut.Dispose();
    }

    [Test]
    public void Empty()
    {
        _sut.ToArray().ShouldBeEmpty();
    }

    [Test]
    public void WriteByte()
    {
        var span = _sut.GetSpan(1);
        span[0] = 10;
        _sut.Advance(1);

        var actual = _sut.ToArray();

        actual.Length.ShouldBe(1);
        actual[0].ShouldBe(10);
    }

    [Test]
    public void Write()
    {
        var span = _sut.GetSpan(10);
        for (var i = 0; i < 10; i++)
        {
            span[i] = i;
        }

        _sut.Advance(5);

        _sut.ToArray().ShouldBe(new[] { 0, 1, 2, 3, 4 });
    }
}