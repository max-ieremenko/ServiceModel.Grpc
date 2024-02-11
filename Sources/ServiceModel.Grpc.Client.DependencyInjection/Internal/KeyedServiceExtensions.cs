// <copyright>
// Copyright 2024 Max Ieremenko
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal static class KeyedServiceExtensions
{
    public static string GetOptionsKey(object? serviceKey)
    {
        var result = serviceKey?.ToString();
        return result ?? Options.DefaultName;
    }

    public static T ResolveOptions<T>(IServiceProvider provider, string key)
        where T : class, new()
    {
        // do not use IOptionsSnapshot: it is scoped service
        var snapshot = provider.GetService<IOptionsMonitor<T>>();
        var options = snapshot?.Get(key) ?? new T();
        return options;
    }

    public static T Resolve<T>(this IServiceProvider provider, object? serviceKey)
        where T : notnull
    {
        if (serviceKey == null)
        {
            // InvalidOperationException(SR.KeyedServicesNotSupported);
            return provider.GetRequiredService<T>();
        }

        return provider.GetRequiredKeyedService<T>(serviceKey);
    }

    public static T? TryResolve<T>(this IServiceProvider provider, object? serviceKey)
    {
        if (serviceKey == null)
        {
            // InvalidOperationException(SR.KeyedServicesNotSupported);
            return provider.GetService<T>();
        }

        return provider.GetKeyedService<T>(serviceKey);
    }
}