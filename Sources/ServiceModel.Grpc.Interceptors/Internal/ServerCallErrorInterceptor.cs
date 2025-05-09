﻿// <copyright>
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

using Grpc.Core;
using Grpc.Core.Utils;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Interceptors.Internal;

internal sealed class ServerCallErrorInterceptor : IServerCallInterceptor
{
    internal const string VisitMarker = nameof(ServerCallErrorInterceptor);

    private readonly IServerErrorHandler _errorHandler;
    private readonly IMarshallerFactory _marshallerFactory;
    private readonly IServerFaultDetailSerializer? _detailSerializer;
    private readonly ILogger? _logger;

    public ServerCallErrorInterceptor(
        IServerErrorHandler errorHandler,
        IMarshallerFactory marshallerFactory,
        IServerFaultDetailSerializer? detailSerializer,
        ILogger? logger)
    {
        _errorHandler = GrpcPreconditions.CheckNotNull(errorHandler, nameof(errorHandler));
        _marshallerFactory = GrpcPreconditions.CheckNotNull(marshallerFactory, nameof(marshallerFactory));
        _detailSerializer = detailSerializer;
        _logger = logger;
    }

    public void OnError(ServerCallInterceptorContext context, Exception error)
    {
        if (context.ServerCallContext.UserState.TryGetValue(VisitMarker, out _))
        {
            return;
        }

        context.ServerCallContext.UserState.Add(VisitMarker, string.Empty);

        var faultOrIgnore = ProvideFaultOrIgnore(context, error);
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

            AddDetail(metadata, fault.Detail);
        }

        throw new RpcException(status, metadata ?? Metadata.Empty, status.Detail);
    }

    private ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        try
        {
            return _errorHandler.ProvideFaultOrIgnore(context, error);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                "Error occurred while calling {0}.{1}:{2}{3}",
                _errorHandler.GetType(),
                nameof(_errorHandler.ProvideFaultOrIgnore),
                Environment.NewLine,
                ex);
            throw;
        }
    }

    private void AddDetail(Metadata metadata, object detail)
    {
        var detailType = detail.GetType();
        byte[] detailPayload;
        try
        {
            detailPayload = _detailSerializer == null
                ? DefaultServerFaultDetailSerializer.Serialize(_marshallerFactory, detail)
                : _detailSerializer.SerializeDetail(_marshallerFactory, detail);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                "Error occurred while trying to serialize the instance of {0} with {1}:{2}{3}",
                detailType,
                _marshallerFactory.GetType(),
                Environment.NewLine,
                ex);
            throw;
        }

        string typePayload;
        try
        {
            typePayload = _detailSerializer == null
                ? DefaultServerFaultDetailSerializer.SerializeType(detailType)
                : _detailSerializer.SerializeDetailType(detailType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                "Error occurred while trying to serialize the type name of {0}:{1}{2}",
                detailType,
                Environment.NewLine,
                ex);
            throw;
        }

        metadata.Add(Headers.HeaderNameErrorDetail, detailPayload);
        metadata.Add(Headers.HeaderNameErrorDetailType, typePayload);
    }
}