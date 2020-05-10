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

using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi.Domain
{
    [ServiceContract]
    public interface IMultipurposeService
    {
        [OperationContract]
        string Concat(string value, CallContext context = default);

        [OperationContract]
        Task<string> ConcatAsync(string value, CallContext context = default);

        [OperationContract]
        ValueTask<long> Sum5ValuesAsync(long x1, int x2, int x3, int x4, int x5, CancellationToken token);

        [OperationContract]
        IAsyncEnumerable<string> RepeatValue(string value, int count, CallContext context = default);

        [OperationContract]
        Task<IAsyncEnumerable<string>> RepeatValueAsync(string value, int count, CallContext context = default);

        [OperationContract]
        Task<long> SumValues(IAsyncEnumerable<int> values, CallContext context = default);

        [OperationContract]
        Task<long> MultiplyByAndSumValues(IAsyncEnumerable<int> values, int multiplier, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<string> ConvertValues(IAsyncEnumerable<int> values, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<int> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CallContext context = default);

        [OperationContract]
        ValueTask<IAsyncEnumerable<int>> MultiplyByAsync(IAsyncEnumerable<int> values, int multiplier, CallContext context = default);
    }
}
