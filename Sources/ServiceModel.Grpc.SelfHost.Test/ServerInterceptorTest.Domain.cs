﻿// <copyright>
// Copyright 2024 Max Ieremenko
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

using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.SelfHost;

public partial class ServerInterceptorTest
{
    private sealed class HackInterceptor : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            if (context.Method == $"/{nameof(IMultipurposeService)}/{nameof(IMultipurposeService.BlockingCallAsync)}")
            {
                HackBlockingCallRequest(request);
            }

            return continuation(request, context);
        }

        private static void HackBlockingCallRequest(object request)
        {
            var message = (Message<int, string>)request;
            message.Value2 += "_h_";
        }
    }
}