// <copyright>
// Copyright 2022 Max Ieremenko
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
using ServiceModel.Grpc.Internal;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;

namespace ServiceModel.Grpc.TestApi;

// see Message.tt
public abstract class MessageBuilderTestBase
{
    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void TypeAttributes(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);

        messageType.IsPublic.ShouldBeTrue();
        messageType.IsClass.ShouldBeTrue();
        messageType.IsSealed.ShouldBeTrue();
        messageType.IsGenericTypeDefinition.ShouldBeFalse();
        messageType.GenericTypeArguments.ShouldBe(typeArguments);
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void CustomAttributes(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);

        // [Serializable]
        messageType.GetCustomAttribute<SerializableAttribute>().ShouldNotBeNull();

        // [DataContract(Name = "m", Namespace = "s")]
        var dataContract = messageType.GetCustomAttribute<DataContractAttribute>();
        dataContract.ShouldNotBeNull();
        dataContract.Name.ShouldBe("m");
        dataContract.Namespace.ShouldBe("s");
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void DefaultCtor(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);
        var sut = Activator.CreateInstance(messageType);
        sut.ShouldNotBeNull();

        for (var i = 0; i < typeArguments.Length; i++)
        {
            var property = messageType.InstanceProperty("Value" + (i + 1));
            property.GetValue(sut, Array.Empty<object>()).ShouldBeNull();
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void ValuesCtor(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);

        var values = new object[typeArguments.Length];
        for (var i = 0; i < typeArguments.Length; i++)
        {
            values[i] = "the value " + i;
        }

        var sut = messageType.Constructor(typeArguments).Invoke(values);
        sut.ShouldNotBeNull();

        for (var i = 0; i < typeArguments.Length; i++)
        {
            var property = messageType.InstanceProperty("Value" + (i + 1));
            property.GetValue(sut, Array.Empty<object>()).ShouldBe(values[i]);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void PropertyAttributes(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);

        for (var i = 0; i < typeArguments.Length; i++)
        {
            var property = messageType.InstanceProperty("Value" + (i + 1));
            property.PropertyType.ShouldBe(typeArguments[i]);
            property.GetMethod.ShouldNotBeNull();
            property.GetMethod.IsPublic.ShouldBeTrue();
            property.SetMethod.ShouldNotBeNull();
            property.SetMethod.IsPublic.ShouldBeTrue();

            var dataMember = property.GetCustomAttribute<DataMemberAttribute>();
            dataMember.ShouldNotBeNull();
            dataMember.Name.ShouldBe("v" + (i + 1));
            dataMember.Order.ShouldBe(i + 1);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void Fields(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);

        for (var i = 0; i < typeArguments.Length; i++)
        {
            var field = messageType.InstanceFiled("_value" + (i + 1));
            field.FieldType.ShouldBe(typeArguments[i]);
            field.IsPrivate.ShouldBeTrue();
            field.Attributes.ShouldNotHaveFlag(FieldAttributes.InitOnly);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void PropertyAndFieldValues(Type[] typeArguments)
    {
        var messageType = GetMessageType(typeArguments);
        var sut = Activator.CreateInstance(messageType);
        sut.ShouldNotBeNull();

        for (var i = 0; i < typeArguments.Length; i++)
        {
            var property = messageType.InstanceProperty("Value" + (i + 1));
            var field = messageType.InstanceFiled("_value" + (i + 1));

            property.GetValue(sut).ShouldBe(null);
            field.GetValue(sut).ShouldBe(null);

            property.SetValue(sut, "new " + i);
            property.GetValue(sut).ShouldBe("new " + i);
            field.GetValue(sut).ShouldBe("new " + i);
        }
    }

    protected abstract Type GetMessageType(Type[] typeArguments);

    private static IEnumerable<TestCaseData> GetTestCases()
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