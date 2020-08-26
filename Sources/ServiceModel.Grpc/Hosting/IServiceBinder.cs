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

using Grpc.Core;

namespace ServiceModel.Grpc.Hosting
{
    internal interface IServiceBinder
    {
        void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;
    }
}
