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

using Grpc.Core;
using MessagePack;
using ServiceModel.Grpc.Benchmarks.MarshallerTest;

namespace ServiceModel.Grpc.Benchmarks;

public class MessagePackMarshallerBenchmark : MarshallerBenchmarkBase
{
    internal override Marshaller<T> CreateDefaultMarshaller<T>() => MessagePackTest.CreateDefaultMarshaller<T>();

    internal override Marshaller<T> CreateStreamMarshaller<T>() => MessagePackTest.CreateStreamMarshaller<T>();

    internal override byte[] Serialize<T>(T value) => MessagePackSerializer.Serialize(value);
}