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
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Interceptors.Internal
{
    internal sealed class ServerCallErrorInterceptor : IServerCallInterceptor
    {
        internal const string VisitMarker = nameof(ServerCallErrorInterceptor);

        private readonly IServerErrorHandler _errorHandler;
        private readonly IMarshallerFactory _marshallerFactory;

        public ServerCallErrorInterceptor(
            IServerErrorHandler errorHandler,
            IMarshallerFactory marshallerFactory)
        {
            errorHandler.AssertNotNull(nameof(errorHandler));
            marshallerFactory.AssertNotNull(nameof(marshallerFactory));

            _errorHandler = errorHandler;
            _marshallerFactory = marshallerFactory;
        }

        public void OnError(ServerCallInterceptorContext context, Exception error)
        {
            if (context.ServerCallContext.UserState.TryGetValue(VisitMarker, out _))
            {
                return;
            }

            context.ServerCallContext.UserState.Add(VisitMarker, string.Empty);

            var faultOrIgnore = _errorHandler.ProvideFaultOrIgnore(context, error);
            if (!faultOrIgnore.HasValue)
            {
                return;
            }

            var fault = faultOrIgnore.Value;

            var status = new Status(fault.StatusCode ?? StatusCode.Internal, fault.Message ?? error.Message);
            var metadata = fault.Trailers;
            if (fault.Detail != null)
            {
                if (metadata == null)
                {
                    metadata = new Metadata();
                }

                metadata.Add(CallContext.HeaderNameErrorDetail, _marshallerFactory.SerializeHeader(fault.Detail));
                metadata.Add(CallContext.HeaderNameErrorDetailType, fault.Detail.GetType().GetShortAssemblyQualifiedName());
            }

            throw new RpcException(status, metadata ?? Metadata.Empty, status.Detail);
        }
    }
}
