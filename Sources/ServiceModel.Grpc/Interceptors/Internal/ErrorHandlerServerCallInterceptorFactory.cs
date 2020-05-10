// <copyright>
// Copyright 2020 Max Ieremenko
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
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors.Internal
{
    internal sealed class ErrorHandlerServerCallInterceptorFactory : IServerCallInterceptorFactory
    {
        private readonly IMarshallerFactory _marshallerFactory;
        private readonly Func<IServiceProvider, IServerErrorHandler> _errorHandlerFactory;

        public ErrorHandlerServerCallInterceptorFactory(
            IMarshallerFactory marshallerFactory,
            Func<IServiceProvider, IServerErrorHandler> errorHandlerFactory)
        {
            marshallerFactory.AssertNotNull(nameof(marshallerFactory));

            _marshallerFactory = marshallerFactory;
            _errorHandlerFactory = errorHandlerFactory;
        }

        public IServerCallInterceptor CreateInterceptor(IServiceProvider serviceProvider)
        {
            serviceProvider.AssertNotNull(nameof(serviceProvider));

            var errorHandler = _errorHandlerFactory(serviceProvider);
            return new ServerCallErrorInterceptor(errorHandler, _marshallerFactory);
        }
    }
}
