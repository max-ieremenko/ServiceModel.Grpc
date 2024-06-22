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

using System;
using System.ComponentModel;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Configuration;

#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1615 // Element return value should be documented
#pragma warning disable SA1604 // Element documentation should have summary

namespace ServiceModel.Grpc.Interceptors.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ErrorHandlerInterceptorFactory
{
    /// <exclude />
    public static Interceptor CreateClientHandler(IClientErrorHandler errorHandler, IMarshallerFactory marshallerFactory, ILogger? logger)
    {
        var interceptor = new ClientCallErrorInterceptor(errorHandler, marshallerFactory, logger);
        return new ClientNativeInterceptor(interceptor);
    }

    /// <exclude />
    public static Interceptor CreateServerHandler(IServerErrorHandler errorHandler, IMarshallerFactory marshallerFactory, ILogger? logger)
    {
        var interceptor = new ServerCallErrorInterceptor(errorHandler, marshallerFactory, logger);
        return new ServerNativeInterceptor(interceptor);
    }

    /// <exclude />
    public static Type GetServerHandlerType() => typeof(ServerNativeInterceptor);

    /// <exclude />
    public static object CreateServerHandlerArgs(Func<IServiceProvider, IServerErrorHandler> errorHandlerFactory, IMarshallerFactory marshallerFactory, ILogger? logger)
        => new ErrorHandlerServerCallInterceptorFactory(marshallerFactory, errorHandlerFactory, logger);
}