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

using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi.Domain;

[ServiceContract]
public interface IFilteredService
{
    [OperationContract]
    ValueTask<IList<string>> UnaryAsync(IList<string> input);

    [OperationContract]
    ValueTask<IList<string>> ClientStreamAsync(IAsyncEnumerable<int> stream, IList<string> input);

    [OperationContract]
    ValueTask<(IAsyncEnumerable<int> Stream, IList<string> Output)> ServerStreamAsync(IList<string> input);

    [OperationContract]
    ValueTask<(IAsyncEnumerable<int> Stream, IList<string> Output)> DuplexStreamAsync(IAsyncEnumerable<int> stream, IList<string> input);
}