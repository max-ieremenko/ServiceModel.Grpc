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
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.Internal;

internal static class ClientChannelAdapter
{
    internal static async Task WaitForServerStreamExceptionAsync<THeader, TResponse>(
        IAsyncStreamReader<TResponse> responseStream,
        Metadata? responseHeaders,
        Marshaller<THeader> marshaller,
        CancellationToken token)
    {
        // here should throw the RpcException from server-side
        // see ExceptionHandlingTestBase ThrowApplicationExceptionServerStreamingHeader and ThrowApplicationExceptionDuplexStreamingHeader
        await responseStream.MoveNext(token).ConfigureAwait(false);

        // this line should not be reached
        // if the case, check for the headers content
        CompatibilityTools.DeserializeMethodOutputHeader(marshaller, responseHeaders);

        throw new InvalidOperationException("The server streaming ResponseHeadersAsync did not provide any headers, headers are available only after the streaming.");
    }

    internal static CallOptions AddRequestHeader<THeader>(in CallOptions callOptions, Marshaller<THeader>? marshaller, THeader? header)
    {
        if (marshaller == null)
        {
            return callOptions;
        }

        var metadata = CompatibilityTools.SerializeMethodInputHeader(marshaller!, header);
        return CallOptionsBuilder.MergeCallOptions(callOptions, new CallOptions(metadata));
    }
}