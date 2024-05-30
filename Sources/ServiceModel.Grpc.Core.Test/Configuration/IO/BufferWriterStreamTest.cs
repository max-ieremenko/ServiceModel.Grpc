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

namespace ServiceModel.Grpc.Configuration.IO;

[TestFixture]
public class BufferWriterStreamTest
{
    private BufferWriter<byte> _writer = null!;
    private BufferWriterStream _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _writer = new BufferWriter<byte>(1);
        _sut = new BufferWriterStream(_writer);
    }

    [TearDown]
    public void AfterEachTest()
    {
        _sut.Dispose();
        _writer.Dispose();
    }

    [Test]
    [TestCase(new byte[] { 1, 2, 3 }, 0, 3, new byte[] { 1, 2, 3 })]
    [TestCase(new byte[] { 1, 2, 3 }, 1, 2, new byte[] { 2, 3 })]
    [TestCase(new byte[] { 1, 2, 3 }, 0, 0, new byte[0])]
    [TestCase(new byte[] { 1, 2, 3 }, 2, 1, new byte[] { 3 })]
    public void Write(byte[] buffer, int offset, int count, byte[] expected)
    {
        _sut.Write(buffer, offset, count);

        var actual = _writer.ToArray();
        actual.Length.ShouldBe(expected.Length);
        actual.ShouldBe(expected);
    }

#if !NET462
    [Test]
    [TestCase(new byte[] { 1, 2, 3 }, 0, 3, new byte[] { 1, 2, 3 })]
    [TestCase(new byte[] { 1, 2, 3 }, 1, 2, new byte[] { 2, 3 })]
    [TestCase(new byte[] { 1, 2, 3 }, 0, 0, new byte[0])]
    [TestCase(new byte[] { 1, 2, 3 }, 2, 1, new byte[] { 3 })]
    public void WriteSpan(byte[] buffer, int offset, int count, byte[] expected)
    {
        var span = new ReadOnlySpan<byte>(buffer, offset, count);
        _sut.Write(span);

        var actual = _writer.ToArray();
        actual.Length.ShouldBe(expected.Length);
        actual.ShouldBe(expected);
    }
#endif

    [Test]
    public void WriteByte()
    {
        _sut.WriteByte(1);

        var actual = _writer.ToArray();
        actual.Length.ShouldBe(1);
        actual.ShouldBe(new byte[] { 1 });
    }
}