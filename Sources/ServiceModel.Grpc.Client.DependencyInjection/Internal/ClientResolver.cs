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
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientResolver<TContract> : IClientResolver
    where TContract : class
{
    public Action<ServiceModelGrpcClientOptions, IServiceProvider>? Configure { get; set; }

    public IClientBuilder<TContract>? Builder { get; set; }

    public IChannelProvider? Channel { get; set; }

    public static TContract Resolve(IServiceProvider provider, CallInvoker invoker)
    {
        GrpcPreconditions.CheckNotNull(provider, nameof(provider));
        GrpcPreconditions.CheckNotNull(invoker, nameof(invoker));

        var factory = provider.GetRequiredService<IClientFactory>();
        return factory.CreateClient<TContract>(invoker);
    }

    public TContract Resolve(IServiceProvider provider)
    {
        GrpcPreconditions.CheckNotNull(provider, nameof(provider));

        // first resolve the factory: invoke lazy initializations
        var factory = provider.GetRequiredService<IClientFactory>();
        var invoker = (Channel ?? ChannelProviderFactory.Default()).GetCallInvoker(provider);

        return factory.CreateClient<TContract>(invoker);
    }

    public void Setup(IServiceProvider provider, IClientFactory clientFactory)
    {
        if (Configure == null && Builder == null)
        {
            return;
        }

        Action<ServiceModelGrpcClientOptions>? configure = null;
        if (Configure != null)
        {
            configure = options => Configure(options, provider);
        }

        if (Builder == null)
        {
            clientFactory.AddClient<TContract>(configure);
        }
        else
        {
            clientFactory.AddClient(Builder, configure);
        }
    }
}