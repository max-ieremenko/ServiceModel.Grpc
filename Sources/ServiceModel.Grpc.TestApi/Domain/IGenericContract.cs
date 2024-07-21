﻿// <copyright>
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

namespace ServiceModel.Grpc.TestApi.Domain;

[ServiceContract]
public interface IGenericContract<in T1, T2>
{
    [OperationContract]
    T2 Invoke(T1 value, T2 value2);

    // [OperationContract] => BlockingCallAsync
    T2 BlockingCall(T1 value, T2 value2);

    [OperationContract]
    Task<T2> BlockingCallAsync(T1 value, T2 value2, CancellationToken token);
}