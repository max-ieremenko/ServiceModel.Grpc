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
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

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

    private sealed class TransientProvider(Func<IServiceProvider, CallInvoker> provider) : IChannelProvider
    {
        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider)
        {
            GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

            var result = provider(serviceProvider);
            AssertNotNull(result, nameof(CallInvoker));

            return result;
        }
    }

    private sealed class TransientChannelProvider(Func<IServiceProvider, ChannelBase> provider) : IChannelProvider
    {
        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider)
        {
            GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

            var channel = provider(serviceProvider);
            AssertNotNull(channel, nameof(ChannelBase));

            var result = channel.CreateCallInvoker();
            AssertNotNull(result, nameof(CallInvoker));

            return result;
        }
    }

    private sealed class SingletonProvider(CallInvoker instance) : IChannelProvider
    {
        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider) => instance;
    }

    private sealed class SingletonChannelProvider(ChannelBase channel) : IChannelProvider
    {
        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider)
        {
            var result = channel.CreateCallInvoker();
            AssertNotNull(result, nameof(CallInvoker));

            return result;
        }
    }

    private sealed class DefaultProvider : IChannelProvider
    {
        public static readonly DefaultProvider Instance = new();

        public CallInvoker GetCallInvoker(IServiceProvider serviceProvider)
        {
            GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

            var channel = serviceProvider.GetService<ChannelBase>();
            if (channel != null)
            {
                var invoker = channel.CreateCallInvoker();
                AssertNotNull(invoker, nameof(CallInvoker));
                return invoker;
            }

            var result = serviceProvider.GetService<CallInvoker>();
            if (result == null)
            {
                throw new InvalidOperationException("Fail to resolve default CallInvoker: neither CallInvoker nor ChannelBase are not registered.");
            }

            return result;
        }
    }
}