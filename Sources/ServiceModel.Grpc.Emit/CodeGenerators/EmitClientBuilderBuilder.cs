﻿// <copyright>
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
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class EmitClientBuilderBuilder
{
    private readonly ContractDescription<Type> _description;
    private readonly Type _contractType;
    private readonly Type _clientType;
    private readonly Type _clientBuilderType;

    private FieldBuilder _contractField = null!;
    private FieldBuilder _clientCallInvokerField = null!;

    public EmitClientBuilderBuilder(ContractDescription<Type> description, Type contractType, Type clientType)
    {
        _description = description;
        _contractType = contractType;
        _clientType = clientType;
        _clientBuilderType = typeof(IClientBuilder<>).MakeConstructedGeneric(_description.ContractInterface);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IClientBuilder<>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IClientMethodBinder))]
    [UnconditionalSuppressMessage("Trimming", "IL2077:TypeBuilder.AddInterfaceImplementation")]
    public TypeInfo Build(ModuleBuilder moduleBuilder, string? className = default)
    {
        var typeBuilder = moduleBuilder.DefineType(
            className ?? NamingContract.ClientBuilder.Class(_description.BaseClassName),
            TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        typeBuilder.AddInterfaceImplementation(_clientBuilderType);

        BuildFields(typeBuilder);
        BuildInitializeMethod(typeBuilder);
        BuildBuildMethod(typeBuilder);

        return typeBuilder.CreateTypeInfo()!;
    }

    private void BuildFields(TypeBuilder typeBuilder)
    {
        _contractField = typeBuilder.DefineField("_contract", _contractType, FieldAttributes.Private);
        _clientCallInvokerField = typeBuilder.DefineField("_clientCallInvoker", typeof(IClientCallInvoker), FieldAttributes.Private);
    }

    private void BuildInitializeMethod(TypeBuilder typeBuilder)
    {
        var method = typeBuilder
            .DefineMethod(
                "Initialize",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(void),
                [typeof(IClientMethodBinder)]);

        typeBuilder.DefineMethodOverride(
            method,
            _clientBuilderType.InstanceMethod("Initialize"));

        var body = method.GetILGenerator();

        // _contract = new (methodBinder.MarshallerFactory);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceProperty(nameof(IClientMethodBinder.MarshallerFactory)).SafeGetGetMethod());
        body.Emit(OpCodes.Newobj, _contractType.Constructor(typeof(IMarshallerFactory)));
        body.Emit(OpCodes.Stfld, _contractField);

        var afterMetadata = body.DefineLabel();

        // if (methodBinder.RequiresMetadata)
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceProperty(nameof(IClientMethodBinder.RequiresMetadata)).SafeGetGetMethod());
        body.Emit(OpCodes.Brfalse, afterMetadata);

        // methodBinder.Add(_contract.Method, Contract.GetDescriptor());
        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                InvokeEmitAdapterAddMethod(body, NamingContract.Contract.GrpcMethod(operation.OperationName), NamingContract.Contract.DescriptorMethod(operation.OperationName));
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                InvokeEmitAdapterAddMethod(body, NamingContract.Contract.GrpcMethod(entry.Async.OperationName), NamingContract.Contract.DescriptorMethodSync(entry.Async.OperationName));
            }
        }

        body.MarkLabel(afterMetadata);

        // _clientCallInvokerField = methodBinder.CreateCallInvoker()
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceMethod(nameof(IClientMethodBinder.CreateCallInvoker)));
        body.Emit(OpCodes.Stfld, _clientCallInvokerField);

        body.Emit(OpCodes.Ret);
    }

    private void BuildBuildMethod(TypeBuilder typeBuilder)
    {
        var method = typeBuilder
            .DefineMethod(
                "Build",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                _description.ContractInterface,
                [typeof(CallInvoker)]);

        typeBuilder.DefineMethodOverride(
            method,
            _clientBuilderType.InstanceMethod("Build"));

        var body = method.GetILGenerator();

        // new (callInvoker, _contract, _defaultCallOptionsFactory)
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, _contractField);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, _clientCallInvokerField);
        body.Emit(OpCodes.Newobj, _clientType.Constructor(typeof(CallInvoker), _contractType, _clientCallInvokerField.FieldType));
        body.Emit(OpCodes.Ret);
    }

    private void InvokeEmitAdapterAddMethod(ILGenerator body, string grpcMethodName, string descriptorMethodName)
    {
        body.Emit(OpCodes.Ldarg_1); // methodBinder

        body.Emit(OpCodes.Ldarg_0); // _contract.Method
        body.Emit(OpCodes.Ldfld, _contractField);
        body.Emit(OpCodes.Ldfld, _contractType.InstanceFiled(grpcMethodName));

        // Contract.GetDescriptor()
        body.Emit(OpCodes.Call, _contractType.StaticMethod(descriptorMethodName));

        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceMethod(nameof(IClientMethodBinder.Add)));
    }
}