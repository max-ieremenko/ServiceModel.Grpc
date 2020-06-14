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
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;
using ServiceModel.Grpc.Internal.IO;
using Shouldly;

namespace ServiceModel.Grpc.Configuration
{
    [TestFixture]
    public partial class MessageMarshallingTest
    {
        [Test]
        [TestCaseSource(nameof(GetMessages))]
        public void DataContractTest(object value) => RunTest(value, nameof(DataContractClone));

        [Test]
        [TestCaseSource(nameof(GetMessages))]
        public void ProtobufTest(object value) => RunTest(value, nameof(ProtobufClone));

#if !NETCOREAPP2_1
        [Test]
        [TestCaseSource(nameof(GetMessages))]
        public void JsonTest(object value) => RunTest(value, nameof(JsonClone));
#endif

        private static void RunTest(object value, string cloneMethodName)
        {
            var clone = typeof(MessageMarshallingTest)
                .StaticMethod(cloneMethodName)
                .MakeGenericMethod(value.GetType())
                .CreateDelegate(typeof(Func<,>).MakeGenericType(value.GetType(), value.GetType()));

            var actual = clone.DynamicInvoke(value);

            var result = new CompareLogic().Compare(value, actual);
            result.AreEqual.ShouldBeTrue(result.DifferencesString);
        }

        private static T DataContractClone<T>(T value)
        {
            var marshaller = DataContractMarshaller<T>.Default;
            return ContextualClone(value, marshaller);
        }

        private static T ProtobufClone<T>(T value)
        {
            var marshaller = ProtobufMarshallerFactory.Default.CreateMarshaller<T>();
            return ContextualClone(value, marshaller);
        }

        private static T JsonClone<T>(T value)
        {
            var marshaller = JsonMarshaller<T>.Default;
            var content = marshaller.Serializer(value);
            Console.WriteLine("Size: {0}", content.Length);
            return marshaller.Deserializer(content);
        }

        private static T ContextualClone<T>(T value, Marshaller<T> marshaller)
        {
            byte[] content;
            using (var serializationContext = new DefaultSerializationContext())
            {
                marshaller.ContextualSerializer(value, serializationContext);
                content = serializationContext.GetContent();
            }

            Console.WriteLine("Size: {0}", content.Length);
            return marshaller.ContextualDeserializer(new DefaultDeserializationContext(content));
        }

        private static IEnumerable<object> GetMessages()
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

            var bigMessageType = MessageBuilder.GetMessageType(Enumerable.Range(0, 1000).Select(_ => typeof(Person)).ToArray());
            var bigMessage = Activator.CreateInstance(
                bigMessageType,
                Enumerable.Range(0, 1000).Select(i => new Person { Name = "name " + i }).Cast<object>().ToArray());
            yield return bigMessage;
        }
    }
}
