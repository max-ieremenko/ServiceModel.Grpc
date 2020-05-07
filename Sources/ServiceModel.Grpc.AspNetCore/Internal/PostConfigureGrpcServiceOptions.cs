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

using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;
using ServiceModel.Grpc.Interceptors.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class PostConfigureGrpcServiceOptions : IPostConfigureOptions<GrpcServiceOptions>
    {
        private readonly IOptions<ServiceModelGrpcServiceOptions> _serviceModelOptions;

        public PostConfigureGrpcServiceOptions(IOptions<ServiceModelGrpcServiceOptions> serviceModelOptions)
        {
            serviceModelOptions.AssertNotNull(nameof(serviceModelOptions));

            _serviceModelOptions = serviceModelOptions;
        }

        public void PostConfigure(string name, GrpcServiceOptions options)
        {
            AddErrorHandler(
                options.Interceptors,
                _serviceModelOptions.Value.DefaultErrorHandler,
                _serviceModelOptions.Value.DefaultMarshallerFactory);
        }

        internal static void AddErrorHandler(
            InterceptorCollection interceptors,
            IServerErrorHandler errorHandler,
            IMarshallerFactory marshallerFactory)
        {
            if (errorHandler != null)
            {
                interceptors.Add<ServerNativeInterceptor>(new ServerCallErrorInterceptor(
                    errorHandler,
                    marshallerFactory.ThisOrDefault()));
            }
        }
    }
}
