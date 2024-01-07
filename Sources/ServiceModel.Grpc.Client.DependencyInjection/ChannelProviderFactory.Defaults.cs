// <copyright>
// Copyright 2023-2024 Max Ieremenko
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
using ServiceModel.Grpc.Client.DependencyInjection.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection;

public partial class ChannelProviderFactory
{
    private static void AssertNotNull(object? instance, string name)
    {
        if (instance == null)
        {
            throw new InvalidOperationException($"A null instance of {name} was returned by the configured call channel provider.");
        }
    }

    private sealed class TransientProvider : IChannelProvider
    {
        private readonly Func<IServiceProvider, object?, CallInvoker> _provider;

        public TransientProvider(Func<IServiceProvider, CallInvoker> provider)
            : this((p, _) => provider(p))
        {
        }

        public TransientProvider(Func<IServiceProvider, object?, CallInvoker> provider)
        {
            _provider = provider;
        }

        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider, object? serviceKey)
        {
            GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

            var result = _provider(serviceProvider, serviceKey);
            AssertNotNull(result, nameof(CallInvoker));

            return result;
        }
    }

    private sealed class TransientChannelProvider : IChannelProvider
    {
        private readonly Func<IServiceProvider, object?, ChannelBase> _provider;

        public TransientChannelProvider(Func<IServiceProvider, ChannelBase> provider)
            : this((p, _) => provider(p))
        {
        }

        public TransientChannelProvider(Func<IServiceProvider, object?, ChannelBase> provider)
        {
            _provider = provider;
        }

        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider, object? serviceKey)
        {
            GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

            var channel = _provider(serviceProvider, serviceKey);
            AssertNotNull(channel, nameof(ChannelBase));

            var result = channel.CreateCallInvoker();
            AssertNotNull(result, nameof(CallInvoker));

            return result;
        }
    }

    private sealed class SingletonProvider(CallInvoker instance) : IChannelProvider
    {
        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider, object? serviceKey) => instance;
    }

    private sealed class SingletonChannelProvider(ChannelBase channel) : IChannelProvider
    {
        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider, object? serviceKey)
        {
            var result = channel.CreateCallInvoker();
            AssertNotNull(result, nameof(CallInvoker));

            return result;
        }
    }

    private sealed class DefaultProvider : IChannelProvider
    {
        public static readonly DefaultProvider Instance = new();

        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider, object? serviceKey)
        {
            GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

            var channel = serviceProvider.TryResolve<ChannelBase>(serviceKey);
            if (channel != null)
            {
                var invoker = channel.CreateCallInvoker();
                AssertNotNull(invoker, nameof(CallInvoker));
                return invoker;
            }

            var result = serviceProvider.TryResolve<CallInvoker>(serviceKey);
            if (result == null)
            {
                throw new InvalidOperationException($"Fail to resolve default CallInvoker: neither CallInvoker nor ChannelBase are not registered with key [{serviceKey}].");
            }

            return result;
        }
    }
}