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

using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ServiceMethodFilterRegistration
{
    private readonly IServiceProvider _serviceProvider;
    private List<FilterRegistration<IServerFilter>>? _registrations;

    public ServiceMethodFilterRegistration(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Add(IList<FilterRegistration<IServerFilter>>? registrations)
    {
        if (registrations == null || registrations.Count == 0)
        {
            return;
        }

        if (_registrations == null)
        {
            _registrations = new List<FilterRegistration<IServerFilter>>();
        }

        _registrations.AddRange(registrations);
    }

    public ServerCallFilterHandlerFactory? CreateHandlerFactory(IList<object> metadata, Func<IOperationDescriptor> getDescriptor)
    {
        var registrations = CombineRegistrations(metadata);
        if (registrations == null)
        {
            return null;
        }

        registrations.Sort();

        var filterFactories = new Func<IServiceProvider, IServerFilter>[registrations.Count];
        for (var i = 0; i < registrations.Count; i++)
        {
            filterFactories[i] = registrations[i].Factory;
        }

        return new ServerCallFilterHandlerFactory(_serviceProvider, getDescriptor(), filterFactories);
    }

    private List<FilterRegistration<IServerFilter>>? CombineRegistrations(IList<object> metadata)
    {
        if (_registrations == null && metadata.Count == 0)
        {
            return null;
        }

        List<FilterRegistration<IServerFilter>>? result = null;
        for (var i = 0; i < metadata.Count; i++)
        {
            FilterRegistration<IServerFilter>? registration = null;

            if (metadata[i] is ServerFilterAttribute filterAttribute)
            {
                registration = new FilterRegistration<IServerFilter>(filterAttribute.Order, _ => filterAttribute);
            }
            else if (metadata[i] is ServerFilterRegistrationAttribute registrationAttribute)
            {
                registration = new FilterRegistration<IServerFilter>(registrationAttribute.Order, registrationAttribute.CreateFilter);
            }

            if (registration != null)
            {
                if (result == null)
                {
                    result = new List<FilterRegistration<IServerFilter>>((_registrations?.Count ?? 0) + 4);
                    if (_registrations != null)
                    {
                        result.AddRange(_registrations);
                    }
                }

                result.Add(registration);
            }
        }

        return result ?? _registrations;
    }
}