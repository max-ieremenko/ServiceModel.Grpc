// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Linq;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal;

[TestFixture]
public partial class ContractDescriptionTest
{
    [Test]
    [TestCase(typeof(IDuplicateOperationName))]
    [TestCase(typeof(IDuplicateServiceName))]
    public void DuplicateOperationName(Type serviceType)
    {
        var sut = new ContractDescription(serviceType);

        sut.Interfaces.Count.ShouldBe(0);
        sut.Services.Count.ShouldNotBe(0);

        foreach (var service in sut.Services)
        {
            service.Methods.Count.ShouldBe(0);
            service.Operations.Count.ShouldBe(0);
            service.NotSupportedOperations.Count.ShouldNotBe(0);

            TestOutput.WriteLine(service.NotSupportedOperations[0].Error);
            service.NotSupportedOperations[0].Error.ShouldNotBeNullOrEmpty();
        }
    }

    [Test]
    public void GenericContractTest()
    {
        var sut = new ContractDescription(typeof(ICalculator<TheValue>));

        sut.ClientClassName.ShouldEndWith(".ICalculator-Some-ValueClient");
        sut.ClientBuilderClassName.ShouldEndWith(".ICalculator-Some-ValueClientBuilder");
        sut.ContractClassName.ShouldEndWith(".ICalculator-Some-ValueContract");

        sut.Interfaces.Count.ShouldBe(0);
        sut.Services.Count.ShouldBe(2);

        var calculator = sut.Services.First(i => i.InterfaceType == typeof(ICalculator<TheValue>));
        calculator.Methods.Count.ShouldBe(0);
        calculator.NotSupportedOperations.Count.ShouldBe(0);
        calculator.Operations.Count.ShouldBe(1);

        var sum = calculator.Operations[0];
        sum.OperationName.ShouldBe("Sum");
        sum.ServiceName.ShouldBe("ICalculator-Some-Value");

        var service = sut.Services.First(i => i.InterfaceType == typeof(IGenericService<TheValue>));
        service.Methods.Count.ShouldBe(0);
        service.NotSupportedOperations.Count.ShouldBe(0);
        service.Operations.Count.ShouldBe(1);

        var ping = service.Operations[0];
        ping.OperationName.ShouldBe("Ping");
        ping.ServiceName.ShouldBe("IGenericService-Some-Value");
    }

    [Test]
    public void SyncOverAsync()
    {
        var sut = new ContractDescription(typeof(ISyncOveAsync));

        sut.Interfaces.ShouldBeEmpty();

        sut.Services.Count.ShouldBe(1);
        sut.Services[0].Methods.ShouldBeEmpty();
        sut.Services[0].NotSupportedOperations.ShouldBeEmpty();

        sut.Services[0].Operations.Count.ShouldBe(1);
        sut.Services[0].Operations[0].OperationName.ShouldBe(nameof(ISyncOveAsync.PingAsync));

        sut.Services[0].SyncOverAsync.Count.ShouldBe(1);
        sut.Services[0].SyncOverAsync[0].Async.ShouldBe(sut.Services[0].Operations[0]);
        sut.Services[0].SyncOverAsync[0].Sync.Operation.Name.ShouldBe(nameof(ISyncOveAsync.Ping));
    }
}