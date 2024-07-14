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
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ServerCallFilterHandlerFactory
{
    public ServerCallFilterHandlerFactory(
        IServiceProvider serviceProvider,
        IOperationDescriptor operation,
        Func<IServiceProvider, IServerFilter>[] filterFactories)
    {
        ServiceProvider = serviceProvider;
        Operation = operation;
        FilterFactories = filterFactories;
    }

    public IServiceProvider ServiceProvider { get; }

    public IOperationDescriptor Operation { get; }

    public Func<IServiceProvider, IServerFilter>[] FilterFactories { get; }

    public ServerCallFilterHandler CreateHandler(object service, ServerCallContext context)
    {
        var filters = new IServerFilter[FilterFactories.Length];
        for (var i = 0; i < FilterFactories.Length; i++)
        {
            filters[i] = CreateFilter(FilterFactories[i]);
        }

        var filterContext = new ServerFilterContext(
            service,
            context,
            ServiceProvider,
            Operation,
            new RequestContext(Operation.GetRequestAccessor(), Operation.GetRequestStreamAccessor()),
            new ResponseContext(Operation.GetResponseAccessor(), Operation.GetResponseStreamAccessor()));

        return new ServerCallFilterHandler(filterContext, filters);
    }

    private IServerFilter CreateFilter(Func<IServiceProvider, IServerFilter> factory)
    {
        IServerFilter? filter;
        try
        {
            filter = factory(ServiceProvider);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Fail to create a server filter: {ex.Message}. Please check server filter registrations.", ex);
        }

        if (filter == null)
        {
            throw new InvalidOperationException("Server filter factory must not return null. Please check server filter registrations.");
        }

        return filter;
    }
}