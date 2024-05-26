// <copyright>
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

using System.ComponentModel;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace ServiceModel.Grpc.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GrpcMethodFactory
{
    public static IMethod Unary<TRequest, TResponse>(IMarshallerFactory marshallerFactory, string serviceName, string name) =>
        new Method<TRequest, TResponse>(
            MethodType.Unary,
            serviceName,
            name,
            marshallerFactory.CreateMarshaller<TRequest>(),
            marshallerFactory.CreateMarshaller<TResponse>());

    public static IMethod ClientStreaming<TRequestHeader, TRequest, TResponse>(IMarshallerFactory marshallerFactory, string serviceName, string name, bool withHeader) =>
        new GrpcMethod<TRequestHeader, TRequest, Message, TResponse>(
            MethodType.ClientStreaming,
            serviceName,
            name,
            withHeader ? marshallerFactory.CreateMarshaller<TRequestHeader>() : null,
            marshallerFactory.CreateMarshaller<TRequest>(),
            null,
            marshallerFactory.CreateMarshaller<TResponse>());

    public static IMethod ServerStreaming<TRequest, TResponseHeader, TResponse>(IMarshallerFactory marshallerFactory, string serviceName, string name, bool withHeader) =>
        new GrpcMethod<Message, TRequest, TResponseHeader, TResponse>(
            MethodType.ServerStreaming,
            serviceName,
            name,
            null,
            marshallerFactory.CreateMarshaller<TRequest>(),
            withHeader ? marshallerFactory.CreateMarshaller<TResponseHeader>() : null,
            marshallerFactory.CreateMarshaller<TResponse>());

    public static IMethod DuplexStreaming<TRequestHeader, TRequest, TResponseHeader, TResponse>(
        IMarshallerFactory marshallerFactory,
        string serviceName,
        string name,
        bool withRequestHeader,
        bool withResponseHeader) =>
        new GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse>(
            MethodType.DuplexStreaming,
            serviceName,
            name,
            withRequestHeader ? marshallerFactory.CreateMarshaller<TRequestHeader>() : null,
            marshallerFactory.CreateMarshaller<TRequest>(),
            withResponseHeader ? marshallerFactory.CreateMarshaller<TResponseHeader>() : null,
            marshallerFactory.CreateMarshaller<TResponse>());
}