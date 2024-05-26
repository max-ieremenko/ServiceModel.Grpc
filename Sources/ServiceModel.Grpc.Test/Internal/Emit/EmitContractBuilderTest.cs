﻿// <copyright>
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
using System.Reflection;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;

namespace ServiceModel.Grpc.Internal.Emit;

[TestFixture]
public class EmitContractBuilderTest
{
    private Type _contractType = null!;
    private Func<IMarshallerFactory, object> _factory = null!;
    private ContractDescription _description = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _description = new ContractDescription(typeof(IContract));

        var builder = new EmitContractBuilder(_description);
        _contractType = builder.Build(ProxyAssembly.CreateModule(nameof(EmitContractBuilderTest)));
        _factory = EmitContractBuilder.CreateFactory(_contractType);
    }

    [Test]
    public void ValidateContractTypeFullName()
    {
        var actual = _contractType.Assembly.GetType(_description.ContractClassName, true, false);

        actual.ShouldBe(_contractType);
    }

    [Test]
    public void ValidateCtor()
    {
        _contractType.GetConstructors().Length.ShouldBe(1);

        var actual = _contractType.GetConstructors()[0];
        TestOutput.WriteLine(actual.Disassemble());

        actual.Attributes.ShouldHaveFlag(MethodAttributes.Public);
        actual.GetParameters().Length.ShouldBe(1);
        actual.GetParameters()[0].ParameterType.ShouldBe(typeof(IMarshallerFactory));

        Activator.CreateInstance(_contractType, DataContractMarshallerFactory.Default);
    }

    [Test]
    public void ValidateMethodFields()
    {
        var instance = _factory(DataContractMarshallerFactory.Default);

        var operations = _description.Services.SelectMany(i => i.Operations);
        foreach (var operation in operations)
        {
            var field = _contractType
                .GetField(operation.GrpcMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ShouldNotBeNull()
                .GetValue(instance)
                .ShouldBeAssignableTo<IMethod>()
                .ShouldNotBeNull();

            field.Type.ShouldBe(operation.Message.OperationType);
            field.Name.ShouldBe(operation.OperationName);
            field.ServiceName.ShouldBe(operation.ServiceName);
        }
    }

    [Test]
    public void ValidateDefinitionFields()
    {
        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                var handle = _contractType
                    .StaticFiled(operation.ClrDefinitionMethodName)
                    .GetValue(null)
                    .ShouldBeOfType<RuntimeMethodHandle>();

                var actual = MethodBase.GetMethodFromHandle(handle).ShouldNotBeNull();

                actual.ShouldBe(operation.Message.Operation);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                var handle = _contractType
                    .StaticFiled(entry.Async.ClrDefinitionMethodNameSyncVersion)
                    .GetValue(null)
                    .ShouldBeOfType<RuntimeMethodHandle>();

                var actual = MethodBase.GetMethodFromHandle(handle).ShouldNotBeNull();

                actual.ShouldBe(entry.Sync.Operation);
            }
        }
    }
}