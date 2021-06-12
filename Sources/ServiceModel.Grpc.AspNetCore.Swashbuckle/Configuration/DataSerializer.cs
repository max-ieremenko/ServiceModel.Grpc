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
using System.Threading;
using System.Threading.Tasks;
using ServiceModel.Grpc.AspNetCore.Internal.Swagger;

namespace ServiceModel.Grpc.AspNetCore.Swashbuckle.Configuration
{
    internal sealed class DataSerializer : IDataSerializer
    {
        private readonly IJsonSerializer _jsonSerializer;

        public DataSerializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public object Deserialize(string json, Type returnType)
        {
            return _jsonSerializer.Deserialize(json, returnType);
        }

        public Task SerializeAsync(Stream stream, object value, Type inputType, CancellationToken cancellationToken)
        {
            return _jsonSerializer.SerializeAsync(stream, value, inputType, cancellationToken);
        }
    }
}
