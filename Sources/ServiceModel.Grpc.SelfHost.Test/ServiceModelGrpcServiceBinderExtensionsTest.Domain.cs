// <copyright>
// Copyright 2022-2023 Max Ieremenko
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

using System.Collections.Generic;
using Grpc.Core;
using Shouldly;

namespace ServiceModel.Grpc.SelfHost;

public partial class ServiceModelGrpcServiceBinderExtensionsTest
{
    private sealed class DummyServiceBinder : ServiceBinderBase
    {
        public IList<IMethod> Methods { get; } = new List<IMethod>();

        public override void AddMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            UnaryServerMethod<TRequest, TResponse>? handler)
        {
            method.ShouldNotBeNull();
            handler.ShouldNotBeNull();

            Methods.Add(method);
        }

        public override void AddMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            ClientStreamingServerMethod<TRequest, TResponse>? handler)
        {
            method.ShouldNotBeNull();
            handler.ShouldNotBeNull();

            Methods.Add(method);
        }

        public override void AddMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            ServerStreamingServerMethod<TRequest, TResponse>? handler)
        {
            method.ShouldNotBeNull();
            handler.ShouldNotBeNull();

            Methods.Add(method);
        }

        public override void AddMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            DuplexStreamingServerMethod<TRequest, TResponse>? handler)
        {
            method.ShouldNotBeNull();
            handler.ShouldNotBeNull();

            Methods.Add(method);
        }
    }
}