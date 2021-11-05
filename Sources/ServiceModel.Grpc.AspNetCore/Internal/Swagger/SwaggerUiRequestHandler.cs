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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger
{
    internal sealed partial class SwaggerUiRequestHandler : ISwaggerUiRequestHandler
    {
        private readonly IDataSerializer _serializer;

        public SwaggerUiRequestHandler(IDataSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<byte[]> ReadRequestMessageAsync(
            PipeReader bodyReader,
            IList<string> orderedParameterNames,
            IMethod method,
            CancellationToken token)
        {
            var values = new object?[orderedParameterNames.Count];
            var methodAccessor = CreateMethodAccessor(method);

            if (orderedParameterNames.Count > 0)
            {
                var requestParameterTypes = methodAccessor.GetParameterTypes();

                JsonElement body;
                using (var stream = bodyReader.AsStream())
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    body = await JsonSerializer.DeserializeAsync<JsonElement>(stream, options, token).ConfigureAwait(false);
                }

                foreach (var entry in body.EnumerateObject())
                {
                    var index = FindIndex(orderedParameterNames, entry.Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    var parameterType = requestParameterTypes[index];
                    try
                    {
                        values[index] = _serializer.Deserialize(entry.Value.GetRawText(), parameterType);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            "Fail to deserialize parameter [{0}] with type [{1}] from request.".FormatWith(entry.Name, parameterType),
                            ex);
                    }
                }
            }

            var payload = methodAccessor.SerializeRequest(values);

            var result = new byte[payload.Length + 5];

            // not compressed
            result[0] = 0;

            // length
            BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(1), (uint)payload.Length);

            // data
            Buffer.BlockCopy(payload, 0, result, 5, payload.Length);

            return result;
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
                if (!responseHeaders.ContainsKey(entry.Key))
                {
                    responseHeaders.Add(entry.Key, entry.Value);
                }
            }
        }

        public Task WriteResponseMessageAsync(
            MemoryStream original,
            PipeWriter bodyWriter,
            IMethod method,
            CancellationToken token)
        {
            var methodAccessor = CreateMethodAccessor(method);
            var responseType = methodAccessor.GetResponseType();
            if (responseType == null)
            {
                return Task.CompletedTask;
            }

            var payload = new byte[original.Length - 5];
            Buffer.BlockCopy(original.GetBuffer(), 5, payload, 0, payload.Length);

            var response = methodAccessor.DeserializeResponse(payload);
            if (response == null)
            {
                return Task.CompletedTask;
            }

            return _serializer.SerializeAsync(bodyWriter.AsStream(true), response, responseType, token);
        }

        public Task WriteResponseErrorAsync(
            RpcException error,
            PipeWriter bodyWriter,
            CancellationToken token)
        {
            return _serializer.SerializeAsync(bodyWriter.AsStream(true), error, typeof(RpcException), token);
        }

        private static int FindIndex(IList<string> names, string name)
        {
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static IMethodAccessor CreateMethodAccessor(IMethod method)
        {
            var type = typeof(MethodAccessor<,>).MakeGenericType(method.GetType().GetGenericArguments());
            var instance = Activator.CreateInstance(type, method)!;
            return (IMethodAccessor)instance;
        }
    }
}
