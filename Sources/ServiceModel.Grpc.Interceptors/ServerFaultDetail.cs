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
public struct ServerFaultDetail
{
    /// <summary>
    /// Gets or sets the the gRPC status code, <see cref="Status"/>.
    /// </summary>
    public StatusCode? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the the optional gRPC error message, <see cref="Status"/>.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the the optional detail of error to pass for a client call.
    /// </summary>
    public object? Detail { get; set; }

    /// <summary>
    /// Gets or sets the optional call trailing metadata, <see cref="RpcException"/>.
    /// </summary>
    public Metadata? Trailers { get; set; }
}