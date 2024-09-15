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

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal static class EmitContractBuilder
{
    public static Func<IMarshallerFactory, object> CreateFactory(Type implementationType)
    {
        var marshaller = Expression.Parameter(typeof(IMarshallerFactory), "marshallerFactory");

        var factory = Expression.New(
            implementationType.Constructor(typeof(IMarshallerFactory)),
            marshaller);

        return Expression.Lambda<Func<IMarshallerFactory, object>>(factory, marshaller).Compile();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(GrpcMethodFactory))]
    public static TypeInfo Build(ModuleBuilder moduleBuilder, ContractDescription<Type> description, string? className = default)
    {
        var typeBuilder = moduleBuilder.DefineType(
            className ?? NamingContract.Contract.Class(description.BaseClassName),
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

        foreach (var operation in description.Services.SelectMany(i => i.Operations))
        {
            BuildMethod(operation, typeBuilder, ctor);
        }

        var reflect = new ReflectDescriptor();
        foreach (var interfaceDescription in description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                var definitionMethod = BuildGetDefinition(typeBuilder, NamingContract.Contract.ClrDefinitionMethod(operation.OperationName));
                BuildGetDescriptor(typeBuilder, operation, reflect, NamingContract.Contract.DescriptorMethod(operation.OperationName), definitionMethod);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                var definitionMethod = BuildGetDefinition(typeBuilder, NamingContract.Contract.ClrDefinitionMethodSync(entry.Async.OperationName));
                BuildGetDescriptor(typeBuilder, entry.Sync, reflect, NamingContract.Contract.DescriptorMethodSync(entry.Async.OperationName), definitionMethod);
            }
        }

        ctor.Emit(OpCodes.Ret);

        var result = typeBuilder.CreateTypeInfo()!;

        foreach (var interfaceDescription in description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                var methodName = NamingContract.Contract.ClrDefinitionMethod(operation.OperationName);
                result
                    .StaticFiled($"_{methodName}")
                    .SetValue(null, operation.GetSource().MethodHandle);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                var methodName = NamingContract.Contract.ClrDefinitionMethodSync(entry.Async.OperationName);
                result
                    .StaticFiled($"_{methodName}")
                    .SetValue(null, entry.Sync.GetSource().MethodHandle);
            }
        }

        return result;
    }

    private static void BuildMethod(OperationDescription<Type> operation, TypeBuilder typeBuilder, ILGenerator ctor)
    {
        // public IMethod MethodX;
        var field = typeBuilder
            .DefineField(
                NamingContract.Contract.GrpcMethod(operation.OperationName),
                typeof(IMethod),
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
                factoryMethod = factoryMethod.MakeConstructedGeneric(operation.RequestType.GetClrType(), operation.ResponseType.GetClrType());
                break;
            case MethodType.ClientStreaming:
                ctor.EmitLdcI4(operation.HeaderRequestType == null ? 0 : 1);
                factoryMethod = factoryMethod.MakeConstructedGeneric(
                    operation.HeaderRequestType.GetClrType(),
                    operation.RequestType.GetClrType(),
                    operation.ResponseType.GetClrType());
                break;
            case MethodType.ServerStreaming:
                ctor.EmitLdcI4(operation.HeaderResponseType == null ? 0 : 1);
                factoryMethod = factoryMethod.MakeConstructedGeneric(
                    operation.RequestType.GetClrType(),
                    operation.HeaderResponseType.GetClrType(),
                    operation.ResponseType.GetClrType());
                break;
            case MethodType.DuplexStreaming:
                ctor.EmitLdcI4(operation.HeaderRequestType == null ? 0 : 1);
                ctor.EmitLdcI4(operation.HeaderResponseType == null ? 0 : 1);
                factoryMethod = factoryMethod.MakeConstructedGeneric(
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

    private static MethodInfo BuildGetDefinition(TypeBuilder typeBuilder, string methodName)
    {
        var field = typeBuilder.DefineField($"_{methodName}", typeof(RuntimeMethodHandle), FieldAttributes.Private | FieldAttributes.Static);

        var method = typeBuilder
            .DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, typeof(MethodInfo), []);

        var body = method.GetILGenerator();
        body.Emit(OpCodes.Ldsfld, field);
        body.Emit(OpCodes.Call, typeof(MethodBase).StaticMethod(nameof(MethodBase.GetMethodFromHandle), field.FieldType));
        body.Emit(OpCodes.Castclass, typeof(MethodInfo));
        body.Emit(OpCodes.Ret);

        return method;
    }

    private static void BuildGetDescriptor(TypeBuilder typeBuilder, OperationDescription<Type> operation, ReflectDescriptor reflect, string methodName, MethodInfo definitionMethod)
    {
        var body = typeBuilder
            .DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, typeof(IOperationDescriptor), [])
            .GetILGenerator();

        body.DeclareLocal(typeof(OperationDescriptorBuilder));

        body.Emit(OpCodes.Ldnull);
        body.Emit(OpCodes.Ldftn, definitionMethod);
        body.Emit(OpCodes.Newobj, reflect.FuncMethodInfoCtor);

        body.Emit(operation.IsAsync ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        body.Emit(OpCodes.Newobj, reflect.BuilderCtor);
        body.Emit(OpCodes.Stloc_0);

        // WithRequest
        body.Emit(OpCodes.Ldloca_S, 0);
        CreateMessageAccessor(body, operation.GetRequest(), reflect);
        body.Emit(OpCodes.Call, reflect.BuilderWithRequest);
        body.Emit(OpCodes.Stloc_0);

        // WithResponse
        body.Emit(OpCodes.Ldloca_S, 0);
        CreateMessageAccessor(body, operation.GetResponse(), reflect);
        body.Emit(OpCodes.Call, reflect.BuilderWithResponse);
        body.Emit(OpCodes.Stloc_0);

        if (operation.HeaderRequestTypeInput.Length > 0)
        {
            body.Emit(OpCodes.Ldloca_S, 0);
            body.EmitInt32Array(operation.HeaderRequestTypeInput);
            body.Emit(OpCodes.Call, reflect.BuilderWithRequestHeaderParameters);
            body.Emit(OpCodes.Stloc_0);
        }

        if (operation.RequestTypeInput.Length > 0)
        {
            body.Emit(OpCodes.Ldloca_S, 0);
            body.EmitInt32Array(operation.RequestTypeInput);
            body.Emit(OpCodes.Call, reflect.BuilderWithRequestParameters);
            body.Emit(OpCodes.Stloc_0);
        }

        if (operation.OperationType == MethodType.ClientStreaming || operation.OperationType == MethodType.DuplexStreaming)
        {
            body.Emit(OpCodes.Ldloca_S, 0);
            body.Emit(OpCodes.Call, reflect.CreateStreamAccessor.MakeConstructedGeneric(operation.RequestType.Properties[0]));
            body.Emit(OpCodes.Call, reflect.BuilderWithRequestStream);
            body.Emit(OpCodes.Stloc_0);
        }

        if (operation.OperationType == MethodType.ServerStreaming || operation.OperationType == MethodType.DuplexStreaming)
        {
            body.Emit(OpCodes.Ldloca_S, 0);
            body.Emit(OpCodes.Call, reflect.CreateStreamAccessor.MakeConstructedGeneric(operation.ResponseType.Properties[0]));
            body.Emit(OpCodes.Call, reflect.BuilderWithResponseStream);
            body.Emit(OpCodes.Stloc_0);
        }

        body.Emit(OpCodes.Ldloca_S, 0);
        body.Emit(OpCodes.Call, reflect.BuilderBuild);
        body.Emit(OpCodes.Ret);
    }

    private static void CreateMessageAccessor(ILGenerator body, (MessageDescription<Type> Message, string[] Names) args, ReflectDescriptor reflect)
    {
        var method = reflect.CreateMessageAccessor(args.Message.Properties.Length);
        if (args.Message.Properties.Length == 0)
        {
            body.Emit(OpCodes.Call, (MethodInfo)method);
            return;
        }

        // new string[5] {1,2,3,}
        body.EmitStringArray(args.Names);

        // CreateMessageAccessor
        if (args.Message.IsBuiltIn)
        {
            body.Emit(OpCodes.Call, ((MethodInfo)method).MakeConstructedGeneric(args.Message.Properties));
        }
        else
        {
            var ctor = ((Type)method).MakeConstructedGeneric(args.Message.Properties).Constructor(1);
            body.Emit(OpCodes.Newobj, ctor);
        }
    }
}