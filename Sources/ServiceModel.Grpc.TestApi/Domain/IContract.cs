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

using System.ServiceModel;
using Grpc.Core;

namespace ServiceModel.Grpc.TestApi.Domain;

[ServiceContract]
public interface IContract : IDisposable
{
    [OperationContract]
    void Empty();

    [OperationContract]
    Task EmptyAsync();

    [OperationContract]
    ValueTask EmptyValueTaskAsync();

    [OperationContract]
    void EmptyContext(CallContext context);

    [OperationContract]
    Task EmptyTokenAsync(CancellationToken token);

    [OperationContract]
    string ReturnString();

    [OperationContract]
    Task<string> ReturnStringAsync(ServerCallContext? context = default);

    [OperationContract]
    ValueTask<bool> ReturnValueTaskBoolAsync();

    [OperationContract]
    void OneParameterContext(CallOptions options, int value);

    [OperationContract]
    Task OneParameterAsync(double value);

    [OperationContract]
    double AddTwoValues(int x, double y);

    [OperationContract]
    Task<string> ConcatThreeValueAsync(int x, string y, CancellationToken token, long z);

    [OperationContract(Name = "DuplicateUnary1")]
    string DuplicateUnary();

    [OperationContract(Name = "DuplicateUnary2")]
    string DuplicateUnary(string value);

    [OperationContract]
    Task UnaryNullableCancellationToken(TimeSpan timeout, CancellationToken? token = default);

    [OperationContract]
    Task UnaryNullableCallOptions(TimeSpan timeout, CallOptions? options = default);

    // [OperationContract] => BlockingCallAsync
    string BlockingCall(int x, string y, CancellationToken token);

    [OperationContract]
    Task<string> BlockingCallAsync(int x, string y, CancellationToken token);

    [OperationContract]
    IAsyncEnumerable<int> EmptyServerStreaming();

    [OperationContract]
    IAsyncEnumerable<int> ServerStreamingRepeatValue(int value, int count, CancellationToken token);

    [OperationContract]
    Task<IAsyncEnumerable<int>> ServerStreamingRepeatValueAsync(int value, int count, CancellationToken token);

    [OperationContract]
    ValueTask<IAsyncEnumerable<int>> ServerStreamingRepeatValueValueTaskAsync(int value, int count, CancellationToken token);

    [OperationContract]
    Task<(int Value, IAsyncEnumerable<int> Stream, int Count)> ServerStreamingWithHeadersTask(int value, int count, CancellationToken token);

    [OperationContract]
    ValueTask<(IAsyncEnumerable<int> Stream, int Count)> ServerStreamingWithHeadersValueTask(int value, int count, CancellationToken token);

    [OperationContract(Name = "DuplicateServerStreaming1")]
    IAsyncEnumerable<string> DuplicateServerStreaming();

    [OperationContract(Name = "DuplicateServerStreaming2")]
    IAsyncEnumerable<string> DuplicateServerStreaming(string value);

    [OperationContract]
    Task ClientStreamingEmpty(IAsyncEnumerable<int> values);

    [OperationContract]
    ValueTask ClientStreamingEmptyValueTask(IAsyncEnumerable<int> values);

    [OperationContract]
    Task<string> ClientStreamingSumValues(IAsyncEnumerable<int> values, CancellationToken token);

    [OperationContract]
    ValueTask<string> ClientStreamingSumValuesValueTask(IAsyncEnumerable<int> values, CancellationToken token);

    [OperationContract]
    Task<string> ClientStreamingHeaderParameters(IAsyncEnumerable<int> values, int multiplier, string prefix);

    [OperationContract(Name = "DuplicateClientStreaming1")]
    Task<string> DuplicateClientStreaming(IAsyncEnumerable<string> values);

    [OperationContract(Name = "DuplicateClientStreaming2")]
    Task<string> DuplicateClientStreaming(IAsyncEnumerable<int> values);

    [OperationContract]
    IAsyncEnumerable<string> DuplexStreamingConvert(IAsyncEnumerable<int> values, CancellationToken token);

    [OperationContract]
    Task<IAsyncEnumerable<string>> DuplexStreamingConvertAsync(IAsyncEnumerable<int> values, CancellationToken token);

    [OperationContract]
    ValueTask<IAsyncEnumerable<string>> DuplexStreamingConvertValueTaskAsync(IAsyncEnumerable<int> values, CancellationToken token);

    [OperationContract]
    IAsyncEnumerable<string> DuplexStreamingHeaderParameters(IAsyncEnumerable<int> values, int multiplier, string prefix);

    [OperationContract(Name = "DuplicateDuplexStreaming1")]
    IAsyncEnumerable<string> DuplicateDuplexStreaming(IAsyncEnumerable<string> values);

    [OperationContract(Name = "DuplicateDuplexStreaming2")]
    IAsyncEnumerable<int> DuplicateDuplexStreaming(IAsyncEnumerable<int> values);

    [OperationContract]
    Task<(int Value, IAsyncEnumerable<int> Stream, int Count)> DuplexStreamingWithHeadersTask(IAsyncEnumerable<int> values, CancellationToken token);

    [OperationContract]
    ValueTask<(IAsyncEnumerable<int> Stream, int Count)> DuplexStreamingWithHeadersValueTask(IAsyncEnumerable<int> values, int value, int count, CancellationToken token);
}