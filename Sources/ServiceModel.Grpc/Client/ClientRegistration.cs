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

using System.Runtime.CompilerServices;
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client;

internal sealed class ClientRegistration
{
    private readonly object _builder;
    private readonly Interceptor? _interceptor;

    private ClientRegistration(object builder, Interceptor? interceptor)
    {
        _builder = builder;
        _interceptor = interceptor;
    }

    public static ClientRegistration Build<TContract>(
        IClientBuilder<TContract>? builder,
        ServiceModelGrpcClientOptions? defaultOptions,
        Action<ServiceModelGrpcClientOptions>? configure)
    {
        var options = new ServiceModelGrpcClientOptions
        {
            MarshallerFactory = defaultOptions?.MarshallerFactory,
            DefaultCallOptionsFactory = defaultOptions?.DefaultCallOptionsFactory,
            Logger = defaultOptions?.Logger,
            ErrorHandler = defaultOptions?.ErrorHandler,
            ErrorDetailDeserializer = defaultOptions?.ErrorDetailDeserializer,
            ServiceProvider = defaultOptions?.ServiceProvider
        };

        configure?.Invoke(options);

        if (builder == null)
        {
            builder = EmitBuilder<TContract>(options.Logger);
        }

        var methodBinder = new ClientMethodBinder(options.ServiceProvider, options.MarshallerFactory.ThisOrDefault(), options.DefaultCallOptionsFactory);
        methodBinder.AddFilters(defaultOptions?.GetFilters());
        methodBinder.AddFilters(options.GetFilters());

        builder.Initialize(methodBinder);

        Interceptor? interceptor = null;
        if (options.ErrorHandler != null)
        {
            interceptor = ErrorHandlerInterceptorFactory.CreateClientHandler(
                options.ErrorHandler,
                methodBinder.MarshallerFactory,
                options.ErrorDetailDeserializer,
                options.Logger);
        }

        return new ClientRegistration(builder, interceptor);
    }

    public TContract Create<TContract>(CallInvoker callInvoker)
    {
        var builder = (IClientBuilder<TContract>)_builder;
        if (_interceptor != null)
        {
            callInvoker = callInvoker.Intercept(_interceptor);
        }

        return builder.Build(callInvoker);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IClientBuilder<TContract> EmitBuilder<TContract>(ILogger? logger)
    {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new PlatformNotSupportedException("Dynamic code generation is not supported on this platform.");
        }
#endif
        if (Features.IsReflectionEmitDisabled)
        {
            throw new NotSupportedException("ServiceModel.Grpc.Emit is disabled within this application.");
        }

        return EmitGenerator.GenerateClientBuilder<TContract>(logger);
    }
}