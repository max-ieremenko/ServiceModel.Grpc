// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Hosting.Internal;

namespace ServiceModel.Grpc.Internal.Emit;

internal sealed class EmitServiceEndpointBinder<TService> : IServiceEndpointBinder<TService>
{
    private readonly ContractDescription _description;
    private readonly Type? _serviceInstanceType;
    private readonly Type _contractType;
    private readonly Type _channelType;
    private readonly ILogger? _logger;

    private readonly MethodInfo _serviceBinderAddUnaryMethod;
    private readonly MethodInfo _serviceBinderAddClientStreamingMethod;
    private readonly MethodInfo _serviceBinderAddServerStreamingMethod;
    private readonly MethodInfo _serviceBinderAddDuplexStreamingMethod;

    public EmitServiceEndpointBinder(
        ContractDescription description,
        Type? serviceInstanceType,
        Type contractType,
        Type channelType,
        ILogger? logger)
    {
        _description = description;
        _serviceInstanceType = serviceInstanceType;
        _contractType = contractType;
        _channelType = channelType;
        _logger = logger;

        var type = GetType();
        _serviceBinderAddUnaryMethod = type.StaticMethod(nameof(AddUnaryMethod));
        _serviceBinderAddClientStreamingMethod = type.StaticMethod(nameof(AddClientStreamingMethod));
        _serviceBinderAddServerStreamingMethod = type.StaticMethod(nameof(AddServerStreamingMethod));
        _serviceBinderAddDuplexStreamingMethod = type.StaticMethod(nameof(AddDuplexStreamingMethod));
    }

    public void Bind(IServiceMethodBinder<TService> binder)
    {
        var contract = EmitContractBuilder.CreateFactory(_contractType)(binder.MarshallerFactory);
        var channelInstance = EmitServiceEndpointBuilder.CreateFactory(_channelType)();
        var serviceType = typeof(TService);

        foreach (var interfaceDescription in _description.Services)
        {
            foreach (var operation in interfaceDescription.Operations)
            {
                var message = operation.Message;
                var channelMethod = _channelType.InstanceMethod(operation.OperationName);
                var metadata = TryGetMethodMetadata(interfaceDescription.InterfaceType, message.Operation);
                var grpcMethodMethod = (IMethod)_contractType.InstanceFiled(operation.GrpcMethodName).GetValue(contract);

                _logger?.LogDebug("Bind service method {0}.{1}.", serviceType.FullName, message.Operation.Name);
                if (grpcMethodMethod.Type == MethodType.Unary)
                {
                    var addMethod = _serviceBinderAddUnaryMethod
                        .MakeGenericMethod(message.RequestType, message.ResponseType)
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, MethodInfo, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, message.Operation, metadata, channelMethod, channelInstance);
                }
                else if (grpcMethodMethod.Type == MethodType.ClientStreaming)
                {
                    var addMethod = _serviceBinderAddClientStreamingMethod
                        .MakeGenericMethod(
                            message.HeaderRequestType ?? typeof(Message),
                            message.RequestType,
                            message.RequestType.GenericTypeArguments[0],
                            message.ResponseType)
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, MethodInfo, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, message.Operation, metadata, channelMethod, channelInstance);
                }
                else if (grpcMethodMethod.Type == MethodType.ServerStreaming)
                {
                    var addMethod = _serviceBinderAddServerStreamingMethod
                        .MakeGenericMethod(
                            message.RequestType,
                            message.HeaderResponseType ?? typeof(Message),
                            message.ResponseType,
                            message.ResponseType.GenericTypeArguments[0])
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, MethodInfo, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, message.Operation, metadata, channelMethod, channelInstance);
                }
                else if (grpcMethodMethod.Type == MethodType.DuplexStreaming)
                {
                    var addMethod = _serviceBinderAddDuplexStreamingMethod
                        .MakeGenericMethod(
                            message.HeaderRequestType ?? typeof(Message),
                            message.RequestType,
                            message.RequestType.GenericTypeArguments[0],
                            message.HeaderResponseType ?? typeof(Message),
                            message.ResponseType,
                            message.ResponseType.GenericTypeArguments[0])
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, MethodInfo, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, message.Operation, metadata, channelMethod, channelInstance);
                }
                else
                {
                    throw new NotImplementedException("{0} operation is not implemented.".FormatWith(grpcMethodMethod.Type));
                }
            }
        }
    }

    private static void AddUnaryMethod<TRequest, TResponse>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        MethodInfo contractMethodDefinition,
        IList<object> metadata,
        MethodInfo channelMethod,
        object channelInstance)
        where TRequest : class
        where TResponse : class
    {
        var method = (Method<TRequest, TResponse>)grpcMethod;
        var handler = channelMethod.CreateDelegate<Func<TService, TRequest, ServerCallContext, Task<TResponse>>>(channelInstance);
        binder.AddUnaryMethod(
            method,
            () => contractMethodDefinition,
            metadata,
            handler);
    }

    private static void AddClientStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponse>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        MethodInfo contractMethodDefinition,
        IList<object> metadata,
        MethodInfo channelMethod,
        object channelInstance)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponse : class
    {
        var handler = channelMethod.CreateDelegate<Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, Task<TResponse>>>(channelInstance);
        binder.AddClientStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponse>(
            grpcMethod,
            () => contractMethodDefinition,
            metadata,
            handler);
    }

    private static void AddServerStreamingMethod<TRequest, TResponseHeader, TResponse, TResponseValue>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        MethodInfo contractMethodDefinition,
        IList<object> metadata,
        MethodInfo channelMethod,
        object channelInstance)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new()
    {
        var handler = channelMethod.CreateDelegate<Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>>>(channelInstance);
        binder.AddServerStreamingMethod<TRequest, TResponseHeader, TResponse, TResponseValue>(
            grpcMethod,
            () => contractMethodDefinition,
            metadata,
            handler);
    }

    private static void AddDuplexStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        MethodInfo contractMethodDefinition,
        IList<object> metadata,
        MethodInfo channelMethod,
        object channelInstance)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new()
    {
        var handler = channelMethod.CreateDelegate<Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>>>(channelInstance);
        binder.AddDuplexStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
            grpcMethod,
            () => contractMethodDefinition,
            metadata,
            handler);
    }

    private static IList<object> GetMethodMetadata(Type serviceInstanceType, MethodInfo serviceInstanceMethod)
    {
        // https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs
        var metadata = new List<object>();

        // Add type metadata first so it has a lower priority
        metadata.AddRange(serviceInstanceType.GetCustomAttributes(inherit: true));

        // Add method metadata last so it has a higher priority
        metadata.AddRange(serviceInstanceMethod.GetCustomAttributes(inherit: true));

        return metadata;
    }

    private IList<object> TryGetMethodMetadata(Type interfaceType, MethodInfo operation)
    {
        if (_serviceInstanceType == null)
        {
            return Array.Empty<object>();
        }

        var serviceInstanceMethod = ReflectionTools.ImplementationOfMethod(_serviceInstanceType, interfaceType, operation);
        return GetMethodMetadata(_serviceInstanceType, serviceInstanceMethod);
    }
}