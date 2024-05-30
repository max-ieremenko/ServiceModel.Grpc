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

using System.IO;
using Grpc.Core;
using Grpc.Core.Utils;
using ServiceModel.Grpc.Configuration.IO;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides set of helpers for <see cref="SerializationContext"/> and <see cref="DeserializationContext"/>.
/// </summary>
public static class SerializationContextExtensions
{
    /// <summary>
    /// Creates a writable <see cref="Stream"/> that can be used to write data into <see cref="SerializationContext"/>.
    /// </summary>
    /// <param name="context">Serialization context.</param>
    /// <returns>Writable <see cref="Stream"/>.</returns>
    public static Stream AsStream(this SerializationContext context)
    {
        GrpcPreconditions.CheckNotNull(context, nameof(context));

        return new BufferWriterStream(context.GetBufferWriter());
    }

    /// <summary>
    /// Creates a readable <see cref="Stream"/> that can be used to read data from <see cref="DeserializationContext"/>.
    /// </summary>
    /// <param name="context">Deserialization context.</param>
    /// <returns>Readable <see cref="Stream"/>.</returns>
    public static Stream AsStream(this DeserializationContext context)
    {
        GrpcPreconditions.CheckNotNull(context, nameof(context));

        return new BufferReaderStream(context.PayloadAsReadOnlySequence());
    }
}