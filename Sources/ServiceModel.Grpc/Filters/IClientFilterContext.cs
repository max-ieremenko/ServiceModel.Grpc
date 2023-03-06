// <copyright>
// Copyright 2023 Max Ieremenko
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
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;

namespace ServiceModel.Grpc.Filters;

/// <summary>
/// A context for client filters, specifically <see cref="IClientFilter.InvokeAsync"/> and <see cref="IClientFilter.Invoke"/>.
/// </summary>
public interface IClientFilterContext
{
    /// <summary>
    /// Gets the <see cref="IMethod"/> instance representing the method to be invoked.
    /// </summary>
    IMethod Method { get; }

    /// <summary>
    /// Gets an instance of <see cref="CallOptions" /> representing the gRPC context of the invocation.
    /// </summary>
    CallOptions CallOptions { get; }

    /// <summary>
    /// Gets the current service provider.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets a dictionary that can be used by the various handlers of this call to store arbitrary state.
    /// </summary>
    IDictionary<object, object> UserState { get; }

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> for the contract method declaration.
    /// </summary>
    MethodInfo ContractMethodInfo { get; }

    /// <summary>
    /// Gets the control of the incoming request.
    /// </summary>
    IRequestContext Request { get; }

    /// <summary>
    /// Gets the control of the outgoing response.
    /// </summary>
    IResponseContext Response { get; }
}