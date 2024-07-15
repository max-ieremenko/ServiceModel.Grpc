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

using Grpc.AspNetCore.Server;
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding;

internal sealed class PostConfigureGrpcServiceOptions<TService> : IPostConfigureOptions<GrpcServiceOptions<TService>>
    where TService : class
{
    private readonly IOptions<ServiceModelGrpcServiceOptions> _rootConfiguration;
    private readonly IOptions<ServiceModelGrpcServiceOptions<TService>> _serviceConfiguration;
    private readonly ILoggerFactory _loggerFactory;

    public PostConfigureGrpcServiceOptions(
        IOptions<ServiceModelGrpcServiceOptions> rootConfiguration,
        IOptions<ServiceModelGrpcServiceOptions<TService>> serviceConfiguration,
        ILoggerFactory loggerFactory)
    {
        _rootConfiguration = GrpcPreconditions.CheckNotNull(rootConfiguration, nameof(rootConfiguration));
        _serviceConfiguration = GrpcPreconditions.CheckNotNull(serviceConfiguration, nameof(serviceConfiguration));
        _loggerFactory = GrpcPreconditions.CheckNotNull(loggerFactory, nameof(loggerFactory));
    }

    public void PostConfigure(string? name, GrpcServiceOptions<TService> options)
    {
        var marshallerFactory = _serviceConfiguration.Value.MarshallerFactory ?? _rootConfiguration.Value.DefaultMarshallerFactory;
        var detailMarshaller = _serviceConfiguration.Value.ErrorDetailSerializer ?? _rootConfiguration.Value.DefaultErrorDetailSerializer;

        PostConfigureGrpcServiceOptions.AddErrorHandler(
            options.Interceptors,
            _serviceConfiguration.Value.ErrorHandlerFactory,
            marshallerFactory,
            detailMarshaller,
            _loggerFactory);
    }
}