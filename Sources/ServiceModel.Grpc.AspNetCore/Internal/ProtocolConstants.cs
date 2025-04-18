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

using System.Net.Mime;

namespace ServiceModel.Grpc.AspNetCore.Internal;

internal static class ProtocolConstants
{
    public const string MediaTypeNameGrpc = "application/grpc";
    public const string MediaTypeNameSwaggerRequest = MediaTypeNames.Application.Json + "+servicemodel.grpc";
    public const string MediaTypeNameSwaggerResponse = MediaTypeNames.Application.Json;

    public const string HeaderGrpcStatus = "grpc-status";
    public const string HeaderGrpcMessage = "grpc-message";

    public const string Http2 = "HTTP/2";

    public static string? NormalizeRelativePath(string? path)
    {
        if (!string.IsNullOrEmpty(path) && path[0] == '/')
        {
            return path.TrimStart('/');
        }

        return path;
    }
}