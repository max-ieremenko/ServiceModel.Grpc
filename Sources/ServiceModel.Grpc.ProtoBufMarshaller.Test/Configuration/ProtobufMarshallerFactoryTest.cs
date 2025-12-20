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

using System.Reflection;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.ProtoBufMarshaller.Configuration;

[TestFixture]
public partial class ProtobufMarshallerFactoryTest
{
    [Test]
    [TestCaseSource(nameof(GetMarshallTestCases))]
    public void Marshall(object expected)
    {
        var payload = MarshallerExtensions.SerializeObject(ProtobufMarshallerFactory.Default, expected);
        var actual = MarshallerExtensions.DeserializeObject(ProtobufMarshallerFactory.Default, expected.GetType(), payload);

        actual.ShouldNotBeNull();
        Compare(expected, actual);
    }

    [Test]
    public void MarshallNull()
    {
        var marshaller = ProtobufMarshallerFactory.Default.CreateMarshaller<string>();
        var payload = MarshallerExtensions.Serialize(marshaller, null!);

        var actual = MarshallerExtensions.Deserialize(marshaller, payload);

        actual.ShouldBeEmpty();
    }

    private static void Compare(object expected, object actual)
    {
        expected.GetType().ShouldBe(actual.GetType());

        foreach (var property in expected.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var expectedValue = property.GetValue(expected);
            var actualValue = property.GetValue(actual);
            actualValue.ShouldBe(expectedValue, property.Name);
        }
    }

    private static IEnumerable<object> GetMarshallTestCases()
    {
        yield return new Message();
        yield return new Message<int>(1);
        yield return new Message<int, string>(1, "ab");

        yield return new Message<int, string, Person>(
            1,
            "ab",
            new Person
            {
                Name = "person name",
                Address = new PersonAddress { Street = "the street" }
            });

        yield return new Message<Person, Weapon, Weapon>(
            new Person { Name = "person name" },
            new Knife { HitDamage = 1 },
            new Sword { HitDamage = 3, Length = 5 });

        yield return new Message<Person, Knife>(
            new Person { Name = "person name" },
            new Knife { HitDamage = 1 });

        yield return new Message<IDictionary<string, Weapon>>(new Dictionary<string, Weapon>
        {
            { "value1", new Knife { HitDamage = 1 } },
            { "value2", new Sword { HitDamage = 3, Length = 5 } }
        });
    }
}