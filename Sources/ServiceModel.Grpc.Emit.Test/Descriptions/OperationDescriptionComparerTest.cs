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
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions;

[TestFixture]
public partial class OperationDescriptionComparerTest
{
    [Test]
    [TestCaseSource(nameof(GetIsCompatibleToCases))]
    public void IsCompatibleWith(MethodInfo method, MethodInfo other, bool expected)
    {
        var description = ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy");
        var otherDescription = ContractDescriptionBuilder.BuildOperation(other, "dummy", "dummy");

        description.ShouldNotBeNull();
        otherDescription.ShouldNotBeNull();

        description.IsCompatibleWith(otherDescription, new ReflectType()).ShouldBe(expected);
        otherDescription.IsCompatibleWith(description, new ReflectType()).ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetIsCompatibleToCases()
    {
        var methodByName = ReflectionTools
            .GetInstanceMethods(typeof(IsCompatibleToCases))
            .ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var method in methodByName.Values)
        {
            yield return new TestCaseData(method, method, true)
            {
                TestName = $"{method.Name} vs {method.Name}"
            };

            foreach (var compatibleTo in method.GetCustomAttributes<CompatibleToAttribute>())
            {
                var other = methodByName[compatibleTo.MethodName];

                yield return new TestCaseData(method, other, true)
                {
                    TestName = $"{method.Name} vs {other.Name}"
                };
            }

            foreach (var notCompatibleTo in method.GetCustomAttributes<NotCompatibleToAttribute>())
            {
                var other = methodByName[notCompatibleTo.MethodName];

                yield return new TestCaseData(method, other, false)
                {
                    TestName = $"{method.Name} vs {other.Name}"
                };
            }
        }
    }
}