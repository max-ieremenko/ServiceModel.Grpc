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

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Grpc.Core;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace ServiceModel.Grpc.Client.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CallContextExtensions
{
    public static bool ContainsResponse(CallContext context) => context.ServerResponse.HasValue;

    public static void SetResponse(CallContext context, Metadata responseHeaders, Status responseStatus, Metadata responseTrailers) =>
        context.ServerResponse = new ServerResponse(responseHeaders, responseStatus, responseTrailers);

    public static void SetResponse(CallContext context, Metadata responseHeaders, Func<Status> getResponseStatus, Func<Metadata> getResponseTrailers) =>
        context.ServerResponse = new ServerResponse(responseHeaders, getResponseStatus, getResponseTrailers);

    public static CallContext WithClientStreamingTracer(CallContext context, Action<Task> tracer)
    {
        context.TraceClientStreaming = tracer;
        return context;
    }

    public static void TraceClientStreaming(CallContext? context, Task writerTask) => context?.TraceClientStreaming?.Invoke(writerTask);
}