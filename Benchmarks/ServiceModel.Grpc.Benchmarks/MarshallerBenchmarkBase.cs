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

using BenchmarkDotNet.Attributes;
using Grpc.Core;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Benchmarks.MarshallerTest;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class MarshallerBenchmarkBase
{
    private StubSerializationContext _serializationContext = null!;
    private StubDeserializationContext _deserializationContext = null!;

    private Marshaller<Message<SomeObject>> _marshaller = null!;
    private Message<SomeObject> _payload = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _marshaller = CreateMarshaller<Message<SomeObject>>();
        _payload = new Message<SomeObject>(DomainExtensions.CreateSomeObject());

        _serializationContext = new StubSerializationContext();
        _deserializationContext = new StubDeserializationContext(MarshallerExtensions.Serialize(_marshaller, _payload));
    }

    [Benchmark]
    public void Serialize() => _marshaller.ContextualSerializer(_payload, _serializationContext);

    [Benchmark]
    public void Deserialize() => _marshaller.ContextualDeserializer(_deserializationContext);

    internal abstract Marshaller<T> CreateMarshaller<T>();
}