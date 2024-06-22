// <copyright>
// Copyright 2021 Max Ieremenko
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
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

internal partial class SwaggerUiRequestHandler
{
    private interface IMethodAccessor
    {
        Type[] GetParameterTypes();

        byte[] SerializeRequest(object?[] values);

        Type? GetResponseType();

        object? DeserializeResponse(byte[] payload);
    }

    private sealed class MethodAccessor<TRequest, TResponse> : IMethodAccessor
    {
        private readonly Method<TRequest, TResponse> _method;

        public MethodAccessor(IMethod method)
        {
            _method = (Method<TRequest, TResponse>)method;
        }

        public Type[] GetParameterTypes() => typeof(TRequest).GetGenericArguments();

        public byte[] SerializeRequest(object?[] values)
        {
            var message = Activator.CreateInstance(typeof(TRequest), values)!;
            return MarshallerExtensions.Serialize(_method.RequestMarshaller, (TRequest)message);
        }

        public Type? GetResponseType()
        {
            var type = typeof(TResponse);
            return type.IsGenericType ? type.GetGenericArguments()[0] : null;
        }

        public object? DeserializeResponse(byte[] payload)
        {
            var message = MarshallerExtensions.Deserialize(_method.ResponseMarshaller, payload);
            return message!
                .GetType()
                .InstanceProperty(nameof(Message<int>.Value1))
                .GetValue(message);
        }
    }
}