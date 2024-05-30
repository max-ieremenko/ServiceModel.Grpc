// <copyright>
// Copyright Max Ieremenko
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
using System.IO;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public class MarshallerExtensionsTest
{
    private Mock<IMarshallerFactory> _marshallerFactory = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict);
    }

    [Test]
    public void SerializeBytesAsObject()
    {
        var expected = Guid.NewGuid().ToByteArray();

        var actual = MarshallerExtensions.SerializeObject(_marshallerFactory.Object, expected);

        actual.ShouldBe(expected);
    }

    [Test]
    public void DeserializeBytesAsObject()
    {
        var expected = Guid.NewGuid().ToByteArray();

        var actual = MarshallerExtensions.DeserializeObject(_marshallerFactory.Object, typeof(byte[]), expected);

        actual.ShouldBe(expected);
    }

    [Test]
    public void SerializeStringAsObject()
    {
        _marshallerFactory
            .Setup(f => f.CreateMarshaller<string>())
            .Returns(new Marshaller<string>(Serialize, Deserialize));

        var expected = "abc";

        var content = MarshallerExtensions.SerializeObject(_marshallerFactory.Object, expected);
        var actual = MarshallerExtensions.DeserializeObject(_marshallerFactory.Object, typeof(string), content);

        actual.ShouldBe(expected);
    }

    private static void Serialize(string value, SerializationContext context)
    {
        using var writer = new StreamWriter(context.AsStream());
        {
            writer.Write(value);
        }

        context.Complete();
    }

    private static string Deserialize(DeserializationContext context)
    {
        using var reader = new StreamReader(context.AsStream());
        return reader.ReadToEnd();
    }
}