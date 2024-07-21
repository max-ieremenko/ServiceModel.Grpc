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

using Grpc.Core;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ClientMethodFilterRegistration
{
    private readonly Dictionary<IMethod, ClientMethodMetadata> _methodMetadataByGrpc = new(GrpcMethodEqualityComparer.Instance);

    public List<FilterRegistration<IClientFilter>> Registrations { get; } = new();

    public void AddMethod(IMethod method, IOperationDescriptor descriptor)
    {
        if (!_methodMetadataByGrpc.TryGetValue(method, out var metadata))
        {
            metadata = new ClientMethodMetadata(descriptor, null);
            _methodMetadataByGrpc.Add(method, metadata);
            return;
        }

        if (method.Type != MethodType.Unary || metadata.AlternateOperation != null)
        {
            throw new InvalidOperationException($"A unary gRPC method [{method.FullName}] cannot have more than 2 definitions.");
        }

        _methodMetadataByGrpc[method] = CreateSyncOverAsync(metadata.Operation.Operation, descriptor);
    }

    public IClientCallFilterHandlerFactory CreateFactory(IServiceProvider? serviceProvider)
    {
        Registrations.Sort();

        var filterFactories = new Func<IServiceProvider, IClientFilter>[Registrations.Count];
        for (var i = 0; i < Registrations.Count; i++)
        {
            filterFactories[i] = Registrations[i].Factory;
        }

        foreach (var metadata in _methodMetadataByGrpc.Values)
        {
            metadata.Operation.FilterFactories = filterFactories;
            if (metadata.AlternateOperation != null)
            {
                metadata.AlternateOperation.FilterFactories = filterFactories;
            }
        }

        return new ClientCallFilterHandlerFactory(serviceProvider, _methodMetadataByGrpc);
    }

    private static ClientMethodMetadata CreateSyncOverAsync(IOperationDescriptor descriptor1, IOperationDescriptor descriptor2)
    {
        if (descriptor1.IsAsync())
        {
            return new ClientMethodMetadata(descriptor1, descriptor2);
        }

        return new ClientMethodMetadata(descriptor2, descriptor1);
    }
}