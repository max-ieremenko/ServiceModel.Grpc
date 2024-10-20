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
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
internal sealed class EmitServiceEndpointBinder<TService> : IServiceEndpointBinder<TService>
{
    private readonly ContractDescription<Type> _description;
    private readonly Type? _serviceInstanceType;
    private readonly Type _contractType;
    private readonly Type _channelType;
    private readonly ILogger? _logger;

    private readonly MethodInfo _serviceBinderAddUnaryMethod;
    private readonly MethodInfo _serviceBinderAddClientStreamingMethod;
    private readonly MethodInfo _serviceBinderAddServerStreamingMethod;
    private readonly MethodInfo _serviceBinderAddDuplexStreamingMethod;

    public EmitServiceEndpointBinder(
        ContractDescription<Type> description,
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
                var channelMethod = _channelType.InstanceMethod(operation.OperationName);
                var metadata = TryGetMethodMetadata(interfaceDescription.InterfaceType, operation.GetSource());
                var grpcMethodMethod = (IMethod)_contractType.InstanceFiled(NamingContract.Contract.GrpcMethod(operation.OperationName)).GetValue(contract)!;
                var getDescriptor = _contractType.StaticMethod(NamingContract.Contract.DescriptorMethod(operation.OperationName)).CreateDelegate<Func<IOperationDescriptor>>();

                _logger?.LogDebug("Bind service method {0}.{1}.", serviceType.FullName, operation.Method.Name);
                if (grpcMethodMethod.Type == MethodType.Unary)
                {
                    var addMethod = _serviceBinderAddUnaryMethod
                        .MakeConstructedGeneric(operation.RequestType.GetClrType(), operation.ResponseType.GetClrType())
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, Func<IOperationDescriptor>, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, getDescriptor, metadata, channelMethod, channelInstance);
                }
                else if (grpcMethodMethod.Type == MethodType.ClientStreaming)
                {
                    var addMethod = _serviceBinderAddClientStreamingMethod
                        .MakeConstructedGeneric(
                            operation.HeaderRequestType.GetClrType(),
                            operation.RequestType.GetClrType(),
                            operation.RequestType.Properties[0],
                            operation.ResponseType.GetClrType())
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, Func<IOperationDescriptor>, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, getDescriptor, metadata, channelMethod, channelInstance);
                }
                else if (grpcMethodMethod.Type == MethodType.ServerStreaming)
                {
                    var addMethod = _serviceBinderAddServerStreamingMethod
                        .MakeConstructedGeneric(
                            operation.RequestType.GetClrType(),
                            operation.HeaderResponseType.GetClrType(),
                            operation.ResponseType.GetClrType(),
                            operation.ResponseType.Properties[0])
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, Func<IOperationDescriptor>, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, getDescriptor, metadata, channelMethod, channelInstance);
                }
                else if (grpcMethodMethod.Type == MethodType.DuplexStreaming)
                {
                    var addMethod = _serviceBinderAddDuplexStreamingMethod
                        .MakeConstructedGeneric(
                            operation.HeaderRequestType.GetClrType(),
                            operation.RequestType.GetClrType(),
                            operation.RequestType.Properties[0],
                            operation.HeaderResponseType.GetClrType(),
                            operation.ResponseType.GetClrType(),
                            operation.ResponseType.Properties[0])
                        .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, Func<IOperationDescriptor>, IList<object>, MethodInfo, object>>();
                    addMethod(binder, grpcMethodMethod, getDescriptor, metadata, channelMethod, channelInstance);
                }
                else
                {
                    throw new NotImplementedException($"{grpcMethodMethod.Type} operation is not implemented.");
                }
            }
        }
    }

    private static void AddUnaryMethod<TRequest, TResponse>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        Func<IOperationDescriptor> getDescriptor,
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
            getDescriptor,
            metadata,
            handler);
    }

    private static void AddClientStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponse>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        Func<IOperationDescriptor> getDescriptor,
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
            getDescriptor,
            metadata,
            handler);
    }

    private static void AddServerStreamingMethod<TRequest, TResponseHeader, TResponse, TResponseValue>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        Func<IOperationDescriptor> getDescriptor,
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
            getDescriptor,
            metadata,
            handler);
    }

    private static void AddDuplexStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        IServiceMethodBinder<TService> binder,
        IMethod grpcMethod,
        Func<IOperationDescriptor> getDescriptor,
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
            getDescriptor,
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