﻿// <copyright>
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
/// Carries along the context associated with intercepted invocations on the server side.
/// </summary>
public readonly struct ServerCallInterceptorContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerCallInterceptorContext"/> struct with the specified <see cref="ServerCallContext"/>.
    /// </summary>
    /// <param name="serverCallContext">A <see cref="ServerCallContext"/> instance containing the context of the current call.</param>
    public ServerCallInterceptorContext(ServerCallContext serverCallContext)
    {
        ServerCallContext = serverCallContext;
    }

    /// <summary>
    /// Gets the <see cref="ServerCallContext"/> instance containing the context of the current call.
    /// </summary>
    public ServerCallContext ServerCallContext { get; }
}