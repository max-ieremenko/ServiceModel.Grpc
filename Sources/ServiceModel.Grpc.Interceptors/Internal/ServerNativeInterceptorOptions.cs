﻿// <copyright>
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

using Grpc.Core.Utils;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors.Internal;

internal sealed class ServerNativeInterceptorOptions
{
    private readonly IMarshallerFactory _marshallerFactory;
    private readonly Func<IServiceProvider, IServerErrorHandler> _errorHandlerFactory;
    private readonly IServerFaultDetailSerializer? _detailMarshaller;
    private readonly ILogger? _logger;

    public ServerNativeInterceptorOptions(
        IMarshallerFactory marshallerFactory,
        IServerFaultDetailSerializer? detailMarshaller,
        Func<IServiceProvider, IServerErrorHandler> errorHandlerFactory,
        ILogger? logger)
    {
        _marshallerFactory = GrpcPreconditions.CheckNotNull(marshallerFactory, nameof(marshallerFactory));
        _errorHandlerFactory = GrpcPreconditions.CheckNotNull(errorHandlerFactory, nameof(errorHandlerFactory));
        _detailMarshaller = detailMarshaller;
        _logger = logger;
    }

    public IServerCallInterceptor CreateInterceptor(IServiceProvider serviceProvider)
    {
        GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

        var errorHandler = _errorHandlerFactory(serviceProvider);
        return new ServerCallErrorInterceptor(errorHandler, _marshallerFactory, _detailMarshaller, _logger);
    }
}