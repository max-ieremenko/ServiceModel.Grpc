using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ServiceModel.Grpc.Internal
{
    [TestFixture]
    public partial class ReflectionToolsTest
    {
        [Test]
        [TestCase(typeof(Task), true)]
        [TestCase(typeof(Task<int>), true)]
        [TestCase(typeof(ValueTask), true)]
        [TestCase(typeof(ValueTask<bool>), true)]
        [TestCase(typeof(IAsyncEnumerable<int>), false)]
        public void IsTask(Type type, bool expected)
        {
            ReflectionTools.IsTask(type).ShouldBe(expected);
        }

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
