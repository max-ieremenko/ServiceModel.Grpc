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
using Grpc.Core.Utils;

namespace ServiceModel.Grpc.Client.DependencyInjection;

/// <summary>
/// The factory that can create an <see cref="IChannelProvider"/> in most common cases.
/// </summary>
public static partial class ChannelProviderFactory
{
    /// <summary>
    /// Creates an <see cref="IChannelProvider"/> that will be used for resolving <see cref="CallInvoker"/> from the current <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="provider">A delegate that is used for resolving <see cref="CallInvoker"/>.</param>
    /// <returns>An <see cref="IChannelProvider"/> that can be used for resolving <see cref="CallInvoker"/>.</returns>
    public static IChannelProvider Transient(Func<IServiceProvider, CallInvoker> provider)
    {
        GrpcPreconditions.CheckNotNull(provider, nameof(provider));

        return new TransientProvider(provider);
    }

    /// <summary>
    /// Creates an <see cref="IChannelProvider"/> that will be used for resolving <see cref="ChannelBase"/> from the current <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="provider">A delegate that is used for resolving <see cref="ChannelBase"/>.</param>
    /// <returns>An <see cref="IChannelProvider"/> that can be used for resolving <see cref="ChannelBase"/>.</returns>
    public static IChannelProvider Transient(Func<IServiceProvider, ChannelBase> provider)
    {
        GrpcPreconditions.CheckNotNull(provider, nameof(provider));

        return new TransientChannelProvider(provider);
    }

    /// <summary>
    /// Creates an <see cref="IChannelProvider"/> that will provide an instance of <see cref="CallInvoker"/>.
    /// </summary>
    /// <param name="callInvoker">An instance of <see cref="CallInvoker"/>.</param>
    /// <returns>An <see cref="IChannelProvider"/> that can be used for resolving <see cref="CallInvoker"/>.</returns>
    public static IChannelProvider Singleton(CallInvoker callInvoker)
    {
        GrpcPreconditions.CheckNotNull(callInvoker, nameof(callInvoker));

        return new SingletonProvider(callInvoker);
    }

    /// <summary>
    /// Creates an <see cref="IChannelProvider"/> that will provide an instance of <see cref="ChannelBase"/>.
    /// </summary>
    /// <param name="channel">An instance of <see cref="ChannelBase"/>.</param>
    /// <returns>An <see cref="IChannelProvider"/> that can be used for resolving <see cref="ChannelBase"/>.</returns>
    public static IChannelProvider Singleton(ChannelBase channel)
    {
        GrpcPreconditions.CheckNotNull(channel, nameof(channel));

        return new SingletonChannelProvider(channel);
    }

    internal static IChannelProvider Default() => DefaultProvider.Instance;
}