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

using System.Reflection;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.Internal;

internal sealed class ClientMethodBinder : IClientMethodBinder
{
    private ClientMethodFilterRegistration? _filterRegistrations;

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

    public IMarshallerFactory MarshallerFactory { get; }

    public Func<CallOptions>? DefaultCallOptionsFactory { get; }

    public bool RequiresMetadata => _filterRegistrations != null;

    public void AddFilters(IList<FilterRegistration<IClientFilter>>? registrations)
    {
        if (registrations == null || registrations.Count == 0)
        {
            return;
        }

        if (_filterRegistrations == null)
        {
            _filterRegistrations = new ClientMethodFilterRegistration();
        }

        _filterRegistrations.Registrations.AddRange(registrations);
    }

    public void Add(IMethod method, Func<MethodInfo> resolveContractMethodDefinition) =>
        _filterRegistrations?.AddMethod(method, EmitGenerator.GenerateOperationDescriptor(resolveContractMethodDefinition));

    public IClientCallInvoker CreateCallInvoker() =>
        new ClientCallInvoker(DefaultCallOptionsFactory, _filterRegistrations?.CreateFactory(ServiceProvider));

    internal IList<FilterRegistration<IClientFilter>>? GetFilterRegistrations() => _filterRegistrations?.Registrations;
}