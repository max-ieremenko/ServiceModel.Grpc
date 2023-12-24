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
using Grpc.Core;

namespace ServiceModel.Grpc.Client.DependencyInjection;

/// <summary>
/// An abstraction for a component that can provide <see cref="CallInvoker"/> instance for gRPC client calls.
/// </summary>
public interface IChannelProvider
{
    /// <summary>
    /// Provides <see cref="CallInvoker"/> instance from the current <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">An instance of current <see cref="IServiceProvider"/>.</param>
    /// <returns>An instance of <see cref="CallInvoker"/>.</returns>
    /// <see cref="ChannelProviderFactory"/>
    CallInvoker GetCallInvoker(IServiceProvider serviceProvider);
}