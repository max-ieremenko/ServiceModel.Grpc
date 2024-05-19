// <copyright>
// Copyright 2020-2024 Max Ieremenko
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
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

[TestFixture]
public partial class ContractDescriptionBuilderTest
{
    private readonly CSharpCompilation _compilation = CSharpCompilationExtensions.CreateDefault();

    [Test]
    [TestCase(typeof(IDuplicateOperationName))]
    [TestCase(typeof(IDuplicateServiceName))]
    public void DuplicateOperationName(Type serviceType)
    {
        var actual = Build(serviceType);

        actual.Interfaces.ShouldBeEmpty();
        actual.Services.ShouldNotBeEmpty();

        foreach (var service in actual.Services)
        {
            service.Methods.ShouldBeEmpty();
            service.Operations.ShouldBeEmpty();
            service.NotSupportedOperations.ShouldNotBeEmpty();

            TestOutput.WriteLine(service.NotSupportedOperations[0].Error);
            service.NotSupportedOperations[0].Error.ShouldNotBeNullOrEmpty();
        }
    }

    [Test]
    public void GenericContractTest()
    {
        var actual = Build(typeof(ICalculator<TheValue>));

        actual.BaseClassName.ShouldBe("CalculatorSome_Value");

        actual.Interfaces.ShouldBeEmpty();
        actual.Services.Length.ShouldBe(2);

        var calculator = actual.Services.First(i => i.InterfaceType.Name == "ICalculator");
        calculator.Methods.ShouldBeEmpty();
        calculator.NotSupportedOperations.ShouldBeEmpty();
        calculator.Operations.Length.ShouldBe(1);

        var sum = calculator.Operations[0];
        sum.OperationName.ShouldBe("Sum");
        sum.ServiceName.ShouldBe("ICalculator-Some-Value");
        sum.ClrDefinitionMethodName.ShouldBe("GetSumDefinition");

        var service = actual.Services.First(i => i.InterfaceType.Name == "IGenericService");
        service.Methods.ShouldBeEmpty();
        service.NotSupportedOperations.ShouldBeEmpty();
        service.Operations.Length.ShouldBe(1);

        var ping = service.Operations[0];
        ping.OperationName.ShouldBe("Ping");
        ping.ServiceName.ShouldBe("IGenericService-Some-Value");
        ping.ClrDefinitionMethodName.ShouldBe("GetPingDefinition");
    }

    [Test]
    public void SyncOverAsync()
    {
        var actual = Build(typeof(ISyncOveAsync));

        actual.Interfaces.ShouldBeEmpty();

        actual.Services.Length.ShouldBe(1);
        actual.Services[0].Methods.ShouldBeEmpty();
        actual.Services[0].NotSupportedOperations.ShouldBeEmpty();

        actual.Services[0].Operations.Length.ShouldBe(1);
        actual.Services[0].Operations[0].OperationName.ShouldBe(nameof(ISyncOveAsync.PingAsync));
        actual.Services[0].Operations[0].ClrDefinitionMethodName.ShouldBe("GetPingAsyncDefinition");

        actual.Services[0].SyncOverAsync.Length.ShouldBe(1);
        actual.Services[0].SyncOverAsync[0].Async.ShouldBe(actual.Services[0].Operations[0]);
        actual.Services[0].SyncOverAsync[0].Sync.Method.Name.ShouldBe(nameof(ISyncOveAsync.Ping));
        actual.Services[0].SyncOverAsync[0].Sync.ClrDefinitionMethodName.ShouldBe("GetPingAsyncDefinitionSync");
    }

    private ContractDescription Build(Type serviceType)
    {
        var symbol = _compilation.ResolveTypeSymbol(serviceType);
        return new ContractDescriptionBuilder(symbol).Build().ShouldBeOfType<ContractDescription>();
    }
}