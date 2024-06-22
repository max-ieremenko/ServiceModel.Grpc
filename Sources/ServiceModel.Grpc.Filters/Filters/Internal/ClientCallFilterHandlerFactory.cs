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
using Grpc.Core;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ClientCallFilterHandlerFactory : IClientCallFilterHandlerFactory
{
    public ClientCallFilterHandlerFactory(
        IServiceProvider? serviceProvider,
        Dictionary<IMethod, ClientMethodMetadata> methodMetadataByGrpc)
    {
        ServiceProvider = serviceProvider;
        MethodMetadataByGrpc = methodMetadataByGrpc;
    }

    public IServiceProvider? ServiceProvider { get; }

    public Dictionary<IMethod, ClientMethodMetadata> MethodMetadataByGrpc { get; }

    public IClientCallFilterHandler? CreateAsyncHandler(IMethod method, CallInvoker callInvoker, CallOptions callOptions)
    {
        if (!MethodMetadataByGrpc.TryGetValue(method, out var metadata))
        {
            return null;
        }

        return CreateHandler(method, callInvoker, callOptions, metadata.Operation);
    }

    public IClientCallFilterHandler? CreateBlockingHandler(IMethod method, CallInvoker callInvoker, CallOptions callOptions)
    {
        if (!MethodMetadataByGrpc.TryGetValue(method, out var metadata))
        {
            return null;
        }

        return CreateHandler(method, callInvoker, callOptions, metadata.AlternateOperation ?? metadata.Operation);
    }

    private IClientCallFilterHandler CreateHandler(IMethod method, CallInvoker callInvoker, CallOptions callOptions, ClientMethodMetadata.Metadata metadata)
    {
        var context = new ClientFilterContext(
            ServiceProvider,
            callInvoker,
            callOptions,
            method,
            metadata.Operation,
            metadata.CreateRequestContext(),
            metadata.CreateResponseContext());

        var filters = new IClientFilter[metadata.FilterFactories.Length];
        for (var i = 0; i < filters.Length; i++)
        {
            filters[i] = CreateFilter(metadata.FilterFactories[i]);
        }

        return new ClientCallFilterHandler(context, filters);
    }

    private IClientFilter CreateFilter(Func<IServiceProvider, IClientFilter> factory)
    {
        IClientFilter? filter;
        try
        {
            filter = factory(ServiceProvider!);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create a client filter: {ex.Message}. Please check client filter registrations.", ex);
        }

        if (filter == null)
        {
            throw new InvalidOperationException("Client filter factory must not return null. Please check client filter registrations.");
        }

        return filter;
    }
}