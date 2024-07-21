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

namespace ServiceModel.Grpc.Client.Internal;

internal readonly struct ServerResponse
{
    private readonly Status _responseStatus;
    private readonly Metadata? _responseTrailers;
    private readonly Func<Status>? _getResponseStatus;
    private readonly Func<Metadata>? _getResponseTrailers;

    public ServerResponse(Metadata responseHeaders, Status responseStatus, Metadata responseTrailers)
    {
        ResponseHeaders = GrpcPreconditions.CheckNotNull(responseHeaders, nameof(responseHeaders));
        _responseStatus = responseStatus;
        _responseTrailers = responseTrailers;

        _getResponseStatus = default;
        _getResponseTrailers = default;
    }

    public ServerResponse(Metadata responseHeaders, Func<Status> getResponseStatus, Func<Metadata> getResponseTrailers)
    {
        ResponseHeaders = GrpcPreconditions.CheckNotNull(responseHeaders, nameof(responseHeaders));
        _getResponseStatus = GrpcPreconditions.CheckNotNull(getResponseStatus, nameof(getResponseStatus));
        _getResponseTrailers = GrpcPreconditions.CheckNotNull(getResponseTrailers, nameof(getResponseTrailers));

        _responseStatus = default;
        _responseTrailers = default;
    }

    public Metadata ResponseHeaders { get; }

    public Status ResponseStatus
    {
        get
        {
            if (_getResponseStatus == null)
            {
                return _responseStatus;
            }

            return _getResponseStatus();
        }
    }

    public Metadata ResponseTrailers
    {
        get
        {
            if (_getResponseTrailers == null)
            {
                return _responseTrailers!;
            }

            return _getResponseTrailers();
        }
    }
}