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

using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// Contains the detail information of the fault condition.
/// </summary>
public readonly struct ClientFaultDetail
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientFaultDetail"/> struct with the specified error and detail.
    /// </summary>
    /// <param name="originalError">The original <see cref="RpcException"/> raised by <see cref="CallInvoker"/>.</param>
    /// <param name="detail">The error detail provided by <see cref="IServerErrorHandler"/>.</param>
    public ClientFaultDetail(RpcException originalError, object? detail)
    {
        OriginalError = originalError;
        Detail = detail;
    }

    /// <summary>
    /// Gets the original <see cref="RpcException"/> raised by <see cref="CallInvoker"/>.
    /// </summary>
    public RpcException OriginalError { get; }

    /// <summary>
    /// Gets the error detail provided by <see cref="IServerErrorHandler"/>.
    /// </summary>
    public object? Detail { get; }
}