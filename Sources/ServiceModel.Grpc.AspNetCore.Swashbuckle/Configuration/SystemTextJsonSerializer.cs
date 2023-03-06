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
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core.Utils;

namespace ServiceModel.Grpc.AspNetCore.Swashbuckle.Configuration;

/// <summary>
/// Provides functionality to serialize objects or value types to JSON and to deserialize JSON into objects or value types via <see cref="JsonSerializer"/>.
/// </summary>
public class SystemTextJsonSerializer : IJsonSerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    public SystemTextJsonSerializer()
        : this(CreateDefaultOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class with <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Options to control the conversion behavior.</param>
    public SystemTextJsonSerializer(JsonSerializerOptions options)
    {
        Options = GrpcPreconditions.CheckNotNull(options, nameof(options));
    }

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public JsonSerializerOptions Options { get; }

    /// <summary>
    /// Parse the text representing a single JSON value into a <paramref name="returnType"/>.
    /// </summary>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="returnType">The type of the object to convert to and return.</param>
    /// <returns>A <paramref name="returnType"/> representation of the JSON value.</returns>
    public virtual object Deserialize(string json, Type returnType)
    {
        return JsonSerializer.Deserialize(json, returnType, Options)!;
    }

    /// <summary>
    /// Convert the provided value to JSON text and write it to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to write to.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="inputType">The type of the <paramref name="value"/> to convert.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> which may be used to cancel the write operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public virtual Task SerializeAsync(Stream stream, object value, Type inputType, CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsync(stream, value, inputType, Options, cancellationToken);
    }

    internal static JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}