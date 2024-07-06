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

namespace ServiceModel.Grpc;

/// <summary>
/// Provides set of helpers.
/// </summary>
public static class GrpcChannelExtensions
{
    private const string SwitchHttp2UnencryptedSupport = "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport";

    /// <summary>
    /// Gets or sets a value indicating whether the switch is enabled to be able to use HTTP/2 without TLS with HttpClient.
    /// https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.1#call-insecure-grpc-services-with-net-core-client.
    /// </summary>
    public static bool Http2UnencryptedSupport
    {
        get => AppContext.TryGetSwitch(SwitchHttp2UnencryptedSupport, out var value) && value;
        set => AppContext.SetSwitch(SwitchHttp2UnencryptedSupport, value);
    }
}