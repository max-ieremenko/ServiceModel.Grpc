// <copyright>
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

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

internal sealed class SwaggerUiRequestHandler : ISwaggerUiRequestHandler
{
    private readonly IDataSerializer _serializer;

    public SwaggerUiRequestHandler(IDataSerializer serializer)
    {
        _serializer = serializer;
    }

    public async Task<byte[]> ReadRequestMessageAsync(
        PipeReader bodyReader,
        IMarshallerFactory marshallerFactory,
        IOperationDescriptor descriptor,
        CancellationToken token)
    {
        var accessor = descriptor.GetRequestAccessor();
        var request = accessor.CreateNew();

        if (accessor.Names.Length > 0)
        {
            JsonElement body;
            using (var stream = bodyReader.AsStream())
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                body = await JsonSerializer.DeserializeAsync<JsonElement>(stream, options, token).ConfigureAwait(false);
            }

            foreach (var entry in body.EnumerateObject())
            {
                var index = FindIndex(accessor.Names, entry.Name);
                if (index < 0)
                {
                    continue;
                }

                var parameterType = accessor.GetValueType(index);
                try
                {
                    var value = _serializer.Deserialize(entry.Value.GetRawText(), parameterType);
                    accessor.SetValue(request, index, value);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Fail to deserialize parameter [{entry.Name}] with type [{parameterType}] from request.",
                        ex);
                }
            }
        }

        var payload = MarshallerExtensions.SerializeObject(marshallerFactory, request);

        var result = new byte[payload.Length + 5];

        // not compressed
        result[0] = 0;

        // length
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(1), (uint)payload.Length);

        // data
        Buffer.BlockCopy(payload, 0, result, 5, payload.Length);

        return result;
    }

    public Task WriteResponseMessageAsync(
        MemoryStream original,
        PipeWriter bodyWriter,
        IMarshallerFactory marshallerFactory,
        IOperationDescriptor descriptor,
        CancellationToken token)
    {
        var accessor = descriptor.GetResponseAccessor();
        if (accessor.Names.Length == 0)
        {
            return Task.CompletedTask;
        }

        var payload = new ReadOnlySequence<byte>(original.GetBuffer(), 5, (int)(original.Length - 5));
        var response = MarshallerExtensions.DeserializeObject(marshallerFactory, accessor.GetInstanceType(), payload);

        var responseValue = accessor.GetValue(response, 0);
        if (responseValue == null)
        {
            return Task.CompletedTask;
        }

        return _serializer.SerializeAsync(bodyWriter.AsStream(true), responseValue, accessor.GetValueType(0), token);
    }

    public void AppendResponseTrailers(
        IHeaderDictionary responseHeaders,
        IHeaderDictionary? trailers)
    {
        if (trailers == null || trailers.Count == 0)
        {
            return;
        }

        foreach (var entry in trailers)
        {
            responseHeaders.TryAdd(entry.Key, entry.Value);
        }
    }

    public Task WriteResponseErrorAsync(
        RpcException error,
        PipeWriter bodyWriter,
        CancellationToken token)
    {
        return _serializer.SerializeAsync(bodyWriter.AsStream(true), error, typeof(RpcException), token);
    }

    private static int FindIndex(string[] names, string name)
    {
        for (var i = 0; i < names.Length; i++)
        {
            if (names[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}