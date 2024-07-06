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
using System.Reflection;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding;

internal sealed class AspNetCoreServiceMethodBinder<TService> : IServiceMethodBinder<TService>
    where TService : class
{
    private readonly ServiceMethodProviderContext<TService> _context;
    private readonly ServiceMethodFilterRegistration _filterRegistration;
    private readonly bool _requiresGrpcMarker;

    public AspNetCoreServiceMethodBinder(
        ServiceMethodProviderContext<TService> context,
        IMarshallerFactory marshallerFactory,
        ServiceMethodFilterRegistration filterRegistration,
        bool requiresGrpcMarker)
    {
        _context = context;
        _filterRegistration = filterRegistration;
        _requiresGrpcMarker = requiresGrpcMarker;
        MarshallerFactory = marshallerFactory;
    }

    public IMarshallerFactory MarshallerFactory { get; }

    public void AddUnaryMethod<TRequest, TResponse>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler)
        where TRequest : class
        where TResponse : class
    {
        Func<IOperationDescriptor> getOperation = () => EmitGenerator.GenerateOperationDescriptor(resolveContractMethodDefinition);

        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getOperation);
        var invoker = new UnaryServerCallHandler<TService, TRequest, TResponse>(handler, filterHandlerFactory);

        if (_requiresGrpcMarker)
        {
            metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.Operation ?? getOperation());
        }

        _context.AddUnaryMethod((Method<TRequest, TResponse>)method, metadata, invoker.Handle);
    }

    public void AddClientStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponse>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, Task<TResponse>> handler)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponse : class
    {
        Func<IOperationDescriptor> getOperation = () => EmitGenerator.GenerateOperationDescriptor(resolveContractMethodDefinition);

        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getOperation);
        var invoker = new ClientStreamingServerCallHandler<TService, TRequestHeader, TRequest, TRequestValue, TResponse>(
            handler,
            method,
            filterHandlerFactory);

        if (_requiresGrpcMarker)
        {
            metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.Operation ?? getOperation());
        }

        _context.AddClientStreamingMethod((Method<TRequest, TResponse>)method, metadata, invoker.Handle);
    }

    public void AddServerStreamingMethod<TRequest, TResponseHeader, TResponse, TResponseValue>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> handler)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new()
    {
        Func<IOperationDescriptor> getOperation = () => EmitGenerator.GenerateOperationDescriptor(resolveContractMethodDefinition);

        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getOperation);
        var invoker = new ServerStreamingServerCallHandler<TService, TRequest, TResponseHeader, TResponse, TResponseValue>(
            handler,
            method,
            filterHandlerFactory);

        if (_requiresGrpcMarker)
        {
            metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.Operation ?? getOperation());
        }

        _context.AddServerStreamingMethod((Method<TRequest, TResponse>)method, metadata, invoker.Handle);
    }

    public void AddDuplexStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> handler)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new()
    {
        Func<IOperationDescriptor> getOperation = () => EmitGenerator.GenerateOperationDescriptor(resolveContractMethodDefinition);

        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getOperation);
        var invoker = new DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
            handler,
            method,
            filterHandlerFactory);

        if (_requiresGrpcMarker)
        {
            metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.Operation ?? getOperation());
        }

        _context.AddDuplexStreamingMethod((Method<TRequest, TResponse>)method, metadata, invoker.Handle);
    }

    private IList<object> AddServiceModelGrpcMarker(IList<object>? metadata, IOperationDescriptor descriptor)
    {
        var metadataLength = metadata?.Count ?? 0;
        var result = new object[metadataLength + 1];

        for (var i = 0; i < metadataLength; i++)
        {
            result[i] = metadata![i];
        }

        result[metadataLength] = new ServiceModelGrpcMarker(descriptor, MarshallerFactory);
        return result;
    }
}