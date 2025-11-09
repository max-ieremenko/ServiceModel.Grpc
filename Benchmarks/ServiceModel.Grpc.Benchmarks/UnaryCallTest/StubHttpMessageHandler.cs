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

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly byte[] _responsePayload;
    private readonly string _responseStatus;

    public StubHttpMessageHandler(byte[] responsePayload)
        : this(responsePayload, StatusCode.OK)
    {
    }

    private StubHttpMessageHandler(byte[] responsePayload, StatusCode responseStatus)
    {
        _responsePayload = responsePayload;
        _responseStatus = responseStatus.ToString("D");
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await HttpMessage.ReadAsync(request.Content!, cancellationToken).ConfigureAwait(false);

        return HttpMessage.CreateResponse(_responsePayload, _responseStatus);
    }
}