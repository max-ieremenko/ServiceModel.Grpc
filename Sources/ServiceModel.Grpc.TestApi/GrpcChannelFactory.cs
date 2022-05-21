// <copyright>
// Copyright 2022 Max Ieremenko
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
using Grpc.Net.Client;
using GrpcCoreChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.TestApi;

public static class GrpcChannelFactory
{
    public static ChannelBase CreateChannel(GrpcChannelType channelType, string host, int port)
    {
        if (channelType == GrpcChannelType.GrpcCore)
        {
            return new GrpcCoreChannel(host, port, ChannelCredentials.Insecure);
        }

        return GrpcChannel.ForAddress("http://{0}:{1}".FormatWith(host, port));
    }
}