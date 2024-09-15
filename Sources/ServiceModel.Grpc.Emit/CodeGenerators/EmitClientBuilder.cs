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

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class EmitClientBuilder
{
    private readonly ContractDescription<Type> _description;
    private readonly Type _contractType;
    private readonly HashSet<string> _uniqueMemberNames;
    private readonly ReflectClientCallInvoker _reflectClientCallInvoker;

    private TypeBuilder _typeBuilder = null!;
    private FieldBuilder _contractField = null!;
    private FieldBuilder _callInvokerField = null!;
    private FieldBuilder _clientCallInvokerField = null!;

    public EmitClientBuilder(ContractDescription<Type> description, Type contractType)
    {
        _description = description;
        _contractType = contractType;
        _uniqueMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _reflectClientCallInvoker = new ReflectClientCallInvoker();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072:TypeBuilder.AddInterfaceImplementation")]
    public TypeInfo Build(ModuleBuilder moduleBuilder)
    {
        _typeBuilder = moduleBuilder
            .DefineType(
                NamingContract.Client.Class(_description.BaseClassName),
                TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        _contractField = _typeBuilder
            .DefineField(
                "__contract",
                _contractType,
                FieldAttributes.Private | FieldAttributes.InitOnly);

        _callInvokerField = _typeBuilder
            .DefineField(
                "__callInvoker",
                typeof(CallInvoker),
                FieldAttributes.Private | FieldAttributes.InitOnly);

        _clientCallInvokerField = _typeBuilder
            .DefineField(
                "_clientCallInvoker",
                typeof(IClientCallInvoker),
                FieldAttributes.Private | FieldAttributes.InitOnly);

        BuildCtor();

        foreach (var interfaceDescription in _description.Interfaces)
        {
            _typeBuilder.AddInterfaceImplementation(interfaceDescription.InterfaceType);

            foreach (var method in interfaceDescription.Methods)
            {
                ImplementNotSupportedMethod(method.GetSource(), method.Error);
            }
        }

        foreach (var interfaceDescription in _description.Services)
        {
            _typeBuilder.AddInterfaceImplementation(interfaceDescription.InterfaceType);

            foreach (var method in interfaceDescription.Methods)
            {
                ImplementNotSupportedMethod(method.GetSource(), method.Error);
            }

            foreach (var method in interfaceDescription.NotSupportedOperations)
            {
                ImplementNotSupportedMethod(method.GetSource(), method.Error);
            }

            foreach (var operation in interfaceDescription.Operations)
            {
                ImplementMethod(operation, null);
            }

            foreach (var entry in interfaceDescription.SyncOverAsync)
            {
                ImplementMethod(entry.Sync, NamingContract.Contract.GrpcMethod(entry.Async.OperationName));
            }
        }

        return _typeBuilder.CreateTypeInfo()!;
    }

    public Func<CallInvoker, object, IClientCallInvoker, TContract> CreateFactory<TContract>(Type implementationType)
    {
        var callInvoker = Expression.Parameter(typeof(CallInvoker), "callInvoker");
        var contract = Expression.Parameter(typeof(object), "contract");
        var clientCallInvoker = Expression.Parameter(typeof(IClientCallInvoker), "clientCallInvoker");

        var ctor = implementationType.Constructor(callInvoker.Type, _contractType, clientCallInvoker.Type);
        var factory = Expression.New(
            ctor,
            callInvoker,
            Expression.Convert(contract, _contractType),
            clientCallInvoker);

        return Expression.Lambda<Func<CallInvoker, object, IClientCallInvoker, TContract>>(factory, callInvoker, contract, clientCallInvoker).Compile();
    }

    private void ImplementNotSupportedMethod(MethodInfo method, string error)
    {
        var body = CreateMethodWithSignature(method);

        // throw new NotSupportedException("...");
        body.Emit(OpCodes.Ldstr, error);
        body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);
    }

    private void ImplementMethod(OperationDescription<Type> operation, string? grpcMethodName)
    {
        var body = CreateMethodWithSignature(operation.GetSource());

        switch (operation.OperationType)
        {
            case MethodType.Unary:
                BuildUnary(body, operation, grpcMethodName);
                break;
            case MethodType.ClientStreaming:
                BuildClientStreaming(body, operation);
                break;
            case MethodType.ServerStreaming:
                BuildServerStreaming(body, operation);
                break;
            case MethodType.DuplexStreaming:
                BuildDuplexStreaming(body, operation);
                break;
            default:
                throw new NotImplementedException($"{operation.OperationType} operation is not implemented.");
        }
    }

    private void BuildUnary(ILGenerator body, OperationDescription<Type> operation, string? grpcMethodName)
    {
        // optionsBuilder
        InitializeCallOptionsBuilder(body, operation);

        // _clientCallInvoker.Unary(
        var invokeMethod = _reflectClientCallInvoker.Unary(operation.RequestType, operation.ResponseType, operation.IsAsync);

        InitializeCall(body, operation, grpcMethodName);

        PushMessage(body, operation.RequestType.GetClrType(), operation.RequestTypeInput); // message

        body.Emit(OpCodes.Callvirt, invokeMethod);

        // Task => new ValueTask
        if (operation.Method.ReturnType.IsValueTask())
        {
            body.Emit(OpCodes.Newobj, operation.Method.ReturnType.Constructor(invokeMethod.ReturnType));
        }

        body.Emit(OpCodes.Ret);
    }

    private void BuildServerStreaming(ILGenerator body, OperationDescription<Type> operation)
    {
        // optionsBuilder
        InitializeCallOptionsBuilder(body, operation);

        // _clientCallInvoker.Server(
        var invokeMethod = _reflectClientCallInvoker.Server(
            operation.RequestType,
            operation.HeaderResponseType,
            operation.ResponseType,
            operation.Method.ReturnType,
            operation.IsAsync);
        InitializeCall(body, operation, null);

        PushMessage(body, operation.RequestType.GetClrType(), operation.RequestTypeInput); // message

        DoServerStreamingCall(body, operation, invokeMethod);

        body.Emit(OpCodes.Ret);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(IAsyncEnumerable<>))]
    private (MethodInfo Method, Type DelegateType) BuildServerStreamingResultAdapter(OperationDescription<Type> operation)
    {
        var parameterHeaderType = operation.HeaderResponseType!.GetClrType();
        var parameterStreamType = typeof(IAsyncEnumerable<>).MakeConstructedGeneric(operation.ResponseType.Properties[0]);

        var returnType = operation.Method.ReturnType.GetGenericArguments()[0];

        var method = _typeBuilder
            .DefineMethod(
                GetUniqueMemberName("Adapt" + operation.Method.Name + "Response"),
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                returnType,
                [parameterHeaderType, parameterStreamType]);

        var body = method.GetILGenerator();

        var propertiesCount = operation.HeaderResponseTypeInput.Length + 1;
        for (var i = 0; i < propertiesCount; i++)
        {
            if (i == operation.ResponseTypeIndex)
            {
                body.Emit(OpCodes.Ldarg_1);
            }
            else
            {
                var index = Array.IndexOf(operation.HeaderResponseTypeInput, i) + 1;
                var propertyName = "Value" + index.ToString(CultureInfo.InvariantCulture);

                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Callvirt, parameterHeaderType.InstanceProperty(propertyName).SafeGetGetMethod());
            }
        }

        body.Emit(OpCodes.Newobj, returnType.Constructor(propertiesCount));
        body.Emit(OpCodes.Ret);

        var delegateType = typeof(Func<,,>).MakeConstructedGeneric(parameterHeaderType, parameterStreamType, returnType);
        return (method, delegateType);
    }

    private void BuildClientStreaming(ILGenerator body, OperationDescription<Type> operation)
    {
        // optionsBuilder
        InitializeCallOptionsBuilder(body, operation);

        // _clientCallInvoker.Server(
        var invokeMethod = _reflectClientCallInvoker.Client(operation.HeaderRequestType, operation.RequestType, operation.ResponseType);
        InitializeCall(body, operation, null);

        if (operation.HeaderRequestType == null)
        {
            body.Emit(OpCodes.Ldnull);
        }
        else
        {
            PushMessage(body, operation.HeaderRequestType.GetClrType(), operation.HeaderRequestTypeInput);
        }

        // stream
        body.EmitLdarg(operation.RequestTypeInput[0] + 1);

        body.Emit(OpCodes.Callvirt, invokeMethod);

        if (operation.Method.ReturnType.IsValueTask())
        {
            // new ValueTask<>()
            body.Emit(OpCodes.Newobj, operation.Method.ReturnType.Constructor(invokeMethod.ReturnType));
        }

        body.Emit(OpCodes.Ret);
    }

    private void BuildDuplexStreaming(ILGenerator body, OperationDescription<Type> operation)
    {
        // optionsBuilder
        InitializeCallOptionsBuilder(body, operation);

        // _clientCallInvoker.Duplex(
        var invokeMethod = _reflectClientCallInvoker.Duplex(
            operation.HeaderRequestType,
            operation.RequestType,
            operation.HeaderResponseType,
            operation.ResponseType,
            operation.Method.ReturnType,
            operation.IsAsync);
        InitializeCall(body, operation, null);

        if (operation.HeaderRequestType == null)
        {
            body.Emit(OpCodes.Ldnull);
        }
        else
        {
            PushMessage(body, operation.HeaderRequestType.GetClrType(), operation.HeaderRequestTypeInput);
        }

        // stream
        body.EmitLdarg(operation.RequestTypeInput[0] + 1);

        DoServerStreamingCall(body, operation, invokeMethod);

        body.Emit(OpCodes.Ret);
    }

    private void InitializeCallOptionsBuilder(ILGenerator body, OperationDescription<Type> operation)
    {
        // var optionsBuilder = _clientCallInvoker.CreateOptionsBuilder();
        body.DeclareLocal(typeof(CallOptionsBuilder));
        body.Emit(OpCodes.Ldarg_0); // this
        body.Emit(OpCodes.Ldfld, _clientCallInvokerField); // _clientCallInvoker
        body.Emit(OpCodes.Callvirt, _reflectClientCallInvoker.CreateOptionsBuilder()); // .CreateOptionsBuilder()
        body.Emit(OpCodes.Stloc_0);

        // optionsBuilder = optionsBuilder.With()
        foreach (var i in operation.ContextInput)
        {
            body.Emit(OpCodes.Ldloca_S, 0); // optionsBuilder
            body.EmitLdarg(i + 1); // parameter

            var parameterType = operation.Method.Parameters[i].Type;

            Type? nullable = null;
            if (parameterType.IsValueType)
            {
                nullable = Nullable.GetUnderlyingType(parameterType);
                if (nullable == null)
                {
                    // CancellationToken => CancellationToken?
                    body.Emit(OpCodes.Newobj, typeof(Nullable<>).MakeConstructedGeneric(parameterType).Constructor(parameterType));
                }
            }

            var withMethodName = "With" + (nullable?.Name ?? parameterType.Name);
            body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(withMethodName)); // .With
            body.Emit(OpCodes.Stloc_0);
        }
    }

    private void InitializeCall(ILGenerator body, OperationDescription<Type> operation, string? grpcMethodName)
    {
        // _clientCallInvoker.method(_callInvoker, _contract.UnaryCall, in optionsBuilder)
        body.Emit(OpCodes.Ldarg_0); // _clientCallInvoker
        body.Emit(OpCodes.Ldfld, _clientCallInvokerField);

        body.Emit(OpCodes.Ldarg_0); // CallInvoker
        body.Emit(OpCodes.Ldfld, _callInvokerField);

        PushContractField(body, grpcMethodName ?? NamingContract.Contract.GrpcMethod(operation.OperationName)); // method

        body.Emit(OpCodes.Ldloc_0); // optionsBuilder
    }

    private void DoServerStreamingCall(ILGenerator body, OperationDescription<Type> operation, MethodInfo invokeMethod)
    {
        if (operation.HeaderResponseType == null && operation.IsAsync)
        {
            body.Emit(OpCodes.Callvirt, invokeMethod);
            if (operation.Method.ReturnType.IsValueTask())
            {
                // new ValueTask<>()
                body.Emit(OpCodes.Newobj, operation.Method.ReturnType.Constructor(invokeMethod.ReturnType));
            }
        }
        else if (operation.HeaderResponseType == null)
        {
            body.Emit(OpCodes.Callvirt, invokeMethod);
        }
        else
        {
            var (adapter, adapterType) = BuildServerStreamingResultAdapter(operation);

            // new Func<,>(Adapter)
            body.Emit(OpCodes.Ldnull);
            body.Emit(OpCodes.Ldftn, adapter);
            body.Emit(OpCodes.Newobj, adapterType.Constructor(typeof(object), typeof(IntPtr)));

            body.Emit(OpCodes.Callvirt, invokeMethod);

            if (operation.Method.ReturnType.IsValueTask())
            {
                // new ValueTask<>()
                body.Emit(OpCodes.Newobj, operation.Method.ReturnType.Constructor(invokeMethod.ReturnType));
            }
        }
    }

    private void PushContractField(ILGenerator body, string fieldName)
    {
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, _contractField);
        body.Emit(OpCodes.Ldfld, _contractType.InstanceFiled(fieldName));
    }

    private void PushMessage(ILGenerator body, Type messageType, int[] messageInput)
    {
        // new Message<string, string>(value12, value3);
        foreach (var i in messageInput)
        {
            body.EmitLdarg(i + 1);
        }

        body.Emit(OpCodes.Newobj, messageType.Constructor(messageType.GenericTypeArguments));
    }

    private ILGenerator CreateMethodWithSignature(MethodInfo signature)
    {
        var parameterTypes = signature.GetParameters().Select(i => i.ParameterType).ToArray();
        var method = _typeBuilder
            .DefineMethod(
                signature.Name,
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                signature.ReturnType,
                parameterTypes);

        if (signature.IsGenericMethod)
        {
            var genericParameters = signature.GetGenericArguments().Select(i => i.Name).ToArray();
            method.DefineGenericParameters(genericParameters);
        }

        // explicit interface implementation
        _typeBuilder.DefineMethodOverride(method, signature);

        return method.GetILGenerator();
    }

    private void BuildCtor()
    {
        var ctor = _typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.HasThis,
            [typeof(CallInvoker), _contractType, typeof(IClientCallInvoker)]);

        var il = ctor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).Constructor());

        // __callInvoker
        il.Emit(OpCodes.Ldarg_0);
        il.EmitLdarg(1);
        il.Emit(OpCodes.Stfld, _callInvokerField);

        // __contract
        il.Emit(OpCodes.Ldarg_0);
        il.EmitLdarg(2);
        il.Emit(OpCodes.Stfld, _contractField);

        // __clientCallInvoker
        il.Emit(OpCodes.Ldarg_0);
        il.EmitLdarg(3);
        il.Emit(OpCodes.Stfld, _clientCallInvokerField);

        il.Emit(OpCodes.Ret);
    }

    private string GetUniqueMemberName(string suggestedName)
    {
        var index = 1;
        var result = suggestedName;

        while (!_uniqueMemberNames.Add(result))
        {
            result = suggestedName + index.ToString(CultureInfo.InvariantCulture);
            index++;
        }

        return result;
    }
}