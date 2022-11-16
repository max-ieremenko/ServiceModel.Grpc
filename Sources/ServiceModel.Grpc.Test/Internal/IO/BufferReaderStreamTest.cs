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
using System.Buffers;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal.IO;

[TestFixture]
public class BufferReaderStreamTest
{
    private readonly byte[] _sequence = { 1, 2, 3 };
    private BufferReaderStream _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new BufferReaderStream(new ReadOnlySequence<byte>(_sequence));
    }

    [Test]
    [TestCase(new byte[] { 5, 5 }, 0, 2, 2, new byte[] { 1, 2 })]
    [TestCase(new byte[] { 5, 5 }, 1, 1, 1, new byte[] { 5, 1 })]
    [TestCase(new byte[] { 5, 5, 5 }, 0, 3, 3, new byte[] { 1, 2, 3 })]
    [TestCase(new byte[] { 5, 5, 5, 5 }, 0, 4, 3, new byte[] { 1, 2, 3, 5 })]
    [TestCase(new byte[] { 5, 5, 5, 5, 5 }, 1, 4, 3, new byte[] { 5, 1, 2, 3, 5 })]
    public void Read(byte[] buffer, int offset, int count, int expectedLength, byte[] expected)
    {
        _sut.Read(buffer, offset, count).ShouldBe(expectedLength);

        buffer.ShouldBe(expected);
    }

#if !NET461
    [Test]
    [TestCase(new byte[] { 5, 5 }, 0, 2, 2, new byte[] { 1, 2 })]
    [TestCase(new byte[] { 5, 5 }, 1, 1, 1, new byte[] { 5, 1 })]
    [TestCase(new byte[] { 5, 5, 5 }, 0, 3, 3, new byte[] { 1, 2, 3 })]
    [TestCase(new byte[] { 5, 5, 5, 5 }, 0, 4, 3, new byte[] { 1, 2, 3, 5 })]
    [TestCase(new byte[] { 5, 5, 5, 5, 5 }, 1, 4, 3, new byte[] { 5, 1, 2, 3, 5 })]
    public void ReadSpan(byte[] buffer, int offset, int count, int expectedLength, byte[] expected)
    {
        var span = buffer.AsSpan(offset, count);
        _sut.Read(span).ShouldBe(expectedLength);

        buffer.ShouldBe(expected);
    }
#endif

    [Test]
    public void ReadByte()
    {
        _sut.ReadByte().ShouldBe(1);
        _sut.ReadByte().ShouldBe(2);
        _sut.ReadByte().ShouldBe(3);

        _sut.ReadByte().ShouldBe(-1);
        _sut.ReadByte().ShouldBe(-1);
    }
}