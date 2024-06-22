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
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Emit.Descriptions;

namespace ServiceModel.Grpc.Internal.Emit;

internal sealed class EmitContractBuilder
{
    private readonly ContractDescription<Type> _description;

    public EmitContractBuilder(ContractDescription<Type> description)
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
            className ?? NamingContract.Contract.Class(_description.BaseClassName),
            TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        // public (IMarshallerFactory marshallerFactory)
        var ctor = typeBuilder
            .DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                [typeof(IMarshallerFactory)])
            .GetILGenerator();

        ctor.Emit(OpCodes.Ldarg_0);
        ctor.Emit(OpCodes.Call, typeof(object).Constructor());

        foreach (var operation in GetAllOperations())
        {
            BuildMethod(operation, typeBuilder, ctor);
        }

        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                BuildDefinition(typeBuilder, NamingContract.Contract.ClrDefinitionMethod(operation.OperationName));
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                BuildDefinition(typeBuilder, NamingContract.Contract.ClrDefinitionMethodSync(entry.Async.OperationName));
            }
        }

        ctor.Emit(OpCodes.Ret);

        var result = typeBuilder.CreateTypeInfo()!;

        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                result
                    .StaticFiled(NamingContract.Contract.ClrDefinitionMethod(operation.OperationName))
                    .SetValue(null, operation.GetSource().MethodHandle);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                result
                    .StaticFiled(NamingContract.Contract.ClrDefinitionMethodSync(entry.Async.OperationName))
                    .SetValue(null, entry.Sync.GetSource().MethodHandle);
            }
        }

        return result;
    }

    private void BuildMethod(OperationDescription<Type> operation, TypeBuilder typeBuilder, ILGenerator ctor)
    {
        var filedType = typeof(GrpcMethod<,,,>).MakeGenericType(
            operation.HeaderRequestType.GetClrType(),
            operation.RequestType.GetClrType(),
            operation.HeaderResponseType.GetClrType(),
            operation.ResponseType.GetClrType());

        // public IMethod MethodX;
        var field = typeBuilder
            .DefineField(
                NamingContract.Contract.GrpcMethod(operation.OperationName),
                filedType,
                FieldAttributes.Public | FieldAttributes.InitOnly);

        // GrpcMethodFactory.Unary<,>(marshallerFactory, "serviceName", "name");
        ctor.Emit(OpCodes.Ldarg_0);
        ctor.Emit(OpCodes.Ldarg_1);
        ctor.Emit(OpCodes.Ldstr, operation.ServiceName);
        ctor.Emit(OpCodes.Ldstr, operation.OperationName);

        var factoryMethod = typeof(GrpcMethodFactory).StaticMethod(operation.OperationType.ToString());
        switch (operation.OperationType)
        {
            case MethodType.Unary:
                factoryMethod = factoryMethod.MakeGenericMethod(operation.RequestType.GetClrType(), operation.ResponseType.GetClrType());
                break;
            case MethodType.ClientStreaming:
                ctor.EmitLdcI4(operation.HeaderRequestType == null ? 0 : 1);
                factoryMethod = factoryMethod.MakeGenericMethod(
                    operation.HeaderRequestType.GetClrType(),
                    operation.RequestType.GetClrType(),
                    operation.ResponseType.GetClrType());
                break;
            case MethodType.ServerStreaming:
                ctor.EmitLdcI4(operation.HeaderResponseType == null ? 0 : 1);
                factoryMethod = factoryMethod.MakeGenericMethod(
                    operation.RequestType.GetClrType(),
                    operation.HeaderResponseType.GetClrType(),
                    operation.ResponseType.GetClrType());
                break;
            case MethodType.DuplexStreaming:
                ctor.EmitLdcI4(operation.HeaderRequestType == null ? 0 : 1);
                ctor.EmitLdcI4(operation.HeaderResponseType == null ? 0 : 1);
                factoryMethod = factoryMethod.MakeGenericMethod(
                    operation.HeaderRequestType.GetClrType(),
                    operation.RequestType.GetClrType(),
                    operation.HeaderResponseType.GetClrType(),
                    operation.ResponseType.GetClrType());
                break;
            default:
                throw new NotSupportedException();
        }

        ctor.Emit(OpCodes.Call, factoryMethod);
        ctor.Emit(OpCodes.Stfld, field);
    }

    private void BuildDefinition(TypeBuilder typeBuilder, string fieldName) =>
        typeBuilder
            .DefineField(
                fieldName,
                typeof(RuntimeMethodHandle),
                FieldAttributes.Public | FieldAttributes.Static);

    private IEnumerable<OperationDescription<Type>> GetAllOperations() => _description.Services.SelectMany(i => i.Operations);
}