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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class EmitServiceEndpointBuilder
{
    private readonly ContractDescription<Type> _description;
    private readonly HashSet<string> _uniqueMemberNames;

    private TypeBuilder _typeBuilder = null!;
    private ILGenerator _ctor = null!;

    public EmitServiceEndpointBuilder(ContractDescription<Type> description)
    {
        _description = description;
        _uniqueMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public static Func<object> CreateFactory(Type implementationType)
    {
        var ctor = implementationType.Constructor();
        var factory = Expression.New(ctor);

        return Expression.Lambda<Func<object>>(factory).Compile();
    }

    public TypeInfo Build(ModuleBuilder moduleBuilder, ILogger? logger = default, string? className = default)
    {
        _typeBuilder = moduleBuilder.DefineType(
            className ?? NamingContract.Endpoint.Class(_description.BaseClassName),
            TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        _ctor = _typeBuilder
            .DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.HasThis, null)
            .GetILGenerator();
        _ctor.Emit(OpCodes.Ldarg_0);
        _ctor.Emit(OpCodes.Call, typeof(object).Constructor());

        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                if (IsSupportedContextInput(operation))
                {
                    BuildOperation(operation, interfaceDescription.InterfaceType);
                }
                else
                {
                    var error = $"Context options in [{ReflectionTools.GetSignature(operation.GetSource())}] are not supported.";
                    logger?.LogError("Service {0}: {1}", _description.ContractInterface.FullName, error);
                    BuildNotSupportedOperation(operation, interfaceDescription.InterfaceType, error);
                }
            }
        }

        _ctor.Emit(OpCodes.Ret);

        return _typeBuilder.CreateTypeInfo()!;
    }

    private static bool IsSupportedContextInput(OperationDescription<Type> operation)
    {
        for (var i = 0; i < operation.ContextInput.Length; i++)
        {
            var input = operation.ContextInput[i];
            if (!EmitServerChannelAdapter.TryGetServiceContextOptionMethod(operation.Method.Parameters[input].Type))
            {
                return false;
            }
        }

        return true;
    }

    private void BuildNotSupportedOperation(OperationDescription<Type> operation, Type serviceType, string error)
    {
        var body = CreateMethodWithSignature(operation, serviceType, operation.OperationName);

        // throw new NotSupportedException("...");
        body.Emit(OpCodes.Ldstr, error);
        body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
        body.Emit(OpCodes.Throw);
    }

    private void BuildOperation(OperationDescription<Type> operation, Type serviceType)
    {
        var body = CreateMethodWithSignature(operation, serviceType, operation.OperationName);

        switch (operation.OperationType)
        {
            case MethodType.Unary:
                BuildUnary(body, operation, serviceType);
                break;

            case MethodType.ClientStreaming:
                BuildClientStreaming(body, operation, serviceType);
                break;

            case MethodType.ServerStreaming:
                BuildServerStreaming(body, operation, serviceType);
                break;

            case MethodType.DuplexStreaming:
                BuildDuplexStreaming(body, operation, serviceType);
                break;
        }
    }

    private void BuildUnary(ILGenerator body, OperationDescription<Type> operation, Type serviceType)
    {
        // service
        body.Emit(OpCodes.Ldarg_1);

        for (var i = 0; i < operation.Method.Parameters.Length; i++)
        {
            var parameter = operation.Method.Parameters[i];
            if (operation.ContextInput.Contains(i))
            {
                PushContext(body, 3, parameter.Type);
            }
            else
            {
                var propertyName = "Value" + (Array.IndexOf(operation.RequestTypeInput, i) + 1);

                // request.Value1
                body.Emit(OpCodes.Ldarg_2);
                body.Emit(OpCodes.Callvirt, operation.RequestType.GetClrType().InstanceProperty(propertyName).GetMethod);
            }
        }

        // service.Method
        CallContractMethod(body, operation, serviceType);

        if (operation.IsAsync)
        {
            AdaptSyncUnaryCallResult(body, operation);
        }
        else
        {
            if (operation.ResponseType.IsGenericType())
            {
                // new Message<T>
                body.Emit(OpCodes.Newobj, operation.ResponseType.GetClrType().Constructor(operation.ResponseType.Properties));
            }
            else
            {
                // new Message
                body.Emit(OpCodes.Newobj, operation.ResponseType.GetClrType().Constructor());
            }

            // Task.FromResult
            body.Emit(OpCodes.Call, typeof(Task).StaticMethod(nameof(Task.FromResult)).MakeGenericMethod(operation.ResponseType.GetClrType()));
        }

        body.Emit(OpCodes.Ret);
    }

    private void BuildClientStreaming(ILGenerator body, OperationDescription<Type> operation, Type serviceType)
    {
        // service
        body.Emit(OpCodes.Ldarg_1);

        for (var i = 0; i < operation.Method.Parameters.Length; i++)
        {
            var parameter = operation.Method.Parameters[i];
            if (operation.ContextInput.Contains(i))
            {
                PushContext(body, 4, parameter.Type);
            }
            else if (operation.HeaderRequestTypeInput.Contains(i))
            {
                PushHeaderProperty(body, operation, i);
            }
            else
            {
                body.Emit(OpCodes.Ldarg_3); // stream
            }
        }

        // service.Method
        CallContractMethod(body, operation, serviceType);

        AdaptSyncUnaryCallResult(body, operation);

        body.Emit(OpCodes.Ret);
    }

    private void BuildServerStreaming(ILGenerator body, OperationDescription<Type> operation, Type serviceType)
    {
        // service
        body.Emit(OpCodes.Ldarg_1);

        for (var i = 0; i < operation.Method.Parameters.Length; i++)
        {
            var parameter = operation.Method.Parameters[i];
            if (operation.ContextInput.Contains(i))
            {
                PushContext(body, 3, parameter.Type);
            }
            else
            {
                var propertyName = "Value" + (Array.IndexOf(operation.RequestTypeInput, i) + 1);

                // request.Value1
                body.Emit(OpCodes.Ldarg_2);
                body.Emit(OpCodes.Callvirt, operation.RequestType.GetClrType().InstanceProperty(propertyName).GetMethod);
            }
        }

        // service.Method
        CallContractMethod(body, operation, serviceType);

        BuildWriteServerStreamingResult(body, operation);

        body.Emit(OpCodes.Ret);
    }

    private FieldBuilder BuildServerStreamingResultAdapter(OperationDescription<Type> operation)
    {
        // private static (Message<string, int>, IAsyncEnumerable<int>) AdaptHeaderTask((string, IAsyncEnumerable<int>, int) result)
        var parameterType = operation.Method.ReturnType.GetGenericArguments()[0];
        var returnType = typeof(ValueTuple<,>).MakeGenericType(
            operation.HeaderResponseType.GetClrType(),
            typeof(IAsyncEnumerable<>).MakeGenericType(operation.ResponseType.Properties[0]));

        var method = _typeBuilder
            .DefineMethod(
                GetUniqueMemberName("__Adapt" + operation.Method.Name + "Response"),
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                returnType,
                [parameterType]);

        var body = method.GetILGenerator();

        // return (new Message<string, int>(result.Item1, result.Item3), result.Item2);
        var headerPropertiesCount = operation.HeaderResponseTypeInput.Length;
        for (var i = 0; i < headerPropertiesCount; i++)
        {
            body.Emit(OpCodes.Ldarg_0);

            var index = operation.HeaderResponseTypeInput[i] + 1;
            var fieldName = "Item" + index.ToString(CultureInfo.InvariantCulture);

            body.Emit(OpCodes.Ldfld, parameterType.InstanceFiled(fieldName));
        }

        // new Message<string, int>()
        body.Emit(OpCodes.Newobj, operation.HeaderResponseType.GetClrType().Constructor(headerPropertiesCount));

        // push stream
        var streamFieldName = "Item" + (operation.ResponseTypeIndex + 1).ToString(CultureInfo.InvariantCulture);
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldfld, parameterType.InstanceFiled(streamFieldName));

        body.Emit(OpCodes.Newobj, returnType!.Constructor(2));
        body.Emit(OpCodes.Ret);

        var delegateType = typeof(Func<,>).MakeGenericType(parameterType, returnType);

        // private readonly Func<(string, IAsyncEnumerable<int>, int), (Message<string, int>, IAsyncEnumerable<int>)> _adaptHeaderTask = AdaptHeaderTask;
        var field = _typeBuilder
            .DefineField(
                GetUniqueMemberName("__" + operation.Method.Name + "ResponseAdapter"),
                delegateType,
                FieldAttributes.Private | FieldAttributes.InitOnly);

        // _adaptHeaderTask = AdaptHeaderTask
        _ctor.Emit(OpCodes.Ldarg_0);
        _ctor.Emit(OpCodes.Ldnull);
        _ctor.Emit(OpCodes.Ldftn, method);
        _ctor.Emit(OpCodes.Newobj, delegateType.Constructor(typeof(object), typeof(IntPtr)));
        _ctor.Emit(OpCodes.Stfld, field);

        return field;
    }

    private void BuildDuplexStreaming(ILGenerator body, OperationDescription<Type> operation, Type serviceType)
    {
        body.Emit(OpCodes.Ldarg_1); // service

        for (var i = 0; i < operation.Method.Parameters.Length; i++)
        {
            var parameter = operation.Method.Parameters[i];
            if (operation.ContextInput.Contains(i))
            {
                PushContext(body, 4, parameter.Type);
            }
            else if (operation.HeaderRequestTypeInput.Contains(i))
            {
                PushHeaderProperty(body, operation, i);
            }
            else
            {
                body.Emit(OpCodes.Ldarg_3); // request
            }
        }

        // service.Method
        CallContractMethod(body, operation, serviceType);

        BuildWriteServerStreamingResult(body, operation);

        body.Emit(OpCodes.Ret);
    }

    private ILGenerator CreateMethodWithSignature(OperationDescription<Type> operation, Type serviceType, string methodName)
    {
        switch (operation.OperationType)
        {
            case MethodType.Unary:
                // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
                return _typeBuilder
                    .DefineMethod(
                        methodName,
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                        typeof(Task<>).MakeGenericType(operation.ResponseType.GetClrType()),
                        [serviceType, operation.RequestType.GetClrType(), typeof(ServerCallContext)])
                    .GetILGenerator();

            case MethodType.ClientStreaming:
                // Task<TResponse> Invoke(TService service, Message<TRequestHeader>? requestHeader, IAsyncEnumerable<TRequest> request, ServerCallContext context)
                return _typeBuilder
                    .DefineMethod(
                        methodName,
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                        typeof(Task<>).MakeGenericType(operation.ResponseType.GetClrType()),
                        new[]
                        {
                            serviceType,
                            operation.HeaderRequestType.GetClrType(),
                            typeof(IAsyncEnumerable<>).MakeGenericType(operation.RequestType.Properties[0]),
                            typeof(ServerCallContext)
                        })
                    .GetILGenerator();

            case MethodType.ServerStreaming:
            {
                var response = typeof(ValueTuple<,>).MakeGenericType(
                    operation.HeaderResponseType.GetClrType(),
                    typeof(IAsyncEnumerable<>).MakeGenericType(operation.ResponseType.Properties[0]));

                // ValueTask<(TResponseHeader, IAsyncEnumerable<TResponse>)> Invoke(TService service, TRequest request, ServerCallContext context)
                return _typeBuilder
                    .DefineMethod(
                        methodName,
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                        typeof(ValueTask<>).MakeGenericType(response),
                        [serviceType, operation.RequestType.GetClrType(), typeof(ServerCallContext)])
                    .GetILGenerator();
            }

            case MethodType.DuplexStreaming:
            {
                var response = typeof(ValueTuple<,>).MakeGenericType(
                    operation.HeaderResponseType.GetClrType(),
                    typeof(IAsyncEnumerable<>).MakeGenericType(operation.ResponseType.Properties[0]));

                // ValueTask<(TResponseHeader, IAsyncEnumerable<TResponse>)> Invoke(TService service, TRequestHeader requestHeader, IAsyncEnumerable<TRequest> request, ServerCallContext context)
                return _typeBuilder
                    .DefineMethod(
                        methodName,
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                        typeof(ValueTask<>).MakeGenericType(response),
                        [
                            serviceType,
                            operation.HeaderRequestType.GetClrType(),
                            typeof(IAsyncEnumerable<>).MakeGenericType(operation.RequestType.Properties[0]),
                            typeof(ServerCallContext)
                        ])
                    .GetILGenerator();
            }
        }

        throw new NotImplementedException($"{operation.OperationType} operation is not implemented.");
    }

    private void AdaptSyncUnaryCallResult(ILGenerator body, OperationDescription<Type> message)
    {
        if (message.ResponseType.IsGenericType())
        {
            var adapter = typeof(EmitServerChannelAdapter)
                .StaticMethod(message.Method.ReturnType.IsValueTask() ? nameof(EmitServerChannelAdapter.GetUnaryCallResultValueTask) : nameof(EmitServerChannelAdapter.GetUnaryCallResultTask))
                .MakeGenericMethod(message.ResponseType.Properties[0]);

            // ServerChannelAdapter.GetUnaryCallResult
            body.Emit(OpCodes.Call, adapter);
        }
        else
        {
            var adapter = typeof(EmitServerChannelAdapter)
                .StaticMethod(message.Method.ReturnType.IsValueTask() ? nameof(EmitServerChannelAdapter.UnaryCallWaitValueTask) : nameof(EmitServerChannelAdapter.UnaryCallWaitTask));

            // ServerChannelAdapter.UnaryCallWait
            body.Emit(OpCodes.Call, adapter);
        }
    }

    private void PushContext(ILGenerator body, int serverContextParameterIndex, Type contextType)
    {
        // ServerChannelAdapter.GetContext(context)
        body.EmitLdarg(serverContextParameterIndex);
        body.Emit(OpCodes.Call, EmitServerChannelAdapter.GetServiceContextOptionMethod(contextType));
    }

    private void PushHeaderProperty(ILGenerator body, OperationDescription<Type> operation, int parameterIndex)
    {
        var propertyName = "Value" + (Array.IndexOf(operation.HeaderRequestTypeInput, parameterIndex) + 1);
        body.Emit(OpCodes.Ldarg_2); // requestHeader
        body.Emit(OpCodes.Callvirt, operation.HeaderRequestType.GetClrType().InstanceProperty(propertyName).GetMethod); // requestHeader
    }

    private void CallContractMethod(ILGenerator body, OperationDescription<Type> operation, Type serviceType)
    {
        var parameters = new Type[operation.Method.Parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            parameters[i] = operation.Method.Parameters[i].Type;
        }

        var method = serviceType.InstanceMethod(operation.Method.Name, parameters);
        body.Emit(OpCodes.Callvirt, method);
    }

    private void BuildWriteServerStreamingResult(ILGenerator body, OperationDescription<Type> operation)
    {
        if (operation.HeaderResponseType == null && !operation.IsAsync)
        {
            // return ServerChannelAdapter.ServerStreaming(service.Simple());
            var channelAdapter = typeof(EmitServerChannelAdapter)
                .StaticMethod(nameof(EmitServerChannelAdapter.ServerStreaming))
                .MakeGenericMethod(operation.ResponseType.Properties[0]);
            body.Emit(OpCodes.Call, channelAdapter);
        }
        else if (operation.HeaderResponseType == null && operation.IsAsync)
        {
            // return ServerChannelAdapter.ServerStreamingTask(service.SimpleTask());
            var channelAdapter = typeof(EmitServerChannelAdapter)
                .StaticMethod(operation.Method.ReturnType.IsValueTask() ? nameof(EmitServerChannelAdapter.ServerStreamingValueTask) : nameof(EmitServerChannelAdapter.ServerStreamingTask))
                .MakeGenericMethod(operation.ResponseType.Properties[0]);
            body.Emit(OpCodes.Call, channelAdapter);
        }
        else
        {
            // return ServerChannelAdapter.ServerStreamingHeaderTask(service.HeaderTask(), AdaptHeaderTask);
            var adapterField = BuildServerStreamingResultAdapter(operation);
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, adapterField);

            var channelAdapter = typeof(EmitServerChannelAdapter)
                .StaticMethod(operation.Method.ReturnType.IsValueTask() ? nameof(EmitServerChannelAdapter.ServerStreamingHeaderValueTask) : nameof(EmitServerChannelAdapter.ServerStreamingHeaderTask))
                .MakeGenericMethod(operation.Method.ReturnType.GetGenericArguments()[0], operation.HeaderResponseType.GetClrType(), operation.ResponseType.Properties[0]);
            body.Emit(OpCodes.Call, channelAdapter);
        }
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