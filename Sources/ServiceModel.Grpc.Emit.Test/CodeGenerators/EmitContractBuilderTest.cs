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
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly.ShouldlyExtensionMethods;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public class EmitContractBuilderTest
{
    private Type _contractType = null!;
    private Func<IMarshallerFactory, object> _factory = null!;
    private ContractDescription<Type> _description = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _description = ContractDescriptionBuilder.Build(typeof(IMultipurposeService));

        _contractType = EmitContractBuilder.Build(ProxyAssembly.CreateModule(nameof(EmitContractBuilderTest)), _description);
        _factory = EmitContractBuilder.CreateFactory(_contractType);
    }

    [Test]
    public void ValidateContractTypeFullName()
    {
        var actual = _contractType.Assembly.GetType(NamingContract.Contract.Class(_description.BaseClassName), true, false);

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
                .GetField(NamingContract.Contract.GrpcMethod(operation.OperationName), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ShouldNotBeNull()
                .GetValue(instance)
                .ShouldBeAssignableTo<IMethod>()
                .ShouldNotBeNull();

            field.Type.ShouldBe(operation.OperationType);
            field.Name.ShouldBe(operation.OperationName);
            field.ServiceName.ShouldBe(operation.ServiceName);
        }
    }

    [Test]
    public void ValidateDefinitions()
    {
        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                var actual = _contractType
                    .StaticMethod(NamingContract.Contract.ClrDefinitionMethod(operation.OperationName))
                    .Invoke(null, null)
                    .ShouldNotBeNull();

                actual.ShouldBe(operation.GetSource());
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                var actual = _contractType
                    .StaticMethod(NamingContract.Contract.ClrDefinitionMethodSync(entry.Async.OperationName))
                    .Invoke(null, null)
                    .ShouldNotBeNull();

                actual.ShouldBe(entry.Sync.GetSource());
            }
        }
    }

    [Test]
    public void ValidateDescriptors()
    {
        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                ValidateDescriptor(operation, NamingContract.Contract.DescriptorMethod(operation.OperationName));
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                ValidateDescriptor(entry.Sync, NamingContract.Contract.DescriptorMethodSync(entry.Async.OperationName));
            }
        }
    }

    private void ValidateDescriptor(OperationDescription<Type> operation, string methodName)
    {
        var actual = _contractType
            .StaticMethod(methodName)
            .Invoke(null, null)
            .ShouldBeAssignableTo<IOperationDescriptor>()
            .ShouldNotBeNull();

        actual.IsAsync().ShouldBe(operation.IsAsync);
        actual.GetContractMethod().ShouldBe(operation.GetSource());

        actual.GetRequestAccessor().Names.ShouldBe(operation.GetRequest().Names);
        actual.GetRequestAccessor().GetInstanceType().ShouldBe(operation.GetRequest().Message.GetClrType());

        actual.GetResponseAccessor().Names.ShouldBe(operation.GetResponse().Names);
        actual.GetResponseAccessor().GetInstanceType().ShouldBe(operation.GetResponse().Message.GetClrType());

        actual.GetRequestHeaderParameters().ShouldBe(operation.HeaderRequestTypeInput);
        actual.GetRequestParameters().ShouldBe(operation.RequestTypeInput);

        if (operation.OperationType == MethodType.ClientStreaming || operation.OperationType == MethodType.DuplexStreaming)
        {
            actual
                .GetRequestStreamAccessor()
                .ShouldNotBeNull()
                .GetInstanceType()
                .ShouldBe(typeof(IAsyncEnumerable<>).MakeGenericType(operation.RequestType.Properties[0]));
        }
        else
        {
            actual.GetRequestStreamAccessor().ShouldBeNull();
        }

        if (operation.OperationType == MethodType.ServerStreaming || operation.OperationType == MethodType.DuplexStreaming)
        {
            actual
                .GetResponseStreamAccessor()
                .ShouldNotBeNull()
                .GetInstanceType()
                .ShouldBe(typeof(IAsyncEnumerable<>).MakeGenericType(operation.ResponseType.Properties[0]));
        }
        else
        {
            actual.GetResponseStreamAccessor().ShouldBeNull();
        }
    }
}