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
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions;

[TestFixture]
public partial class ContractDescriptionBuilderTest
{
    [Test]
    [TestCase(typeof(IDuplicateOperationName))]
    [TestCase(typeof(IDuplicateServiceName))]
    public void DuplicateOperationName(Type serviceType)
    {
        var actual = ContractDescriptionBuilder<Type>.Build(serviceType, "dummy", new ReflectType());

        actual.Interfaces.ShouldBeEmpty();
        actual.Services.ShouldNotBeEmpty();

        foreach (var service in actual.Services)
        {
            service.Methods.ShouldBeEmpty();
            service.Operations.ShouldBeEmpty();
            service.NotSupportedOperations.ShouldNotBeEmpty();

            service.NotSupportedOperations[0].Error.ShouldNotBeNullOrEmpty();
        }
    }

    [Test]
    public void GenericContractTest()
    {
        var actual = ContractDescriptionBuilder<Type>.Build(typeof(ICalculator<TheValue>), "dummy", new ReflectType());

        actual.Interfaces.ShouldBeEmpty();
        actual.Services.Length.ShouldBe(2);

        var calculator = actual.Services.First(i => i.InterfaceType == typeof(ICalculator<TheValue>));
        calculator.Methods.ShouldBeEmpty();
        calculator.NotSupportedOperations.ShouldBeEmpty();
        calculator.Operations.Length.ShouldBe(1);

        var sum = calculator.Operations[0];
        sum.OperationName.ShouldBe("Sum");
        sum.ServiceName.ShouldBe("ICalculator-Some-Value");

        var service = actual.Services.First(i => i.InterfaceType == typeof(IGenericService<TheValue>));
        service.Methods.ShouldBeEmpty();
        service.NotSupportedOperations.ShouldBeEmpty();
        service.Operations.Length.ShouldBe(1);

        var ping = service.Operations[0];
        ping.OperationName.ShouldBe("Ping");
        ping.ServiceName.ShouldBe("IGenericService-Some-Value");
    }

    [Test]
    public void SyncOverAsync()
    {
        var actual = ContractDescriptionBuilder<Type>.Build(typeof(ISyncOveAsync), "dummy", new ReflectType());

        actual.Interfaces.ShouldBeEmpty();

        actual.Services.Length.ShouldBe(1);
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[0].Operations.Length.ShouldBe(1);
        actual.Services[0].Operations[0].OperationName.ShouldBe(nameof(ISyncOveAsync.PingAsync));

        actual.Services[0].SyncOverAsync.Length.ShouldBe(1);
        actual.Services[0].SyncOverAsync[0].Async.ShouldBe(actual.Services[0].Operations[0]);
        actual.Services[0].SyncOverAsync[0].Sync.Method.Name.ShouldBe(nameof(ISyncOveAsync.Ping));
    }
}