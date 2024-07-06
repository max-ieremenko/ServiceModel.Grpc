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

using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors.Internal;

internal static class ErrorHandlerInterceptorFactory
{
    public static Interceptor CreateClientHandler(IClientErrorHandler errorHandler, IMarshallerFactory marshallerFactory, ILogger? logger)
    {
        var interceptor = new ClientCallErrorInterceptor(errorHandler, marshallerFactory, logger);
        return new ClientNativeInterceptor(interceptor);
    }

    public static Interceptor CreateServerHandler(IServerErrorHandler errorHandler, IMarshallerFactory marshallerFactory, ILogger? logger)
    {
        var interceptor = new ServerCallErrorInterceptor(errorHandler, marshallerFactory, logger);
        return new ServerNativeInterceptor(interceptor);
    }

    public static Type GetServerHandlerType() => typeof(ServerNativeInterceptor);

    public static object CreateServerHandlerArgs(Func<IServiceProvider, IServerErrorHandler> errorHandlerFactory, IMarshallerFactory marshallerFactory, ILogger? logger)
        => new ErrorHandlerServerCallInterceptorFactory(marshallerFactory, errorHandlerFactory, logger);
}