// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit;

internal sealed class EmitContractBuilder
{
    private readonly ContractDescription _description;

    public EmitContractBuilder(ContractDescription description)
    {
        _description = description;
    }

    public static Func<IMarshallerFactory, object> CreateFactory(Type implementationType)
    {
        var marshaller = Expression.Parameter(typeof(IMarshallerFactory), "marshallerFactory");

        var factory = Expression.New(
            implementationType.Constructor(typeof(IMarshallerFactory)),
            marshaller);

        return Expression.Lambda<Func<IMarshallerFactory, object>>(factory, marshaller).Compile();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Method<,>))]
    public TypeInfo Build(ModuleBuilder moduleBuilder, string? className = default)
    {
        var typeBuilder = moduleBuilder.DefineType(
            className ?? _description.ContractClassName,
            TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        // public (IMarshallerFactory marshallerFactory)
        var ctor = typeBuilder
            .DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                new[] { typeof(IMarshallerFactory) })
            .GetILGenerator();

        ctor.Emit(OpCodes.Ldarg_0);
        ctor.Emit(OpCodes.Call, typeof(object).Constructor());

        foreach (var operation in GetAllOperations())
        {
            BuildMethod(operation, typeBuilder, ctor);
            BuildRequestHeader(operation, typeBuilder, ctor);
            BuildResponseHeader(operation, typeBuilder, ctor);
        }

        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                BuildDefinition(typeBuilder, operation.ClrDefinitionMethodName);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                BuildDefinition(typeBuilder, entry.Async.ClrDefinitionMethodNameSyncVersion);
            }
        }

        ctor.Emit(OpCodes.Ret);

        var result = typeBuilder.CreateTypeInfo()!;

        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                result.StaticFiled(operation.ClrDefinitionMethodName).SetValue(null, operation.Message.Operation.MethodHandle);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                result.StaticFiled(entry.Async.ClrDefinitionMethodNameSyncVersion).SetValue(null, entry.Sync.Operation.MethodHandle);
            }
        }

        return result;
    }

    private void BuildMethod(OperationDescription operation, TypeBuilder typeBuilder, ILGenerator ctor)
    {
        var filedType = typeof(Method<,>).MakeGenericType(operation.Message.RequestType, operation.Message.ResponseType);

        // public Method<string, string> MethodX;
        var field = typeBuilder
            .DefineField(
                operation.GrpcMethodName,
                filedType,
                FieldAttributes.Public | FieldAttributes.InitOnly);

        var createMarshaller = typeof(IMarshallerFactory).InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller));

        ctor.Emit(OpCodes.Ldarg_0);

        // new Method<>(MethodType.Unary, serviceName, operationName, marshallerFactory.CreateMarshaller<>(), marshallerFactory.CreateMarshaller<>());
        ctor.EmitLdcI4((int)operation.Message.OperationType); // MethodType
        ctor.Emit(OpCodes.Ldstr, operation.ServiceName);
        ctor.Emit(OpCodes.Ldstr, operation.OperationName);
        ctor.Emit(OpCodes.Ldarg_1);
        ctor.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(operation.Message.RequestType));
        ctor.Emit(OpCodes.Ldarg_1);
        ctor.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(operation.Message.ResponseType));
        ctor.Emit(
            OpCodes.Newobj,
            filedType.Constructor(
                typeof(MethodType),
                typeof(string),
                typeof(string),
                typeof(Marshaller<>).MakeGenericType(operation.Message.RequestType),
                typeof(Marshaller<>).MakeGenericType(operation.Message.ResponseType)));

        ctor.Emit(OpCodes.Stfld, field);
    }

    private void BuildRequestHeader(OperationDescription operation, TypeBuilder typeBuilder, ILGenerator ctor)
    {
        if (operation.Message.HeaderRequestType == null)
        {
            return;
        }

        var filedType = typeof(Marshaller<>).MakeGenericType(operation.Message.HeaderRequestType);

        // public Marshaller<> HeaderX
        var field = typeBuilder
            .DefineField(
                operation.GrpcMethodInputHeaderName,
                filedType,
                FieldAttributes.Public | FieldAttributes.InitOnly);

        var createMarshaller = typeof(IMarshallerFactory)
            .InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller))
            .MakeGenericMethod(operation.Message.HeaderRequestType);

        ctor.Emit(OpCodes.Ldarg_0);

        // marshallerFactory.CreateMarshaller<>()
        ctor.Emit(OpCodes.Ldarg_1);
        ctor.Emit(OpCodes.Callvirt, createMarshaller);
        ctor.Emit(OpCodes.Stfld, field);
    }

    private void BuildResponseHeader(OperationDescription operation, TypeBuilder typeBuilder, ILGenerator ctor)
    {
        if (operation.Message.HeaderResponseType == null)
        {
            return;
        }

        var filedType = typeof(Marshaller<>).MakeGenericType(operation.Message.HeaderResponseType);

        // public Marshaller<> HeaderX
        var field = typeBuilder
            .DefineField(
                operation.GrpcMethodOutputHeaderName,
                filedType,
                FieldAttributes.Public | FieldAttributes.InitOnly);

        var createMarshaller = typeof(IMarshallerFactory)
            .InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller))
            .MakeGenericMethod(operation.Message.HeaderResponseType);

        ctor.Emit(OpCodes.Ldarg_0);

        // marshallerFactory.CreateMarshaller<>()
        ctor.Emit(OpCodes.Ldarg_1);
        ctor.Emit(OpCodes.Callvirt, createMarshaller);
        ctor.Emit(OpCodes.Stfld, field);
    }

    private void BuildDefinition(TypeBuilder typeBuilder, string fieldName)
    {
        typeBuilder
            .DefineField(
                fieldName,
                typeof(RuntimeMethodHandle),
                FieldAttributes.Public | FieldAttributes.Static);
    }

    private IEnumerable<OperationDescription> GetAllOperations()
    {
        return _description.Services.SelectMany(i => i.Operations);
    }
}