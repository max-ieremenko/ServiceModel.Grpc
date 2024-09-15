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

using NUnit.Framework;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public class EmitMessageAccessorBuilder5Test
{
    private IMessageAccessor _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var accessorType = EmitMessageAccessorBuilder.GetMessageAccessorGenericType(5);
        var genericType = accessorType.MakeGenericType(typeof(long), typeof(int), typeof(int?), typeof(string), typeof(Type));

        _sut = Activator
            .CreateInstance(genericType, [new[] { "x1", "x2", "x3", "x4", "x5" }])
            .ShouldBeAssignableTo<IMessageAccessor>()
            .ShouldNotBeNull();
    }

    [Test]
    public void Names()
    {
        _sut.Names.ShouldBe(["x1", "x2", "x3", "x4", "x5"]);
    }

    [Test]
    public void GetInstanceType()
    {
        var actual = _sut.GetInstanceType();

        actual.IsConstructedGenericType.ShouldBeTrue();
        actual.GenericTypeArguments.ShouldBe([typeof(long), typeof(int), typeof(int?), typeof(string), typeof(Type)]);
    }

    [Test]
    [TestCase(0, typeof(long))]
    [TestCase(1, typeof(int))]
    [TestCase(2, typeof(int?))]
    [TestCase(3, typeof(string))]
    [TestCase(4, typeof(Type))]
    public void GetValueType(int property, Type expected)
    {
        _sut.GetValueType(property).ShouldBe(expected);
    }

    [Test]
    [TestCase(0, 0L, 10L)]
    [TestCase(1, 0, 10)]
    [TestCase(2, null, 10)]
    [TestCase(2, null, null)]
    [TestCase(3, null, "dummy")]
    [TestCase(3, null, null)]
    [TestCase(4, null, typeof(object))]
    [TestCase(4, null, null)]
    public void GetSetValue(int property, object initialValue, object newValue)
    {
        var message = _sut.CreateNew();

        _sut.GetValue(message, property).ShouldBe(initialValue);
        _sut.SetValue(message, property, newValue);
        _sut.GetValue(message, property).ShouldBe(newValue);
    }
}