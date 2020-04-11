using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Shouldly;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public partial class ReflectionToolsTest
    {
        [Test]
        [TestCaseSource(nameof(GetImplementationOfMethodCases))]
        public void ImplementationOfMethod(Type declaringType, MethodInfo method, string expected)
        {
            var actual = ReflectionTools.ImplementationOfMethod(typeof(Implementation), declaringType, method);

            actual.ShouldNotBeNull();

            actual.GetCustomAttribute<DescriptionAttribute>().Description.ShouldBe(expected);
        }

        private static IEnumerable<TestCaseData> GetImplementationOfMethodCases()
        {
            var i1 = typeof(I1);
            yield return new TestCaseData(
                i1,
                i1.GetMethod(nameof(I1.Overload), Array.Empty<Type>()),
                "I1.Overload");

            yield return new TestCaseData(
                i1,
                i1.GetMethod(nameof(I1.Overload), new[] { typeof(int) }),
                "I1.Overload(int)");

            var i2 = typeof(I2);
            yield return new TestCaseData(
                i2,
                i2.GetMethod(nameof(I2.Overload), new[] { typeof(int) }),
                "I2.Overload(int)");
        }
    }
}
