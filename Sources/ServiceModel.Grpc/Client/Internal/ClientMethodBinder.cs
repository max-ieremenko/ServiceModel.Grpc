// <copyright>
// Copyright 2023 Max Ieremenko
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
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.Internal;

internal sealed class ClientMethodBinder : IClientMethodBinder
{
    private List<FilterRegistration<IClientFilter>>? _filterRegistrations;
    private Dictionary<IMethod, ClientMethodMetadata>? _methodMetadataByGrpc;

    public ClientMethodBinder(
        IServiceProvider? serviceProvider,
        IMarshallerFactory marshallerFactory,
        Func<CallOptions>? defaultCallOptionsFactory)
    {
        ServiceProvider = serviceProvider;
        MarshallerFactory = marshallerFactory;
        DefaultCallOptionsFactory = defaultCallOptionsFactory;
    }

    public IServiceProvider? ServiceProvider { get; }

    public bool RequiresMetadata => _filterRegistrations != null;

    public IMarshallerFactory MarshallerFactory { get; }

    public Func<CallOptions>? DefaultCallOptionsFactory { get; }

    public void AddFilters(IList<FilterRegistration<IClientFilter>>? registrations)
    {
        if (registrations == null || registrations.Count == 0)
        {
            return;
        }

        if (_filterRegistrations == null)
        {
            _filterRegistrations = new List<FilterRegistration<IClientFilter>>();
        }

        _filterRegistrations.AddRange(registrations);
    }

    public void Add(IMethod method, Func<MethodInfo> resolveContractMethodDefinition)
    {
        if (_filterRegistrations == null)
        {
            return;
        }

        if (_methodMetadataByGrpc == null)
        {
            _methodMetadataByGrpc = new Dictionary<IMethod, ClientMethodMetadata>(GrpcMethodEqualityComparer.Instance);
        }

        if (!_methodMetadataByGrpc.TryGetValue(method, out var metadata))
        {
            metadata = new ClientMethodMetadata(resolveContractMethodDefinition, null);
            _methodMetadataByGrpc.Add(method, metadata);
            return;
        }

        if (method.Type != MethodType.Unary || metadata.AlternateMethod != null)
        {
            throw new InvalidOperationException("A unary gRPC method [{0}] cannot have more than 2 definitions.".FormatWith(method.FullName));
        }

        _methodMetadataByGrpc[method] = CreateSyncOverAsync(metadata.Method.ContractMethodDefinition, resolveContractMethodDefinition);
    }

    public IClientCallFilterHandlerFactory? CreateFilterHandlerFactory()
    {
        if (_filterRegistrations == null || _methodMetadataByGrpc == null)
        {
            return null;
        }

        _filterRegistrations.Sort();

        var filterFactories = new Func<IServiceProvider, IClientFilter>[_filterRegistrations.Count];
        for (var i = 0; i < _filterRegistrations.Count; i++)
        {
            filterFactories[i] = _filterRegistrations[i].Factory;
        }

        foreach (var metadata in _methodMetadataByGrpc.Values)
        {
            metadata.Method.FilterFactories = filterFactories;
            if (metadata.AlternateMethod != null)
            {
                metadata.AlternateMethod.FilterFactories = filterFactories;
            }
        }

        return new ClientCallFilterHandlerFactory(ServiceProvider, _methodMetadataByGrpc);
    }

    internal IList<FilterRegistration<IClientFilter>>? GetFilterRegistrations() => _filterRegistrations;

    private static ClientMethodMetadata CreateSyncOverAsync(Func<MethodInfo> method1, Func<MethodInfo> method2)
    {
        if (ReflectionTools.IsTask(method1().ReturnType))
        {
            return new ClientMethodMetadata(method1, method2);
        }

        return new ClientMethodMetadata(method2, method1);
    }
}