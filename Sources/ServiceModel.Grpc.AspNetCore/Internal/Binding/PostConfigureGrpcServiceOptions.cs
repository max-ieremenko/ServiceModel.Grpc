// <copyright>
// Copyright 2020-2023 Max Ieremenko
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
using Grpc.AspNetCore.Server;
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.Interceptors.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding;

internal sealed class PostConfigureGrpcServiceOptions : IPostConfigureOptions<GrpcServiceOptions>
{
    private const string ErrorInterceptorLoggerName = "ServiceModel.Grpc.Interceptors.ServerCallErrorInterceptor";

    private readonly IOptions<ServiceModelGrpcServiceOptions> _serviceModelOptions;
    private readonly ILoggerFactory _loggerFactory;

    public PostConfigureGrpcServiceOptions(IOptions<ServiceModelGrpcServiceOptions> serviceModelOptions, ILoggerFactory loggerFactory)
    {
        _serviceModelOptions = GrpcPreconditions.CheckNotNull(serviceModelOptions, nameof(serviceModelOptions));
        _loggerFactory = GrpcPreconditions.CheckNotNull(loggerFactory, nameof(loggerFactory));
    }

    public void PostConfigure(string? name, GrpcServiceOptions options)
    {
        AddErrorHandler(
            options.Interceptors,
            _serviceModelOptions.Value.DefaultErrorHandlerFactory,
            _serviceModelOptions.Value.DefaultMarshallerFactory,
            _loggerFactory);
    }

    internal static void AddErrorHandler(
        InterceptorCollection interceptors,
        Func<IServiceProvider, IServerErrorHandler>? errorHandlerFactory,
        IMarshallerFactory? marshallerFactory,
        ILoggerFactory loggerFactory)
    {
        if (errorHandlerFactory != null)
        {
            var args = ErrorHandlerInterceptorFactory.CreateServerHandlerArgs(
                errorHandlerFactory,
                marshallerFactory.ThisOrDefault(),
                CreateLogger(loggerFactory));

            interceptors.Add(ErrorHandlerInterceptorFactory.GetServerHandlerType(), args);
        }
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(ErrorInterceptorLoggerName);
        return new LogAdapter(logger);
    }
}