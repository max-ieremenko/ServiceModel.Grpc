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
using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Grpc.Core.Utils;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors.Internal;

internal sealed class ClientCallErrorInterceptor : IClientCallInterceptor
{
    private readonly ILogger? _logger;

    public ClientCallErrorInterceptor(
        IClientErrorHandler errorHandler,
        IMarshallerFactory marshallerFactory,
        ILogger? logger)
    {
        ErrorHandler = GrpcPreconditions.CheckNotNull(errorHandler, nameof(errorHandler));
        MarshallerFactory = GrpcPreconditions.CheckNotNull(marshallerFactory, nameof(marshallerFactory));
        _logger = logger;
    }

    public IClientErrorHandler ErrorHandler { get; }

    public IMarshallerFactory MarshallerFactory { get; }

    public void OnError(ClientCallInterceptorContext context, RpcException error)
    {
        object? detail = null;
        if (TryFindHeaders(error.Trailers, out var detailTypeName, out var detailContent))
        {
            var detailType = ResolveDetailType(detailTypeName);
            if (detailType != null)
            {
                detail = MarshallerFactory.DeserializeHeader(detailType, detailContent);
            }
        }

        ErrorHandler.ThrowOrIgnore(context, new ClientFaultDetail(error, detail));
    }

    private static bool TryFindHeaders(Metadata metadata, [NotNullWhen(true)] out string? detailTypeName, [NotNullWhen(true)] out byte[]? detailContent)
    {
        detailTypeName = null;
        detailContent = null;

        for (var i = 0; i < metadata.Count; i++)
        {
            var entry = metadata[i];
            if (entry.Equals(CallContext.HeaderNameErrorDetailType, false))
            {
                detailTypeName = entry.Value;
            }
            else if (entry.Equals(CallContext.HeaderNameErrorDetail, true))
            {
                detailContent = entry.ValueBytes;
            }

            // empty byte[] headers are ignored by server-side
            if (detailTypeName != null && detailContent != null)
            {
                return true;
            }
        }

        return false;
    }

    private Type? ResolveDetailType(string typeName)
    {
        try
        {
            return Type.GetType(typeName, true, false);
        }
        catch (Exception ex)
        {
            _logger?.LogError("Fail to resolve fault detail type {0}: {1}", typeName, ex);
        }

        return null;
    }
}