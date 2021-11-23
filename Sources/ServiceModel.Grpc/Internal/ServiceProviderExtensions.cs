// <copyright>
// Copyright 2021 Max Ieremenko
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

namespace ServiceModel.Grpc.Internal
{
    internal static class ServiceProviderExtensions
    {
        public static object GetServiceRequired(this IServiceProvider provider, Type serviceType)
        {
            provider.AssertNotNull(nameof(provider));
            serviceType.AssertNotNull(nameof(serviceType));

            var service = provider.GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException("No service for type '{0}' has been registered.".FormatWith(serviceType.FullName));
            }

            return service;
        }

        public static TService GetServiceRequired<TService>(this IServiceProvider provider)
        {
            return (TService)GetServiceRequired(provider, typeof(TService));
        }
    }
}
