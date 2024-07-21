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

using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// Carries along the context associated with intercepted invocations on the client side.
/// </summary>
public readonly struct ClientCallInterceptorContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCallInterceptorContext"/> struct with the specified method, host, and call options.
    /// </summary>
    /// <param name="callOptions">A <see cref="CallOptions"/> instance containing the call options of the current call.</param>
    /// <param name="host">The host to dispatch the current call to.</param>
    /// <param name="method">A <see cref="IMethod"/> object representing the method to be invoked.</param>
    public ClientCallInterceptorContext(CallOptions callOptions, string? host, IMethod method)
    {
        CallOptions = callOptions;
        Host = host;
        Method = method;
    }

    /// <summary>
    /// Gets the <see cref="CallOptions"/> structure representing the call options associated with the current invocation.
    /// </summary>
    public CallOptions CallOptions { get; }

    /// <summary>
    /// Gets the host to dispatch the current call to.
    /// </summary>
    public string? Host { get; }

    /// <summary>
    /// Gets the <see cref="IMethod"/> instance representing the method to be invoked.
    /// </summary>
    public IMethod Method { get; }
}