// <copyright>
// Copyright 2020-2023 Max Ieremenko
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
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit;

internal sealed class EmitClientBuilderBuilder
{
    private readonly ContractDescription _description;
    private readonly Type _contractType;
    private readonly Type _clientType;
    private readonly Type _clientBuilderType;

    private FieldBuilder _contractField = null!;
    private FieldBuilder _callOptionsFactoryField = null!;
    private FieldBuilder _filterHandlerFactoryField = null!;

    public EmitClientBuilderBuilder(ContractDescription description, Type contractType, Type clientType)
    {
        _description = description;
        _contractType = contractType;
        _clientType = clientType;
        _clientBuilderType = typeof(IClientBuilder<>).MakeGenericType(_description.ServiceType);
    }

    public TypeInfo Build(ModuleBuilder moduleBuilder, string? className = default)
    {
        var typeBuilder = moduleBuilder.DefineType(
            className ?? _description.ClientBuilderClassName,
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
        _callOptionsFactoryField = typeBuilder.DefineField("_defaultCallOptionsFactory", typeof(Func<CallOptions>), FieldAttributes.Private);
        _filterHandlerFactoryField = typeBuilder.DefineField("_filterHandlerFactory", typeof(IClientCallFilterHandlerFactory), FieldAttributes.Private);
    }

    private void BuildInitializeMethod(TypeBuilder typeBuilder)
    {
        var method = typeBuilder
            .DefineMethod(
                "Initialize",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(void),
                new[] { typeof(IClientMethodBinder) });

        typeBuilder.DefineMethodOverride(
            method,
            _clientBuilderType.InstanceMethod("Initialize"));

        var body = method.GetILGenerator();

        // _contract = new (methodBinder.MarshallerFactory);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceProperty(nameof(IClientMethodBinder.MarshallerFactory)).GetMethod);
        body.Emit(OpCodes.Newobj, _contractType.Constructor(typeof(IMarshallerFactory)));
        body.Emit(OpCodes.Stfld, _contractField);

        // _defaultCallOptionsFactory = methodBinder.DefaultCallOptionsFactory
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceProperty(nameof(IClientMethodBinder.DefaultCallOptionsFactory)).GetMethod);
        body.Emit(OpCodes.Stfld, _callOptionsFactoryField);

        // EmitAdapter.AddMethod(methodBinder, _contract.Method, Contract.Handle);
        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                InvokeEmitAdapterAddMethod(body, operation.GrpcMethodName, operation.ClrDefinitionMethodName);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                InvokeEmitAdapterAddMethod(body, entry.Async.GrpcMethodName, entry.Async.ClrDefinitionMethodNameSyncVersion);
            }
        }

        // _filterHandlerFactory = methodBinder.CreateFilterHandlerFactory()
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Callvirt, typeof(IClientMethodBinder).InstanceMethod(nameof(IClientMethodBinder.CreateFilterHandlerFactory)));
        body.Emit(OpCodes.Stfld, _filterHandlerFactoryField);

        body.Emit(OpCodes.Ret);
    }

    private void BuildBuildMethod(TypeBuilder typeBuilder)
    {
        var method = typeBuilder
            .DefineMethod(
                "Build",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                _description.ServiceType,
                new[] { typeof(CallInvoker) });

        typeBuilder.DefineMethodOverride(
            method,
            _clientBuilderType.InstanceMethod("Build"));

        var body = method.GetILGenerator();

        // new (callInvoker, _contract, _defaultCallOptionsFactory)
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, _contractField);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, _callOptionsFactoryField);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, _filterHandlerFactoryField);
        body.Emit(OpCodes.Newobj, _clientType.Constructor(typeof(CallInvoker), _contractType, _callOptionsFactoryField.FieldType, _filterHandlerFactoryField.FieldType));
        body.Emit(OpCodes.Ret);
    }

    private void InvokeEmitAdapterAddMethod(ILGenerator body, string grpcMethodName, string clrDefinitionMethodName)
    {
        body.Emit(OpCodes.Ldarg_1); // methodBinder

        body.Emit(OpCodes.Ldarg_0); // _contract.Method
        body.Emit(OpCodes.Ldfld, _contractField);
        body.Emit(OpCodes.Ldfld, _contractType.InstanceFiled(grpcMethodName));

        body.Emit(OpCodes.Ldsfld, _contractType.StaticFiled(clrDefinitionMethodName)); // Contract.Handle

        body.Emit(OpCodes.Call, typeof(EmitAdapter).StaticMethod(nameof(EmitAdapter.AddMethod))); // EmitAdapter.AddMethod();
    }
}