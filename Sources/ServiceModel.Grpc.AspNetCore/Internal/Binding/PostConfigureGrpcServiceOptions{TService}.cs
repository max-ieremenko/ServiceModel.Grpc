// <copyright>
// Copyright 2020-201 Max Ieremenko
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

using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding
{
    internal sealed class PostConfigureGrpcServiceOptions<TService> : IPostConfigureOptions<GrpcServiceOptions<TService>>
        where TService : class
    {
        private readonly IOptions<ServiceModelGrpcServiceOptions> _rootConfiguration;
        private readonly IOptions<ServiceModelGrpcServiceOptions<TService>> _serviceConfiguration;

        public PostConfigureGrpcServiceOptions(
            IOptions<ServiceModelGrpcServiceOptions> rootConfiguration,
            IOptions<ServiceModelGrpcServiceOptions<TService>> serviceConfiguration)
        {
            rootConfiguration.AssertNotNull(nameof(rootConfiguration));
            serviceConfiguration.AssertNotNull(nameof(serviceConfiguration));

            _rootConfiguration = rootConfiguration;
            _serviceConfiguration = serviceConfiguration;
        }

        public void PostConfigure(string name, GrpcServiceOptions<TService> options)
        {
            var marshallerFactory = _serviceConfiguration.Value.MarshallerFactory ?? _rootConfiguration.Value.DefaultMarshallerFactory;

            PostConfigureGrpcServiceOptions.AddErrorHandler(
                options.Interceptors,
                _serviceConfiguration.Value.ErrorHandlerFactory,
                marshallerFactory);
        }
    }
}
