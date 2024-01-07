// <copyright>
// Copyright 2024 Max Ieremenko
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

using System.Collections.Generic;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientFactoryOptions
{
    public List<IClientOptions> Clients { get; } = new(0);

    public IChannelProvider? Channel { get; set; }

    public ClientOptions<TContract>? FindClient<TContract>()
        where TContract : class
    {
        for (var i = 0; i < Clients.Count; i++)
        {
            if (Clients[i] is ClientOptions<TContract> client)
            {
                return client;
            }
        }

        return null;
    }

    public ClientOptions<TContract> FindOrCreateClient<TContract>()
        where TContract : class
    {
        var result = FindClient<TContract>();
        if (result == null)
        {
            result = new ClientOptions<TContract>();
            Clients.Add(result);
        }

        return result;
    }
}