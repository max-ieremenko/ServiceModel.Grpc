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

using Grpc.Net.Client;
using ServiceModel.Grpc.Benchmarks.Domain;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Client;

internal sealed class NativeGrpcClientCallTest : IUnaryCallTest
{
    private readonly SomeObjectProto _payload;
    private readonly StubHttpMessageHandler _httpHandler;
    private readonly GrpcChannel _channel;
    private readonly TestServiceNative.TestServiceNativeClient _proxy;

    public NativeGrpcClientCallTest(SomeObjectProto payload)
    {
        _payload = payload;

        _httpHandler = new StubHttpMessageHandler(MessageSerializer.Create(_payload));
        _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = _httpHandler });

        _proxy = new TestServiceNative.TestServiceNativeClient(_channel);
    }

    public Task StartAsync() => Task.CompletedTask;

    public async Task PingPongAsync()
    {
        using (var call = _proxy.PingPongAsync(_payload))
        {
            await call.ResponseAsync.ConfigureAwait(false);
        }
    }

    public async ValueTask<long> GetPingPongPayloadSize()
    {
        await PingPongAsync().ConfigureAwait(false);
        return _httpHandler.PayloadSize;
    }

    public ValueTask DisposeAsync()
    {
        _channel.Dispose();
        _httpHandler.Dispose();
        return ValueTask.CompletedTask;
    }
}