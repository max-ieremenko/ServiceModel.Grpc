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

namespace ServiceModel.Grpc.DesignTime.Generator.Test;

[TestFixture]
[ImportGrpcService(typeof(ISomeService))]
public partial class CSharpContractBuilderTest
{
    [Test]
    public void GetDefinitionTest()
    {
        var actual = SomeServiceContract.GetSum5ValuesAsyncDefinition();

        actual.ShouldNotBeNull();
        actual.Name.ShouldBe(nameof(ISomeService.Sum5ValuesAsync));
        actual.DeclaringType.ShouldBe(typeof(ISomeService));
    }

    [Test]
    public void PartialMessage5Test()
    {
        var actual = new Message<string?, string, char, string?, char?>("f", "i", 'v', null, 'e');

        string.Join(string.Empty, actual.GetAllValues()).ShouldBe("five");
    }

    public partial class Message<T1, T2, T3, T4, T5>
    {
        public object[] GetAllValues() => [Value1, Value2, Value3, Value4, Value5];
    }
}