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
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class MessageBuilderTest
    {
        [Test]
        [TestCaseSource(nameof(GetMessageTypeTestCases))]
        public void GetMessageType(Type[] typeArguments)
        {
            var actual = MessageBuilder.GetMessageType(typeArguments);
            MessageBuilder.GetMessageType(typeArguments).ShouldBe(actual);

            actual.ShouldNotBeNull();
            Activator.CreateInstance(actual).ShouldNotBeNull(); // default ctor

            actual.GetCustomAttribute<SerializableAttribute>().ShouldNotBeNull();
            actual.GetCustomAttribute<DataContractAttribute>().ShouldNotBeNull();

            // new (,,,,)
            actual.Constructor(typeArguments).IsPublic.ShouldBeTrue();

            var values = new object[typeArguments.Length];
            for (var i = 0; i < typeArguments.Length; i++)
            {
                values[i] = "the value " + i;
            }

            ////Console.WriteLine(actual.Constructor(typeArguments).Disassemble());
            var instance = actual.Constructor(typeArguments).Invoke(values);

            for (var i = 0; i < typeArguments.Length; i++)
            {
                var property = actual.InstanceProperty("Value" + (i + 1));

                property.ShouldNotBeNull();
                property.PropertyType.ShouldBe(typeArguments[i]);

                property.GetMethod.ShouldNotBeNull();
                property.GetMethod!.IsPublic.ShouldBeTrue();
                property.SetMethod.ShouldNotBeNull();
                property.SetMethod!.IsPublic.ShouldBeTrue();

                if (i == 255)
                {
                    Console.WriteLine(property.GetMethod.Disassemble());
                }

                property.GetValue(instance).ShouldBe(values[i]);
                property.SetValue(instance, "new " + values[i]);
                property.GetValue(instance).ShouldBe("new " + values[i]);

                var dataMember = property.GetCustomAttribute<DataMemberAttribute>();
                dataMember.ShouldNotBeNull();
                dataMember!.Name.ShouldBe("v" + (i + 1));
                dataMember.Order.ShouldBe(i + 1);
            }
        }

        private static IEnumerable<TestCaseData> GetMessageTypeTestCases()
        {
            var cases = Enumerable.Range(0, 10).ToList();
            cases.Add(255);
            cases.Add(1000);

            foreach (var i in cases)
            {
                object test = Enumerable.Range(0, i).Select(_ => typeof(string)).ToArray();
                yield return new TestCaseData(test)
                {
                    TestName = i + " args"
                };
            }
        }
    }
}
